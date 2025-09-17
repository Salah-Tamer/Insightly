using Insightly.Models;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Article)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task<Comment?> GetByIdWithAuthorAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.CommentId == id);
        }

        public async Task<IEnumerable<Comment>> GetByArticleIdAsync(int articleId)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.ArticleId == articleId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(string authorId)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Article)
                .Where(c => c.AuthorId == authorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment> AddAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Comments.AnyAsync(c => c.CommentId == id);
        }

        public async Task<int> GetCountByArticleAsync(int articleId)
        {
            return await _context.Comments.CountAsync(c => c.ArticleId == articleId);
        }

        public async Task<int> GetCountByAuthorAsync(string authorId)
        {
            return await _context.Comments.CountAsync(c => c.AuthorId == authorId);
        }
    }
}
