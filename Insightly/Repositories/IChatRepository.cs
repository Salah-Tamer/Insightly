using Insightly.Models;
using Insightly.ViewModels;

namespace Insightly.Repositories
{
    public interface IChatRepository
    {
        Task<IEnumerable<MessageViewModel>> GetAllMessages(string SenderId, string ReceiverId);
        Task AddMessge(ChatMessage message);
        Task<IEnumerable<Chat>> GetChats(string UserId);
        Task<Chat?> GetChatBetweenUsers(string SenderId, string ReceiverId);
        Task AddChat(Chat chat);

    }
}
