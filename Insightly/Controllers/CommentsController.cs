using AutoMapper;
using Insightly.Models;
using Insightly.Repositories;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public CommentsController(ICommentRepository commentRepository, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
            _mapper = mapper;
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

            await _commentRepository.AddAsync(comment);

            var commentWithAuthor = await _commentRepository.GetByIdWithAuthorAsync(comment.CommentId);
            if (commentWithAuthor == null)
            {
                return BadRequest(new { message = "Failed to retrieve comment" });
            }

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                TempData["SuccessMessage"] = "Comment added!";
                return RedirectToAction("Details", "Articles", new { id = articleId });
            }

            var commentDto = _mapper.Map<CommentJsonDto>(commentWithAuthor);
            return Json(commentDto);
        }


        [HttpGet]
        public async Task<IActionResult> List(int articleId)
        {
            var comments = await _commentRepository.GetByArticleIdAsync(articleId);
            var commentDtos = _mapper.Map<List<CommentJsonDto>>(comments);

            return Json(commentDtos);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int commentId)
        {
            var comment = await _commentRepository.GetByIdWithAuthorAsync(commentId);
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
            await _commentRepository.DeleteAsync(commentId);

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

            var comment = await _commentRepository.GetByIdWithAuthorAsync(commentId);
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
            await _commentRepository.UpdateAsync(comment);

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                TempData["SuccessMessage"] = "Comment updated.";
                return RedirectToAction("Details", "Articles", new { id = comment.ArticleId });
            }

            var commentDto = _mapper.Map<CommentJsonDto>(comment);
            return Json(commentDto);
        }
    }
}
