using Insightly.Models;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Repositories
{
    public class CommentVoteRepository : ICommentVoteRepository
    {
        private readonly AppDbContext _context;

        public CommentVoteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CommentVote?> GetByUserAndCommentAsync(string userId, int commentId)
        {
            return await _context.CommentVotes
                .FirstOrDefaultAsync(cv => cv.UserId == userId && cv.CommentId == commentId);
        }

        public async Task<IEnumerable<CommentVote>> GetByCommentIdAsync(int commentId)
        {
            return await _context.CommentVotes
                .Where(cv => cv.CommentId == commentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommentVote>> GetByUserIdAsync(string userId)
        {
            return await _context.CommentVotes
                .Where(cv => cv.UserId == userId)
                .ToListAsync();
        }

        public async Task<CommentVote> AddAsync(CommentVote commentVote)
        {
            _context.CommentVotes.Add(commentVote);
            return commentVote;
        }

        public async Task<CommentVote> UpdateAsync(CommentVote commentVote)
        {
            _context.CommentVotes.Update(commentVote);
            return commentVote;
        }

        public async Task DeleteAsync(int commentVoteId)
        {
            var commentVote = await _context.CommentVotes.FindAsync(commentVoteId);
            if (commentVote != null)
            {
                _context.CommentVotes.Remove(commentVote);
            }
        }

        public async Task DeleteByUserAndCommentAsync(string userId, int commentId)
        {
            var commentVote = await GetByUserAndCommentAsync(userId, commentId);
            if (commentVote != null)
            {
                _context.CommentVotes.Remove(commentVote);
            }
        }

        public async Task<bool> ExistsAsync(string userId, int commentId)
        {
            return await _context.CommentVotes.AnyAsync(cv => cv.UserId == userId && cv.CommentId == commentId);
        }

        public async Task<int> GetNetScoreAsync(int commentId)
        {
            return await _context.CommentVotes
                .Where(cv => cv.CommentId == commentId)
                .SumAsync(cv => cv.IsUpvote ? 1 : -1);
        }

        public async Task<int> GetUpvoteCountAsync(int commentId)
        {
            return await _context.CommentVotes
                .CountAsync(cv => cv.CommentId == commentId && cv.IsUpvote);
        }

        public async Task<int> GetDownvoteCountAsync(int commentId)
        {
            return await _context.CommentVotes
                .CountAsync(cv => cv.CommentId == commentId && !cv.IsUpvote);
        }
    }
}
