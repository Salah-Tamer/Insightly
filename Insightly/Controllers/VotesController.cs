using Insightly.Models;
using Insightly.Repositories;
using Insightly.Services;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    [Authorize]
    public class VotesController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVoteService _voteService;

        public VotesController(ICommentRepository commentRepository, UserManager<ApplicationUser> userManager, IVoteService voteService)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
            _voteService = voteService;
        }

   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int articleId, bool isUpvote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var (success, removed, message) = await _voteService.ToggleArticleVoteAsync(articleId, user.Id, isUpvote);
            
            if (!success)
            {
                return BadRequest(new { message });
            }

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Details", "Articles", new { id = articleId });
            }

            return Ok(new VoteResponseDto { IsRemoved = removed });
        }

        [HttpGet]
        public async Task<IActionResult> Count(int articleId)
        {
            var netScore = await _voteService.GetArticleNetScoreAsync(articleId);
            return Ok(new VoteCountDto { Score = netScore });
        }

        [HttpGet]
        public async Task<IActionResult> UserArticleVote(int articleId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var (voted, isUpvote) = await _voteService.GetUserArticleVoteAsync(articleId, user.Id);

            if (!voted) return Ok(new UserVoteStatusDto { HasVoted = false });
            return Ok(new UserVoteStatusDto { HasVoted = true, IsUpvote = isUpvote });
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

            var (success, removed, message) = await _voteService.ToggleArticleVoteAsync(request.ArticleId, user.Id, request.IsUpvote);
            
            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new VoteResponseDto { IsRemoved = removed });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentVote(int commentId, bool isUpvote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var (success, removed, message) = await _voteService.ToggleCommentVoteAsync(commentId, user.Id, isUpvote);
            
            if (!success)
            {
                return BadRequest(new { message });
            }

            var isAjax = string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            if (!isAjax)
            {
                return RedirectToAction("Details", "Articles", new { id = await GetArticleIdFromComment(commentId) });
            }

            return Ok(new VoteResponseDto { IsRemoved = removed });
        }

        [HttpGet]
        public async Task<IActionResult> CommentCount(int commentId)
        {
            var netScore = await _voteService.GetCommentNetScoreAsync(commentId);
            return Ok(new VoteCountDto { Score = netScore });
        }

        [HttpGet]
        public async Task<IActionResult> UserCommentVote(int commentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var (voted, isUpvote) = await _voteService.GetUserCommentVoteAsync(commentId, user.Id);

            if (!voted) return Ok(new UserVoteStatusDto { HasVoted = false });
            return Ok(new UserVoteStatusDto { HasVoted = true, IsUpvote = isUpvote });
        }

        private async Task<int> GetArticleIdFromComment(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            return comment?.ArticleId ?? 0;
        }
    }
}
