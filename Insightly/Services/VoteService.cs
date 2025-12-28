using Insightly.Models;
using Insightly.Repositories;

namespace Insightly.Services
{
    public class VoteService : IVoteService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly ICommentVoteRepository _commentVoteRepository;

        public VoteService(
            IArticleRepository articleRepository,
            ICommentRepository commentRepository,
            IVoteRepository voteRepository,
            ICommentVoteRepository commentVoteRepository)
        {
            _articleRepository = articleRepository;
            _commentRepository = commentRepository;
            _voteRepository = voteRepository;
            _commentVoteRepository = commentVoteRepository;
        }

        public async Task<(bool Success, bool Removed, string Message)> ToggleArticleVoteAsync(int articleId, string userId, bool isUpvote)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, false, "User information is missing.");
            }

            if (articleId <= 0)
            {
                return (false, false, "Invalid article ID.");
            }

            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
            {
                return (false, false, "Article not found.");
            }

            if (article.AuthorId == userId)
            {
                return (false, false, "You cannot vote on your own article.");
            }

            bool removed = false;
            var existingVote = await _voteRepository.GetByUserAndArticleAsync(userId, articleId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    // Remove vote if clicking the same vote type
                    await _voteRepository.DeleteByUserAndArticleAsync(userId, articleId);
                    removed = true;
                }
                else
                {
                    // Change vote type (upvote to downvote or vice versa)
                    existingVote.IsUpvote = isUpvote;
                    await _voteRepository.UpdateAsync(existingVote);
                }
            }
            else
            {
                var vote = new Vote
                {
                    ArticleId = articleId,
                    UserId = userId,
                    IsUpvote = isUpvote
                };
                await _voteRepository.AddAsync(vote);
            }

            var voteType = isUpvote ? "upvote" : "downvote";
            return (true, removed, removed ? "Vote removed successfully!" : $"Article {voteType}d successfully!");
        }

        public async Task<(bool Success, bool Removed, string Message)> ToggleCommentVoteAsync(int commentId, string userId, bool isUpvote)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, false, "User information is missing.");
            }

            if (commentId <= 0)
            {
                return (false, false, "Invalid comment ID.");
            }

            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
            {
                return (false, false, "Comment not found.");
            }

            if (comment.AuthorId == userId)
            {
                return (false, false, "You cannot vote on your own comment.");
            }

            bool removed = false;
            var existingVote = await _commentVoteRepository.GetByUserAndCommentAsync(userId, commentId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    await _commentVoteRepository.DeleteByUserAndCommentAsync(userId, commentId);
                    removed = true;
                }
                else
                {
                    existingVote.IsUpvote = isUpvote;
                    await _commentVoteRepository.UpdateAsync(existingVote);
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
                await _commentVoteRepository.AddAsync(vote);
            }

            var voteType = isUpvote ? "upvote" : "downvote";
            return (true, removed, removed ? "Vote removed successfully!" : $"Comment {voteType}d successfully!");
        }

        public async Task<int> GetArticleNetScoreAsync(int articleId)
        {
            return await _voteRepository.GetNetScoreAsync(articleId);
        }

        public async Task<int> GetCommentNetScoreAsync(int commentId)
        {
            return await _commentVoteRepository.GetNetScoreAsync(commentId);
        }

        public async Task<(bool Voted, bool? IsUpvote)> GetUserArticleVoteAsync(int articleId, string userId)
        {
            var vote = await _voteRepository.GetByUserAndArticleAsync(userId, articleId);
            if (vote == null)
            {
                return (false, null);
            }
            return (true, vote.IsUpvote);
        }

        public async Task<(bool Voted, bool? IsUpvote)> GetUserCommentVoteAsync(int commentId, string userId)
        {
            var vote = await _commentVoteRepository.GetByUserAndCommentAsync(userId, commentId);
            if (vote == null)
            {
                return (false, null);
            }
            return (true, vote.IsUpvote);
        }
    }
}

