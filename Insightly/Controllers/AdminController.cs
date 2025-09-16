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

        public IActionResult Index()
        {
            return View();
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
    }
}


