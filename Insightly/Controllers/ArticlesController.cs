using Insightly.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create([FromForm] string title, [FromForm] string content)
        {
            var article = new Article
            {
                Title = title,
                Content = content,
            };


            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                article.AuthorId = currentUser.Id;
                article.CreatedAt = DateTime.Now;

                _context.Add(article);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Article created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(article);
        }
    }
}