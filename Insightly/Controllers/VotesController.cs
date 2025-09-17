using Insightly.Models;
using Insightly.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    [Authorize]
    public class VotesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public VotesController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int articleId, bool isUpvote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool removed = false;
            var existingVote = await _unitOfWork.Votes.GetByUserAndArticleAsync(user.Id, articleId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    await _unitOfWork.Votes.DeleteByUserAndArticleAsync(user.Id, articleId);
                    removed = true;
                }
                else
                {
                    existingVote.IsUpvote = isUpvote;
                    await _unitOfWork.Votes.UpdateAsync(existingVote);
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
                await _unitOfWork.Votes.AddAsync(vote);
            }

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
            var netScore = await _unitOfWork.Votes.GetNetScoreAsync(articleId);
            return Ok(new { netScore });
        }

        [HttpGet]
        public async Task<IActionResult> UserArticleVote(int articleId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var vote = await _unitOfWork.Votes.GetByUserAndArticleAsync(user.Id, articleId);

            if (vote == null) return Ok(new { voted = false });
            return Ok(new { voted = true, isUpvote = vote.IsUpvote });
        }

        [HttpPost]
        public async Task<IActionResult> AjaxVote([FromBody] AjaxVoteRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Validate that the article exists
            var article = await _unitOfWork.Articles.GetByIdAsync(request.ArticleId);
            if (article == null)
            {
                return BadRequest("Article not found");
            }

            bool removed = false;
            var existingVote = await _unitOfWork.Votes.GetByUserAndArticleAsync(user.Id, request.ArticleId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == request.IsUpvote)
                {
                    await _unitOfWork.Votes.DeleteByUserAndArticleAsync(user.Id, request.ArticleId);
                    removed = true;
                }
                else
                {
                    existingVote.IsUpvote = request.IsUpvote;
                    await _unitOfWork.Votes.UpdateAsync(existingVote);
                }
            }
            else
            {
                var vote = new Vote
                {
                    ArticleId = request.ArticleId,
                    UserId = user.Id,
                    IsUpvote = request.IsUpvote
                };
                await _unitOfWork.Votes.AddAsync(vote);
            }

            return Ok(new { message = removed ? "Vote removed!" : "Vote saved!", removed });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentVote(int commentId, bool isUpvote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool removed = false;
            var existingVote = await _unitOfWork.CommentVotes.GetByUserAndCommentAsync(user.Id, commentId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    await _unitOfWork.CommentVotes.DeleteByUserAndCommentAsync(user.Id, commentId);
                    removed = true;
                }
                else
                {
                    existingVote.IsUpvote = isUpvote;
                    await _unitOfWork.CommentVotes.UpdateAsync(existingVote);
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
                await _unitOfWork.CommentVotes.AddAsync(vote);
            }

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
            var netScore = await _unitOfWork.CommentVotes.GetNetScoreAsync(commentId);
            return Ok(new { netScore });
        }

        [HttpGet]
        public async Task<IActionResult> UserCommentVote(int commentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var vote = await _unitOfWork.CommentVotes.GetByUserAndCommentAsync(user.Id, commentId);

            if (vote == null) return Ok(new { voted = false });
            return Ok(new { voted = true, isUpvote = vote.IsUpvote });
        }

        private async Task<int> GetArticleIdFromComment(int commentId)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            return comment?.ArticleId ?? 0;
        }
    }
}
