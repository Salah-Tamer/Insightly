using Insightly.Models;
using Insightly.Repositories;
using Insightly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IArticleService _articleService;
        private readonly IFileUploadService _fileUploadService;

        public ArticlesController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IArticleService articleService, IFileUploadService fileUploadService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _articleService = articleService;
            _fileUploadService = fileUploadService;
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

                string? imagePath = null;
                if (photo != null && photo.Length > 0)
                {
                    var (isValid, errorMessage) = await _fileUploadService.ValidateImageAsync(photo);
                    if (!isValid)
                    {
                        ModelState.AddModelError("photo", errorMessage ?? "Invalid image file.");
                        return View(article);
                    }

                    imagePath = await _fileUploadService.UploadArticleImageAsync(photo);
                }

                // Create article using service
                var (success, errorMessage, createdArticle) = await _articleService.CreateArticleAsync(title, content, currentUser.Id, imagePath);
                
                if (!success)
                {
                    ModelState.AddModelError("", errorMessage ?? "Failed to create article.");
                    return View(article);
                }

                TempData["SuccessMessage"] = "Article created successfully!";
                return RedirectToAction("Index", "Home");
            }

            return View(article);
        }
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userId = currentUser?.Id;

            var article = await _articleService.GetArticleDetailsAsync(id, userId);

            if (article == null)
            {
                return NotFound();
            }

            var netScore = await _unitOfWork.Votes.GetNetScoreAsync(id);
            var commentsCount = await _unitOfWork.Comments.GetCountByArticleAsync(id);

            ViewBag.NetScore = netScore;
            ViewBag.CommentsCount = commentsCount;

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
                var (success, errorMessage) = await _articleService.UpdateArticleAsync(id, article.Title, article.Content, user.Id);
                
                if (!success)
                {
                    if (errorMessage == "Unauthorized")
                    {
                        return Forbid();
                    }
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Article updated successfully!";
                return RedirectToAction(nameof(Details), new { id = article.ArticleId });
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
            var (success, errorMessage) = await _articleService.DeleteArticleAsync(id, user.Id, isAdmin);
            
            if (!success)
            {
                if (errorMessage == "Unauthorized")
                {
                    return Forbid();
                }
                return NotFound();
            }

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

            var (success, isSaved, message) = await _articleService.ToggleSaveArticleAsync(id, currentUser.Id);
            
            if (!success)
            {
                return NotFound();
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