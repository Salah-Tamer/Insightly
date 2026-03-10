using Insightly.Models;

namespace Insightly.Services
{
    public interface IChatService
    {
        Task<(IEnumerable<Chat> Chats, IEnumerable<Follow> Followers, IEnumerable<Follow> Following)> GetUserChatDataAsync(string userId);
    }
}

