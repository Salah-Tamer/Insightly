using Insightly.Models;
using Insightly.Repositories;

namespace Insightly.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IFollowRepository _followRepository;

        public ChatService(IChatRepository chatRepository, IFollowRepository followRepository)
        {
            _chatRepository = chatRepository;
            _followRepository = followRepository;
        }

        public async Task<(IEnumerable<Chat> Chats, IEnumerable<Follow> Followers, IEnumerable<Follow> Following)> GetUserChatDataAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (Enumerable.Empty<Chat>(), Enumerable.Empty<Follow>(), Enumerable.Empty<Follow>());
            }

            var chats = await _chatRepository.GetChats(userId);
            var followers = await _followRepository.GetFollowersAsync(userId);
            var following = await _followRepository.GetFollowingAsync(userId);

            var validChats = chats.Where(c => c != null && c.User != null && c.OtherUser != null);
            var validFollowers = followers.Where(f => f != null && f.Follower != null);
            var validFollowing = following.Where(f => f != null && f.Following != null);

            return (validChats, validFollowers, validFollowing);
        }
    }
}



