using Insightly.Models;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly AppDbContext _context;

        public ArticleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Article>> GetAllAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Article?> GetByIdAsync(int id)
        {
            return await _context.Articles.FindAsync(id);
        }

        public async Task<Article?> GetByIdWithAuthorAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
        }

        public async Task<Article?> GetByIdWithCommentsAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
        }

        public async Task<Article?> GetByIdWithAuthorAndCommentsAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Comments)
                .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
        }

        public async Task<IEnumerable<Article>> GetByAuthorIdAsync(string authorId)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Where(a => a.AuthorId == authorId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetByFollowingUsersAsync(string userId)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Where(a => _context.Follows
                    .Any(f => f.FollowerId == userId && f.FollowingId == a.AuthorId))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetLatestAsync(int count)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetLatestAsync(int skip, int take)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllAsync();
            }

            var searchTerm = query.Trim().ToLower();
            return await _context.Articles
                .Include(a => a.Author)
                .Where(a => 
                    a.Title.ToLower().Contains(searchTerm) ||
                    a.Content.ToLower().Contains(searchTerm) ||
                    a.Author.Name.ToLower().Contains(searchTerm))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Article> AddAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article> UpdateAsync(Article article)
        {
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task DeleteAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Articles.AnyAsync(a => a.ArticleId == id);
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Articles.CountAsync();
        }

        public async Task<int> GetCountByAuthorAsync(string authorId)
        {
            return await _context.Articles.CountAsync(a => a.AuthorId == authorId);
        }
    }
}
