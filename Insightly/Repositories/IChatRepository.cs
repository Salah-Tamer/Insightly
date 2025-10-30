using Insightly.Models;

namespace Insightly.Repositories
{
    public interface IChatRepository
    {
        Task<IEnumerable<ChatMessage>>GetAllMessages(string SenderId, string ReceiverId);
        Task AddMessge(ChatMessage message);
        Task<IEnumerable<Chat>> GetChats(string UserId);
        Task<Chat?> GetChatBetweenUsers(string SenderId, string ReceiverId);
        Task AddChat(Chat chat);

    }
}
