using Insightly.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUserId = currentUser?.Id;
            return View(admins);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["SuccessMessage"] = "Please enter a valid email.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user == null)
            {
                TempData["SuccessMessage"] = $"No user found with email {email}.";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["SuccessMessage"] = $"{user.Name} is already an Admin.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"{user.Name} has been promoted to Admin.";
            }
            else
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                TempData["SuccessMessage"] = $"Failed to promote user: {error}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent removing the last admin
            var allAdmins = await _userManager.GetUsersInRoleAsync("Admin");
            if (allAdmins.Count <= 1)
            {
                TempData["ErrorMessage"] = "Cannot remove the last admin. At least one admin must remain.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent self-removal
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == userId)
            {
                TempData["ErrorMessage"] = "You cannot remove your own admin privileges.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"{user.Name} has been removed from Admin role.";
            }
            else
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = $"Failed to remove admin privileges: {error}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}


