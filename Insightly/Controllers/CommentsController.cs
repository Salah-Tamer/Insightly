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
                UpdatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();


            return Json(new
            {
                id = comment.CommentId,
                content = comment.Content,
                author = user.UserName,
                createdAt = comment.CreatedAt.ToString("dd MMM yyyy HH:mm")
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
                    author = c.Author.UserName,
                    createdAt = c.CreatedAt.ToString("dd MMM yyyy HH:mm")
                })
                .ToListAsync();

            return Json(comments);
        }
    }
}
