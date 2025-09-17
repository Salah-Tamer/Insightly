using Insightly.Models;

namespace Insightly.Repositories
{
    public interface IFollowRepository
    {
        Task<IEnumerable<Follow>> GetAllAsync();
        Task<Follow?> GetByIdAsync(int id);
        Task<Follow?> GetByFollowerAndFollowingAsync(string followerId, string followingId);
        Task<IEnumerable<Follow>> GetFollowersAsync(string userId);
        Task<IEnumerable<Follow>> GetFollowingAsync(string userId);
        Task<Follow> AddAsync(Follow follow);
        Task DeleteAsync(int id);
        Task DeleteByFollowerAndFollowingAsync(string followerId, string followingId);
        Task<bool> ExistsAsync(string followerId, string followingId);
        Task<int> GetFollowersCountAsync(string userId);
        Task<int> GetFollowingCountAsync(string userId);
    }
}
