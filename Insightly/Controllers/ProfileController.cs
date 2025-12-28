using Insightly.Models;
using Insightly.Services;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Insightly.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProfileService _profileService;

        public ProfileController(UserManager<ApplicationUser> userManager, IProfileService profileService)
        {
            _userManager = userManager;
            _profileService = profileService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }

            var (profile, articles, followersCount, followingCount) = await _profileService.GetProfileWithDetailsAsync(userId);
            
            if (profile == null)
            {
                return NotFound();
            }

            ViewBag.Articles = articles;
            ViewBag.FollowersCount = followersCount;
            ViewBag.FollowingCount = followingCount;
            return View(profile);
        }

        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Name = user.Name,
                ProfilePicture = user.ProfilePicture,
                Bio = user.Bio,
                Email = user.Email ?? string.Empty,
                Gender = user.Gender
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }

            var (success, errorMessage) = await _profileService.UpdateProfileAsync(userId, model);
            
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to update profile.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ViewProfile(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id;

            var (profile, articles, followersCount, followingCount, isFollowing) = 
                await _profileService.GetProfileWithDetailsAsync(id, currentUserId);

            if (profile == null)
            {
                return NotFound();
            }

            ViewBag.Articles = articles;
            ViewBag.FollowersCount = followersCount;
            ViewBag.FollowingCount = followingCount;
            ViewBag.IsFollowing = isFollowing;

            return View(profile);
        }


    }
}
