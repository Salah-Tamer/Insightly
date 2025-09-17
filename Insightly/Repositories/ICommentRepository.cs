using Insightly.Models;

namespace Insightly.Repositories
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        Task<Comment?> GetByIdWithAuthorAsync(int id);
        Task<IEnumerable<Comment>> GetByArticleIdAsync(int articleId);
        Task<IEnumerable<Comment>> GetByAuthorIdAsync(string authorId);
        Task<Comment> AddAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetCountByArticleAsync(int articleId);
        Task<int> GetCountByAuthorAsync(string authorId);
    }
}
