using Insightly.Models;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Repositories
{
    public class ArticleReadRepository : IArticleReadRepository
    {
        private readonly AppDbContext _context;

        public ArticleReadRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ArticleRead>> GetAllAsync()
        {
            return await _context.ArticleReads
                .Include(ar => ar.Article)
                .Include(ar => ar.User)
                .OrderByDescending(ar => ar.ReadAt)
                .ToListAsync();
        }

        public async Task<ArticleRead?> GetByIdAsync(int id)
        {
            return await _context.ArticleReads.FindAsync(id);
        }

        public async Task<ArticleRead?> GetByUserAndArticleAsync(string userId, int articleId)
        {
            return await _context.ArticleReads
                .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.ArticleId == articleId);
        }

        public async Task<IEnumerable<ArticleRead>> GetByUserIdAsync(string userId)
        {
            return await _context.ArticleReads
                .Include(ar => ar.Article)
                .ThenInclude(a => a.Author)
                .Where(ar => ar.UserId == userId)
                .OrderByDescending(ar => ar.ReadAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticleRead>> GetByArticleIdAsync(int articleId)
        {
            return await _context.ArticleReads
                .Include(ar => ar.User)
                .Where(ar => ar.ArticleId == articleId)
                .OrderByDescending(ar => ar.ReadAt)
                .ToListAsync();
        }

        public async Task<ArticleRead> AddAsync(ArticleRead articleRead)
        {
            _context.ArticleReads.Add(articleRead);
            await _context.SaveChangesAsync();
            return articleRead;
        }

        public async Task DeleteAsync(int id)
        {
            var articleRead = await _context.ArticleReads.FindAsync(id);
            if (articleRead != null)
            {
                _context.ArticleReads.Remove(articleRead);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByUserAndArticleAsync(string userId, int articleId)
        {
            var articleRead = await GetByUserAndArticleAsync(userId, articleId);
            if (articleRead != null)
            {
                _context.ArticleReads.Remove(articleRead);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string userId, int articleId)
        {
            return await _context.ArticleReads.AnyAsync(ar => ar.UserId == userId && ar.ArticleId == articleId);
        }

        public async Task<int> GetCountByUserAsync(string userId)
        {
            return await _context.ArticleReads.CountAsync(ar => ar.UserId == userId);
        }

        public async Task<int> GetCountByArticleAsync(int articleId)
        {
            return await _context.ArticleReads.CountAsync(ar => ar.ArticleId == articleId);
        }
    }
}
