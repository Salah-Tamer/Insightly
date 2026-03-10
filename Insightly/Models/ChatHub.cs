using AutoMapper;
using Insightly.Repositories;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Models
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatRepository chatRepository, IMapper mapper, ILogger<ChatHub> logger)
        {
            _chatRepository = chatRepository;
            _mapper = mapper;
            _logger = logger;
        }

        private const int MaxMessageLength = 2000;

        public async Task SendPrivateMessage(string receiverId, string message)
        {
            try
            {
                var senderId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(senderId))
                {
                    _logger.LogWarning("SendPrivateMessage called with null userId");
                    return;
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("MessageError", "Message cannot be empty.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(receiverId))
                {
                    await Clients.Caller.SendAsync("MessageError", "Invalid recipient.");
                    return;
                }

                message = message.Trim();

                if (message.Length > MaxMessageLength)
                {
                    await Clients.Caller.SendAsync("MessageError", $"Message exceeds the {MaxMessageLength} character limit.");
                    return;
                }

                var chat = await _chatRepository.GetChatBetweenUsers(senderId, receiverId);

                if (chat == null)
                {
                    try
                    {
                        chat = new Chat
                        {
                            UserId = senderId,
                            OtherUserId = receiverId,
                            Messages = new List<ChatMessage>()
                        };
                        await _chatRepository.AddChat(chat);
                    }
                    catch (DbUpdateException)
                    {
                        chat = await _chatRepository.GetChatBetweenUsers(senderId, receiverId);
                        if (chat == null)
                        {
                            _logger.LogError("Failed to create or retrieve chat between {SenderId} and {ReceiverId}", senderId, receiverId);
                            return;
                        }
                    }
                }

                var newMessage = new ChatMessage
                {
                    SenderId = senderId,
                    Message = message,
                    SentAt = DateTime.UtcNow,
                    IsRead = false,
                    ChatId = chat.Id
                };

                await _chatRepository.AddMessge(newMessage);

                await Clients.Caller.SendAsync("ReceiveMessage", senderId, message, newMessage.SentAt.ToString("O"));
                await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message, newMessage.SentAt.ToString("O"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
            }
        }

        public async Task LoadChatMessages(string otherUserId)
        {
            try
            {
                var userId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("LoadChatMessages called with null userId");
                    await Clients.Caller.SendAsync("LoadChatHistory", new List<MessageViewModel>());
                    return;
                }

                var messages = await _chatRepository.GetAllMessages(userId, otherUserId);
                var messageList = _mapper.Map<List<MessageViewModel>>(messages ?? Enumerable.Empty<Insightly.Models.ChatMessage>());

                _logger.LogInformation($"Loading {messageList.Count} messages for chat between {userId} and {otherUserId}");

                await Clients.Caller.SendAsync("LoadChatHistory", messageList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading messages: {ex.Message}");
                await Clients.Caller.SendAsync("LoadChatHistory", new List<MessageViewModel>());
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation($"User connected: {userId ?? "NULL"}");
            await base.OnConnectedAsync();
        }
    }
}