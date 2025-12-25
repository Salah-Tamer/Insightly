using Insightly.Models;

namespace Insightly.Services
{
    public interface IChatService
    {
        Task<(IEnumerable<Chat> Chats, IEnumerable<ApplicationUser> Followers, IEnumerable<ApplicationUser> Following)> GetUserChatDataAsync(string userId);
    }
}

