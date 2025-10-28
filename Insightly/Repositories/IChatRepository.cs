using Insightly.Models;

namespace Insightly.Repositories
{
    public interface IChatRepository
    {
        Task<IEnumerable<ChatMessage>>GetAllMessages(string SenderId, string ReceiverId);
        Task AddMessge(ChatMessage message);

    }
}
