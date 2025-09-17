using Insightly.Models;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Repositories
{
    public class VoteRepository : IVoteRepository
    {
        private readonly AppDbContext _context;

        public VoteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Vote?> GetByUserAndArticleAsync(string userId, int articleId)
        {
            return await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ArticleId == articleId);
        }

        public async Task<IEnumerable<Vote>> GetByArticleIdAsync(int articleId)
        {
            return await _context.Votes
                .Where(v => v.ArticleId == articleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vote>> GetByUserIdAsync(string userId)
        {
            return await _context.Votes
                .Where(v => v.UserId == userId)
                .ToListAsync();
        }

        public async Task<Vote> AddAsync(Vote vote)
        {
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();
            return vote;
        }

        public async Task<Vote> UpdateAsync(Vote vote)
        {
            _context.Votes.Update(vote);
            await _context.SaveChangesAsync();
            return vote;
        }

        public async Task DeleteAsync(int voteId)
        {
            var vote = await _context.Votes.FindAsync(voteId);
            if (vote != null)
            {
                _context.Votes.Remove(vote);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByUserAndArticleAsync(string userId, int articleId)
        {
            var vote = await GetByUserAndArticleAsync(userId, articleId);
            if (vote != null)
            {
                _context.Votes.Remove(vote);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string userId, int articleId)
        {
            return await _context.Votes.AnyAsync(v => v.UserId == userId && v.ArticleId == articleId);
        }

        public async Task<int> GetNetScoreAsync(int articleId)
        {
            return await _context.Votes
                .Where(v => v.ArticleId == articleId)
                .SumAsync(v => v.IsUpvote ? 1 : -1);
        }

        public async Task<int> GetUpvoteCountAsync(int articleId)
        {
            return await _context.Votes
                .CountAsync(v => v.ArticleId == articleId && v.IsUpvote);
        }

        public async Task<int> GetDownvoteCountAsync(int articleId)
        {
            return await _context.Votes
                .CountAsync(v => v.ArticleId == articleId && !v.IsUpvote);
        }
    }
}
