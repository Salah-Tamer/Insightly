using Insightly.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Controllers
{
    [Authorize]
    public class ReactionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReactionsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

   
        [HttpPost]
        public async Task<IActionResult> React(int articleId, ReactionType type)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existingReaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.ArticleId == articleId && r.UserId == user.Id);

            if (existingReaction != null)
            {
                
                existingReaction.Type = type;
                existingReaction.CreatedAt = DateTime.Now;
                _context.Update(existingReaction);
            }
            else
            {
               
                var reaction = new Reaction
                {
                    ArticleId = articleId,
                    UserId = user.Id,
                    Type = type,
                    CreatedAt = DateTime.Now
                };
                _context.Reactions.Add(reaction);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Reaction saved!" });
        }

        [HttpGet]
        public async Task<IActionResult> Count(int articleId)
        {
            var likes = await _context.Reactions
                .CountAsync(r => r.ArticleId == articleId && r.Type == ReactionType.Like);

            var dislikes = await _context.Reactions
                .CountAsync(r => r.ArticleId == articleId && r.Type == ReactionType.Dislike);

            return Ok(new { likes, dislikes });
        }
    }
}
