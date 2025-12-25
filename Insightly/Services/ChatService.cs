using Insightly.Models;
using Insightly.Repositories;

namespace Insightly.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IEnumerable<Chat> Chats, IEnumerable<ApplicationUser> Followers, IEnumerable<ApplicationUser> Following)> GetUserChatDataAsync(string userId)
        {
            var chats = await _unitOfWork.Chats.GetChats(userId);
            var followers = await _unitOfWork.Follows.GetFollowersAsync(userId);
            var following = await _unitOfWork.Follows.GetFollowingAsync(userId);

            return (chats, followers, following);
        }
    }
}



