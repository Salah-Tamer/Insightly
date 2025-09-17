using Insightly.Models;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly AppDbContext _context;

        public FollowRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Follow>> GetAllAsync()
        {
            return await _context.Follows
                .Include(f => f.Follower)
                .Include(f => f.Following)
                .ToListAsync();
        }

        public async Task<Follow?> GetByIdAsync(int id)
        {
            return await _context.Follows.FindAsync(id);
        }

        public async Task<Follow?> GetByFollowerAndFollowingAsync(string followerId, string followingId)
        {
            return await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<IEnumerable<Follow>> GetFollowersAsync(string userId)
        {
            return await _context.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowingId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Follow>> GetFollowingAsync(string userId)
        {
            return await _context.Follows
                .Include(f => f.Following)
                .Where(f => f.FollowerId == userId)
                .ToListAsync();
        }

        public async Task<Follow> AddAsync(Follow follow)
        {
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();
            return follow;
        }

        public async Task DeleteAsync(int id)
        {
            var follow = await _context.Follows.FindAsync(id);
            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByFollowerAndFollowingAsync(string followerId, string followingId)
        {
            var follow = await GetByFollowerAndFollowingAsync(followerId, followingId);
            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string followerId, string followingId)
        {
            return await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<int> GetFollowersCountAsync(string userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowingId == userId);
        }

        public async Task<int> GetFollowingCountAsync(string userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowerId == userId);
        }
    }
}
