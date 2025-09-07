using Insightly.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Controllers
{
    public class CommentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpPost]
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

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();


            return Json(new
            {
                id = comment.CommentId,
                content = comment.Content,
                author = user.Name,
                authorId = user.Id,
                createdAt = comment.CreatedAt.ToString("dd MMM yyyy HH:mm"),
                updatedAt = (string?)null,
                isUpdated = false
            });
        }


        [HttpGet]
        public async Task<IActionResult> List(int articleId)
        {
            var comments = await _context.Comments
                .Where(c => c.ArticleId == articleId)
                .Include(c => c.Author)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    id = c.CommentId,
                    content = c.Content,
                    author = c.Author.Name,
                    authorId = c.AuthorId,
                    createdAt = c.CreatedAt.ToString("dd MMM yyyy HH:mm"),
                    updatedAt = c.UpdatedAt.HasValue ? c.UpdatedAt.Value.ToString("dd MMM yyyy HH:mm") : (string?)null,
                    isUpdated = c.UpdatedAt.HasValue
                })
                .ToListAsync();

            return Json(comments);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int commentId)
        {
            var comment = await _context.Comments.Include(c => c.Author).FirstOrDefaultAsync(c => c.CommentId == commentId);
            if (comment == null)
            {
                return NotFound(new { message = "Comment not found" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || (comment.AuthorId != user.Id))
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int commentId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(new { message = "Comment cannot be empty" });
            }

            var comment = await _context.Comments.Include(c => c.Author).FirstOrDefaultAsync(c => c.CommentId == commentId);
            if (comment == null)
            {
                return NotFound(new { message = "Comment not found" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || (comment.AuthorId != user.Id))
            {
                return Forbid();
            }

            comment.Content = content;
            comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new
            {
                id = comment.CommentId,
                content = comment.Content,
                author = comment.Author.Name,
                authorId = comment.AuthorId,
                createdAt = comment.CreatedAt.ToString("dd MMM yyyy HH:mm"),
                updatedAt = comment.UpdatedAt?.ToString("dd MMM yyyy HH:mm"),
                isUpdated = comment.UpdatedAt.HasValue
            });
        }
    }
}
