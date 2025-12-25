using Insightly.Models;
using Insightly.Repositories;

namespace Insightly.Services
{
    public class VoteService : IVoteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VoteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, bool Removed, string Message)> ToggleArticleVoteAsync(int articleId, string userId, bool isUpvote)
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(articleId);
            if (article == null)
            {
                return (false, false, "Article not found");
            }

            bool removed = false;
            var existingVote = await _unitOfWork.Votes.GetByUserAndArticleAsync(userId, articleId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    // Remove vote if clicking the same vote type
                    await _unitOfWork.Votes.DeleteByUserAndArticleAsync(userId, articleId);
                    removed = true;
                }
                else
                {
                    // Change vote type (upvote to downvote or vice versa)
                    existingVote.IsUpvote = isUpvote;
                    await _unitOfWork.Votes.UpdateAsync(existingVote);
                }
            }
            else
            {
                // Create new vote
                var vote = new Vote
                {
                    ArticleId = articleId,
                    UserId = userId,
                    IsUpvote = isUpvote
                };
                await _unitOfWork.Votes.AddAsync(vote);
            }

            await _unitOfWork.SaveChangesAsync();

            return (true, removed, removed ? "Vote removed!" : "Vote saved!");
        }

        public async Task<(bool Success, bool Removed, string Message)> ToggleCommentVoteAsync(int commentId, string userId, bool isUpvote)
        {
            bool removed = false;
            var existingVote = await _unitOfWork.CommentVotes.GetByUserAndCommentAsync(userId, commentId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    await _unitOfWork.CommentVotes.DeleteByUserAndCommentAsync(userId, commentId);
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
                    UserId = userId,
                    IsUpvote = isUpvote
                };
                await _unitOfWork.CommentVotes.AddAsync(vote);
            }

            await _unitOfWork.SaveChangesAsync();

            return (true, removed, removed ? "Vote removed!" : "Vote saved!");
        }

        public async Task<int> GetArticleNetScoreAsync(int articleId)
        {
            return await _unitOfWork.Votes.GetNetScoreAsync(articleId);
        }

        public async Task<int> GetCommentNetScoreAsync(int commentId)
        {
            return await _unitOfWork.CommentVotes.GetNetScoreAsync(commentId);
        }

        public async Task<(bool Voted, bool? IsUpvote)> GetUserArticleVoteAsync(int articleId, string userId)
        {
            var vote = await _unitOfWork.Votes.GetByUserAndArticleAsync(userId, articleId);
            if (vote == null)
            {
                return (false, null);
            }
            return (true, vote.IsUpvote);
        }

        public async Task<(bool Voted, bool? IsUpvote)> GetUserCommentVoteAsync(int commentId, string userId)
        {
            var vote = await _unitOfWork.CommentVotes.GetByUserAndCommentAsync(userId, commentId);
            if (vote == null)
            {
                return (false, null);
            }
            return (true, vote.IsUpvote);
        }
    }
}

