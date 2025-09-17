using Insightly.Models;

namespace Insightly.Repositories
{
    public interface IVoteRepository
    {
        Task<Vote?> GetByUserAndArticleAsync(string userId, int articleId);
        Task<IEnumerable<Vote>> GetByArticleIdAsync(int articleId);
        Task<IEnumerable<Vote>> GetByUserIdAsync(string userId);
        Task<Vote> AddAsync(Vote vote);
        Task<Vote> UpdateAsync(Vote vote);
        Task DeleteAsync(int voteId);
        Task DeleteByUserAndArticleAsync(string userId, int articleId);
        Task<bool> ExistsAsync(string userId, int articleId);
        Task<int> GetNetScoreAsync(int articleId);
        Task<int> GetUpvoteCountAsync(int articleId);
        Task<int> GetDownvoteCountAsync(int articleId);
    }
}
