using Insightly.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Controllers
{
    [Authorize]
    public class VotesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VotesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int articleId, bool isUpvote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool removed = false;
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.ArticleId == articleId && v.UserId == user.Id);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    _context.Votes.Remove(existingVote);
                    removed = true;
                }
                else
                {
                    existingVote.IsUpvote = isUpvote;
                    _context.Update(existingVote);
                }
            }
            else
            {
                var vote = new Vote
                {
                    ArticleId = articleId,
                    UserId = user.Id,
                    IsUpvote = isUpvote
                };
                _context.Votes.Add(vote);
            }

            await _context.SaveChangesAsync();

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                TempData["SuccessMessage"] = removed ? "Vote removed!" : "Vote saved!";
                return RedirectToAction("Details", "Articles", new { id = articleId });
            }

            return Ok(new { message = removed ? "Vote removed!" : "Vote saved!", removed });
        }

        [HttpGet]
        public async Task<IActionResult> Count(int articleId)
        {
            var netScore = await _context.Votes
                .Where(v => v.ArticleId == articleId)
                .SumAsync(v => v.IsUpvote ? 1 : -1);

            return Ok(new { netScore });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentVote(int commentId, bool isUpvote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool removed = false;
            var existingVote = await _context.CommentVotes
                .FirstOrDefaultAsync(v => v.CommentId == commentId && v.UserId == user.Id);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    _context.CommentVotes.Remove(existingVote);
                    removed = true;
                }
                else
                {
                    existingVote.IsUpvote = isUpvote;
                    _context.CommentVotes.Update(existingVote);
                }
            }
            else
            {
                var vote = new CommentVote
                {
                    CommentId = commentId,
                    UserId = user.Id,
                    IsUpvote = isUpvote
                };
                _context.CommentVotes.Add(vote);
            }

            await _context.SaveChangesAsync();

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                return RedirectToAction("Details", "Articles", new { id = await GetArticleIdFromComment(commentId) });
            }

            return Ok(new { message = removed ? "Vote removed!" : "Vote saved!", removed });
        }

        [HttpGet]
        public async Task<IActionResult> CommentCount(int commentId)
        {
            var netScore = await _context.CommentVotes
                .Where(v => v.CommentId == commentId)
                .SumAsync(v => v.IsUpvote ? 1 : -1);

            return Ok(new { netScore });
        }

        private async Task<int> GetArticleIdFromComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            return comment?.ArticleId ?? 0;
        }
    }
}
