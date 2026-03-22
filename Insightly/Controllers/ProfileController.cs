using Insightly.Services;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Insightly.Controllers
{
    [Authorize]
    [Route("profile")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }

            var page = await _profileService.GetMyProfilePageAsync(userId);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ById(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _profileService.GetPublicProfilePageAsync(id, currentUserId);
            if (result.UserNotFound)
            {
                return NotFound();
            }

            if (result.ViewerIsOwner)
            {
                return RedirectToAction(nameof(Me));
            }

            if (result.Page == null)
            {
                return NotFound();
            }

            return View("User", result.Page);
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }

            var model = await _profileService.GetEditProfileAsync(userId);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost("edit")]
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
            return RedirectToAction(nameof(Me));
        }

        [HttpGet("/Profile/Index")]
        public IActionResult LegacyIndex() => Redirect("/profile/me");

        [HttpGet("/Profile/ViewProfile/{id}")]
        public IActionResult LegacyViewProfile(string id) => Redirect($"/profile/{id}");
    }
}
