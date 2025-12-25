using Insightly.Models;

namespace Insightly.Services
{
    public interface IVoteService
    {
        Task<(bool Success, bool Removed, string Message)> ToggleArticleVoteAsync(int articleId, string userId, bool isUpvote);
        Task<(bool Success, bool Removed, string Message)> ToggleCommentVoteAsync(int commentId, string userId, bool isUpvote);
        Task<int> GetArticleNetScoreAsync(int articleId);
        Task<int> GetCommentNetScoreAsync(int commentId);
        Task<(bool Voted, bool? IsUpvote)> GetUserArticleVoteAsync(int articleId, string userId);
        Task<(bool Voted, bool? IsUpvote)> GetUserCommentVoteAsync(int commentId, string userId);
    }
}

