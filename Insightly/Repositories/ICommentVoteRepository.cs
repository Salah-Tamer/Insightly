using Insightly.Models;

namespace Insightly.Repositories
{
    public interface ICommentVoteRepository
    {
        Task<CommentVote?> GetByUserAndCommentAsync(string userId, int commentId);
        Task<IEnumerable<CommentVote>> GetByCommentIdAsync(int commentId);
        Task<IEnumerable<CommentVote>> GetByUserIdAsync(string userId);
        Task<CommentVote> AddAsync(CommentVote commentVote);
        Task<CommentVote> UpdateAsync(CommentVote commentVote);
        Task DeleteAsync(int commentVoteId);
        Task DeleteByUserAndCommentAsync(string userId, int commentId);
        Task<bool> ExistsAsync(string userId, int commentId);
        Task<int> GetNetScoreAsync(int commentId);
        Task<int> GetUpvoteCountAsync(int commentId);
        Task<int> GetDownvoteCountAsync(int commentId);
    }
}
