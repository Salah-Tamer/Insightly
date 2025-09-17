using Insightly.Models;
using Insightly.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
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

            var alreadyFollowing = await _unitOfWork.Follows.ExistsAsync(currentUser.Id, userId);

            if (!alreadyFollowing)
            {
                var follow = new Follow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = userId
                };

                await _unitOfWork.Follows.AddAsync(follow);
            }

            return RedirectToAction("ViewProfile", "Profile", new { id = userId });
        }

        [HttpPost]
        public async Task<IActionResult> Unfollow(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            await _unitOfWork.Follows.DeleteByFollowerAndFollowingAsync(currentUser.Id, userId);

            return RedirectToAction("ViewProfile", "Profile", new { id = userId });
        }
    }
}
