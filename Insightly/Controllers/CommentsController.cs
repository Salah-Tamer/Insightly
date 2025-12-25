using Insightly.Models;
using Insightly.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    public class CommentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Add(int articleId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(new { message = "Comment cannot be empty" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var comment = new Comment
            {
                Content = content,
                AuthorId = user.Id,
                ArticleId = articleId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                TempData["SuccessMessage"] = "Comment added!";
                return RedirectToAction("Details", "Articles", new { id = articleId });
            }

            return Json(new
            {
                id = comment.CommentId,
                content = comment.Content,
                author = user.Name,
                authorId = user.Id,
                authorProfilePicture = user.ProfilePicture,
                createdAt = comment.CreatedAt.ToString("dd MMM yyyy HH:mm"),
                updatedAt = (string?)null,
                isUpdated = false
            });
        }


        [HttpGet]
        public async Task<IActionResult> List(int articleId)
        {
            var comments = await _unitOfWork.Comments.GetByArticleIdAsync(articleId);
            var result = comments.Select(c => new
            {
                id = c.CommentId,
                content = c.Content,
                author = c.Author.Name,
                authorId = c.AuthorId,
                authorProfilePicture = c.Author.ProfilePicture,
                createdAt = c.CreatedAt.ToString("dd MMM yyyy HH:mm"),
                updatedAt = c.UpdatedAt.HasValue ? c.UpdatedAt.Value.ToString("dd MMM yyyy HH:mm") : (string?)null,
                isUpdated = c.UpdatedAt.HasValue
            });

            return Json(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int commentId)
        {
            var comment = await _unitOfWork.Comments.GetByIdWithAuthorAsync(commentId);
            if (comment == null)
            {
                return NotFound(new { message = "Comment not found" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Allow deletion if user is the comment author OR if user is an admin
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (comment.AuthorId != user.Id && !isAdmin)
            {
                return Forbid();
            }

            int articleId = comment.ArticleId;
            await _unitOfWork.Comments.DeleteAsync(commentId);
            await _unitOfWork.SaveChangesAsync();

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                TempData["SuccessMessage"] = "Comment deleted.";
                return RedirectToAction("Details", "Articles", new { id = articleId });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int commentId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(new { message = "Comment cannot be empty" });
            }

            var comment = await _unitOfWork.Comments.GetByIdWithAuthorAsync(commentId);
            if (comment == null)
            {
                return NotFound(new { message = "Comment not found" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (comment.AuthorId != user.Id)
            {
                return Forbid();
            }

            comment.Content = content;
            comment.UpdatedAt = DateTime.Now;
            await _unitOfWork.Comments.UpdateAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                TempData["SuccessMessage"] = "Comment updated.";
                return RedirectToAction("Details", "Articles", new { id = comment.ArticleId });
            }

            return Json(new
            {
                id = comment.CommentId,
                content = comment.Content,
                author = comment.Author.Name,
                authorId = comment.AuthorId,
                authorProfilePicture = comment.Author.ProfilePicture,
                createdAt = comment.CreatedAt.ToString("dd MMM yyyy HH:mm"),
                updatedAt = comment.UpdatedAt?.ToString("dd MMM yyyy HH:mm"),
                isUpdated = comment.UpdatedAt.HasValue
            });
        }
    }
}
