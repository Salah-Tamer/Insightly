using Insightly.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Follow(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            if (currentUser.Id == userId)
            {
                TempData["Error"] = "You cannot follow yourself.";
                return RedirectToAction("ViewProfile", "Profile", new { id = userId });
            }

            var alreadyFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == userId);

            if (!alreadyFollowing)
            {
                var follow = new Follow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = userId
                };

                _context.Follows.Add(follow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewProfile", "Profile", new { id = userId });
        }

        [HttpPost]
        public async Task<IActionResult> Unfollow(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == userId);

            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewProfile", "Profile", new { id = userId });
        }
    }
}
