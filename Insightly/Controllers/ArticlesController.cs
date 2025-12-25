using Insightly.Models;
using Insightly.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ArticlesController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
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

                await _unitOfWork.Articles.AddAsync(article);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Article created successfully!";
                return RedirectToAction("Index", "Home");
            }

            return View(article);
        }
        public async Task<IActionResult> Details(int id)
        {
            var article = await _unitOfWork.Articles.GetByIdWithAuthorAndCommentsAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            var netScore = await _unitOfWork.Votes.GetNetScoreAsync(id);
            var commentsCount = await _unitOfWork.Comments.GetCountByArticleAsync(id);

            ViewBag.NetScore = netScore;
            ViewBag.CommentsCount = commentsCount;

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var isRead = await _unitOfWork.ArticleReads.ExistsAsync(currentUser.Id, id);
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
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
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

            var existingArticle = await _unitOfWork.Articles.GetByIdAsync(id);
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

                    await _unitOfWork.Articles.UpdateAsync(existingArticle);
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Article updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = article.ArticleId });
                }
                catch (Exception)
                {
                    return NotFound();
                }
            }
            return View(article);
        }
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _unitOfWork.Articles.GetByIdWithAuthorAsync(id);

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
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
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

            await _unitOfWork.Articles.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

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

            var article = await _unitOfWork.Articles.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var existingRead = await _unitOfWork.ArticleReads.GetByUserAndArticleAsync(currentUser.Id, id);
            bool isSaved = false;
            string message = "";

            if (existingRead == null)
            {
                var articleRead = new ArticleRead
                {
                    ArticleId = id,
                    UserId = currentUser.Id,
                    ReadAt = DateTime.Now
                };

                await _unitOfWork.ArticleReads.AddAsync(articleRead);
                await _unitOfWork.SaveChangesAsync();
                isSaved = true;
                message = "Article saved!";
            }
            else
            {
                await _unitOfWork.ArticleReads.DeleteByUserAndArticleAsync(currentUser.Id, id);
                await _unitOfWork.SaveChangesAsync();
                isSaved = false;
                message = "Article unsaved!";
            }

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Details), new { id });
            }

            return Json(new { success = true, message = message, isSaved = isSaved });
        }

        [Authorize]
        public async Task<IActionResult> SavedArticles()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var readArticles = await _unitOfWork.ArticleReads.GetByUserIdAsync(currentUser.Id);
            var result = readArticles.Select(ar => new
            {
                Article = ar.Article,
                ReadAt = ar.ReadAt
            });

            return View(result);
        }

        [Authorize]
        public async Task<IActionResult> MyArticles()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var myArticles = await _unitOfWork.Articles.GetByAuthorIdAsync(currentUser.Id);
            return View(myArticles);
        }

        [Authorize]
        public async Task<IActionResult> FollowingArticles()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var followingArticles = await _unitOfWork.Articles.GetByFollowingUsersAsync(currentUser.Id);
            return View(followingArticles);
        }
    }
}