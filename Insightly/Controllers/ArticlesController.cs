using AutoMapper;
using Insightly.Models;
using Insightly.Repositories;
using Insightly.Services;
using Insightly.ViewModels;
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
        private readonly IMapper _mapper;

        public ArticlesController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IArticleService articleService, IFileUploadService fileUploadService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _articleService = articleService;
            _fileUploadService = fileUploadService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin,User")]
        public IActionResult Create()
        {
            return View(new ArticleCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create(ArticleCreateViewModel viewModel, IFormFile? photo)
        {
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
                    var (isValid, validationError) = await _fileUploadService.ValidateImageAsync(photo);
                    if (!isValid)
                    {
                        ModelState.AddModelError("photo", validationError ?? "Invalid image file.");
                        return View(viewModel);
                    }

                    imagePath = await _fileUploadService.UploadArticleImageAsync(photo);
                }

                // Create article using service
                var (success, createError, createdArticle) = await _articleService.CreateArticleAsync(viewModel.Title, viewModel.Content, currentUser.Id, imagePath);
                
                if (!success)
                {
                    ModelState.AddModelError("", createError ?? "Failed to create article.");
                    return View(viewModel);
                }

                TempData["SuccessMessage"] = "Article created successfully!";
                return RedirectToAction("Index", "Home");
            }

            return View(viewModel);
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

            // Map Article entity to ArticleDetailsViewModel using AutoMapper
            var viewModel = _mapper.Map<ArticleDetailsViewModel>(article);

            return View(viewModel);
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

            // Map Article entity to ArticleEditViewModel using AutoMapper
            var viewModel = _mapper.Map<ArticleEditViewModel>(article);

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, ArticleEditViewModel viewModel)
        {
            if (id != viewModel.ArticleId) return NotFound();

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

            if (ModelState.IsValid)
            {
                var (success, errorMessage) = await _articleService.UpdateArticleAsync(id, viewModel.Title, viewModel.Content, user.Id);
                
                if (!success)
                {
                    if (errorMessage == "Unauthorized")
                    {
                        return Forbid();
                    }
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Article updated successfully!";
                return RedirectToAction(nameof(Details), new { id = viewModel.ArticleId });
            }
            return View(viewModel);
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

            // Map Article entity to ArticleDetailsViewModel using AutoMapper
            var viewModel = _mapper.Map<ArticleDetailsViewModel>(article);

            return View(viewModel);
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
            var viewModels = readArticles.Select(ar => new SavedArticleViewModel
            {
                Article = _mapper.Map<ArticleListItemViewModel>(ar.Article),
                ReadAt = ar.ReadAt
            }).ToList();

            return View(viewModels);
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
            var viewModels = _mapper.Map<List<ArticleListItemViewModel>>(myArticles);

            return View(viewModels);
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
            var viewModels = _mapper.Map<List<ArticleListItemViewModel>>(followingArticles);

            return View(viewModels);
        }
    }
}