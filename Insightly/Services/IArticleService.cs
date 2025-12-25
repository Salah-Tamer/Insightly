using Insightly.Models;

namespace Insightly.Services
{
    public interface IArticleService
    {
        Task<(bool Success, string? ErrorMessage, Article? Article)> CreateArticleAsync(string title, string content, string authorId, string? imagePath);
        Task<Article?> GetArticleDetailsAsync(int id, string? userId);
        Task<(bool Success, string? ErrorMessage)> UpdateArticleAsync(int id, string title, string content, string userId);
        Task<(bool Success, string? ErrorMessage)> DeleteArticleAsync(int id, string userId, bool isAdmin);
        Task<(bool Success, bool IsSaved, string Message)> ToggleSaveArticleAsync(int articleId, string userId);
    }
}

