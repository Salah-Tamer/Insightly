using Insightly.Models;

namespace Insightly.Repositories
{
    public interface IArticleRepository
    {
        Task<IEnumerable<Article>> GetAllAsync();
        Task<Article?> GetByIdAsync(int id);
        Task<Article?> GetByIdWithAuthorAsync(int id);
        Task<Article?> GetByIdWithCommentsAsync(int id);
        Task<Article?> GetByIdWithAuthorAndCommentsAsync(int id);
        Task<IEnumerable<Article>> GetByAuthorIdAsync(string authorId);
        Task<IEnumerable<Article>> GetByFollowingUsersAsync(string userId);
        Task<IEnumerable<Article>> GetLatestAsync(int count);
        Task<IEnumerable<Article>> GetLatestAsync(int skip, int take);
        Task<IEnumerable<Article>> SearchAsync(string query);
        Task<Article> AddAsync(Article article);
        Task<Article> UpdateAsync(Article article);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetCountAsync();
        Task<int> GetCountByAuthorAsync(string authorId);
    }
}
