using Insightly.Models;

namespace Insightly.Services
{
    public interface ICommentService
    {
        Task<(bool Success, string? ErrorMessage, Comment? Comment)> AddCommentAsync(int articleId, string content, string authorId);
        Task<(bool Success, string? ErrorMessage, Comment? Comment)> UpdateCommentAsync(int commentId, string content, string userId);
        Task<(bool Success, string? ErrorMessage)> DeleteCommentAsync(int commentId, string userId, bool isAdmin);
    }
}


