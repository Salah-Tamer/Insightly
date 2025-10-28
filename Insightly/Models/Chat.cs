using Insightly.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using NuGet.Protocol.Plugins;

namespace Insightly.Models
{
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public ChatHub(IUnitOfWork unitOfWork) {

            _unitOfWork = unitOfWork;
        }
        public async Task SendPrivateMessage(string ReceiverId , string Message)
        {
            var SenderId = Context.UserIdentifier;
            await Clients.Caller.SendAsync("ReceiveMessage", SenderId, Message);
            await Clients.User(ReceiverId).SendAsync("ReceiveMessage",SenderId,Message);
            var msg = new ChatMessage { 
                
                SenderId = SenderId, 
                Message = Message,
                ReceiverId = ReceiverId
            };
            await _unitOfWork.Chats.AddMessge(msg);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task LoadChatMessages(string otherUserId)
        {
            var userId = Context.UserIdentifier;    
            var messages = await _unitOfWork.Chats.GetAllMessages(userId,otherUserId);
            await Clients.Caller.SendAsync("LoadChatHistory", messages);
        }

    }
}
