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

        public ArticlesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "User")]
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
                return RedirectToAction("Index", "Home");
            }

            return View(article);
        }
        public async Task<IActionResult> Details(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Comments)
                    .ThenInclude(c => c.Author)
                .Include(a => a.Reactions)
                .FirstOrDefaultAsync(a => a.ArticleId == id);

            if (article == null)
            {
                return NotFound();
            }


            ViewBag.Likes = article.Reactions.Count(r => r.Type == ReactionType.Like);
            ViewBag.Dislikes = article.Reactions.Count(r => r.Type == ReactionType.Dislike);
            ViewBag.CommentsCount = article.Comments.Count;

            return View(article);
        }
    }
}