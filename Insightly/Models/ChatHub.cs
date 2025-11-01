using Insightly.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Insightly.ViewModels;

namespace Insightly.Models
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IUnitOfWork unitOfWork, ILogger<ChatHub> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

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

                var chat = await _unitOfWork.Chats.GetChatBetweenUsers(senderId, receiverId);

                if (chat == null)
                {
                    chat = new Chat
                    {
                        UserId = senderId,
                        OtherUserId = receiverId,
                        Messages = new List<ChatMessage>()
                    };
                    await _unitOfWork.Chats.AddChat(chat);
                    await _unitOfWork.SaveChangesAsync();
                }

                var newMessage = new ChatMessage
                {
                    SenderId = senderId,
                    Message = message,
                    SentAt = DateTime.UtcNow,
                    IsRead = false,
                    ChatId = chat.Id
                };

                await _unitOfWork.Chats.AddMessge(newMessage);
                await _unitOfWork.SaveChangesAsync();

                // Send via SignalR - now just send the raw data, not the entity
                await Clients.Caller.SendAsync("ReceiveMessage", senderId, message);
                await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
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

                var messages = await _unitOfWork.Chats.GetAllMessages(userId, otherUserId);
                var messageList = messages?.ToList() ?? new List<MessageViewModel>();

                _logger.LogInformation($"Loading {messageList.Count} messages for chat between {userId} and {otherUserId}");

                // Now sending DTOs instead of entities - no more circular reference!
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