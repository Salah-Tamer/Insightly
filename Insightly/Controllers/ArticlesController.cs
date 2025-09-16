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

        [Authorize(Roles = "Admin,User")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create([FromForm] string title, [FromForm] string content, IFormFile? photo)
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

                // Handle optional photo upload
                if (photo != null && photo.Length > 0)
                {
                    // Basic validation: limit size to 5 MB and allow common image types
                    long maxBytes = 5 * 1024 * 1024;
                    if (photo.Length > maxBytes)
                    {
                        ModelState.AddModelError("photo", "Photo must be 5 MB or smaller.");
                        return View(article);
                    }

                    var permitted = new[] { "image/jpeg", "image/png", "image/gif" };
                    if (!permitted.Contains(photo.ContentType))
                    {
                        ModelState.AddModelError("photo", "Only JPG, PNG, or GIF images are allowed.");
                        return View(article);
                    }

                    var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "articles");
                    if (!Directory.Exists(uploadsRoot))
                    {
                        Directory.CreateDirectory(uploadsRoot);
                    }

                    var fileExtension = Path.GetExtension(photo.FileName);
                    var safeFileName = $"article_{Guid.NewGuid():N}{fileExtension}";
                    var filePath = Path.Combine(uploadsRoot, safeFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    // Store a web-accessible relative path for rendering
                    article.ImagePath = $"/uploads/articles/{safeFileName}";
                }

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
                .FirstOrDefaultAsync(a => a.ArticleId == id);

            if (article == null)
            {
                return NotFound();
            }

            var netScore = await _context.Votes
                .Where(v => v.ArticleId == id)
                .SumAsync(v => v.IsUpvote ? 1 : -1);

            ViewBag.NetScore = netScore;
            ViewBag.CommentsCount = article.Comments.Count;

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var isRead = await _context.ArticleReads
                    .AnyAsync(ar => ar.ArticleId == id && ar.UserId == currentUser.Id);
                ViewBag.IsRead = isRead;
            }
            else
            {
                ViewBag.IsRead = false;
            }

            return View(article);
        }
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (article.AuthorId != user.Id)
            {
                return Forbid();
            }

            return View(article);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleId,Title,Content")] Article article)
        {
            if (id != article.ArticleId) return NotFound();

            var existingArticle = await _context.Articles.FindAsync(id);
            if (existingArticle == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (existingArticle.AuthorId != user.Id)
            {
                return Forbid();
            }

            // Clear ModelState errors for fields we're not binding
            ModelState.Remove("AuthorId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Author");

            if (ModelState.IsValid)
            {
                try
                {
                    existingArticle.Title = article.Title;
                    existingArticle.Content = article.Content;
                    existingArticle.UpdatedAt = DateTime.Now;

                    _context.Update(existingArticle);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Article updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = article.ArticleId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }
            return View(article);
        }
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id);

            if (article == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (article.AuthorId != user.Id && !isAdmin)
            {
                return Forbid();
            }

            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (article.AuthorId != user.Id && !isAdmin)
            {
                return Forbid();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Article deleted successfully!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Save(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var existingRead = await _context.ArticleReads
                .FirstOrDefaultAsync(ar => ar.ArticleId == id && ar.UserId == currentUser.Id);

            if (existingRead == null)
            {
                var articleRead = new ArticleRead
                {
                    ArticleId = id,
                    UserId = currentUser.Id,
                    ReadAt = DateTime.Now
                };

                _context.ArticleReads.Add(articleRead);
                await _context.SaveChangesAsync();
            }

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                TempData["SuccessMessage"] = "Article saved!";
                return RedirectToAction(nameof(Details), new { id });
            }

            return Json(new { success = true, message = "Article saved!" });
        }

        [Authorize]
        public async Task<IActionResult> SavedArticles()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var readArticles = await _context.ArticleReads
                .Include(ar => ar.Article)
                .ThenInclude(a => a.Author)
                .Where(ar => ar.UserId == currentUser.Id)
                .OrderByDescending(ar => ar.ReadAt)
                .Select(ar => new
                {
                    Article = ar.Article,
                    ReadAt = ar.ReadAt
                })
                .ToListAsync();

            return View(readArticles);
        }

        [Authorize]
        public async Task<IActionResult> MyArticles()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var myArticles = await _context.Articles
                .Include(a => a.Author)
                .Where(a => a.AuthorId == currentUser.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(myArticles);
        }
    }
}