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
        public async Task<IActionResult> Vote(int articleId, bool isUpvote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.ArticleId == articleId && v.UserId == user.Id);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    _context.Votes.Remove(existingVote);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Vote removed!", removed = true });
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
            return Ok(new { message = "Vote saved!", removed = false });
        }

        [HttpGet]
        public async Task<IActionResult> Count(int articleId)
        {
            var netScore = await _context.Votes
                .Where(v => v.ArticleId == articleId)
                .SumAsync(v => v.IsUpvote ? 1 : -1);

            return Ok(new { netScore });
        }
    }
}
