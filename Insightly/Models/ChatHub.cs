using Insightly.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Insightly.Models
{
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

      
        public async Task SendPrivateMessage(string receiverId, string message)
        {
            var senderId = Context.UserIdentifier;

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
            await Clients.Caller.SendAsync("ReceiveMessage", senderId, message);
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);

            var newMessage = new ChatMessage
            {
                SenderId = senderId,
                Message = message,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                ChatId = chat.Id 
            };

            chat.Messages.Add(newMessage);
            await _unitOfWork.SaveChangesAsync();

           
          
        }

        
        public async Task LoadChatMessages(string otherUserId)
        {
            var userId = Context.UserIdentifier;
            var messages = await _unitOfWork.Chats.GetAllMessages(userId, otherUserId);
            await Clients.Caller.SendAsync("LoadChatHistory", messages);
        }
    }
}
