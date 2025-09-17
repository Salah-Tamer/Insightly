using Insightly.Models;

namespace Insightly.Repositories
{
    public interface IArticleReadRepository
    {
        Task<IEnumerable<ArticleRead>> GetAllAsync();
        Task<ArticleRead?> GetByIdAsync(int id);
        Task<ArticleRead?> GetByUserAndArticleAsync(string userId, int articleId);
        Task<IEnumerable<ArticleRead>> GetByUserIdAsync(string userId);
        Task<IEnumerable<ArticleRead>> GetByArticleIdAsync(int articleId);
        Task<ArticleRead> AddAsync(ArticleRead articleRead);
        Task DeleteAsync(int id);
        Task DeleteByUserAndArticleAsync(string userId, int articleId);
        Task<bool> ExistsAsync(string userId, int articleId);
        Task<int> GetCountByUserAsync(string userId);
        Task<int> GetCountByArticleAsync(int articleId);
    }
}
