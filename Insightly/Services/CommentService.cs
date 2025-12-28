using Insightly.Models;
using Insightly.Repositories;

namespace Insightly.Services
{
    public class CommentService : ICommentService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ICommentVoteRepository _commentVoteRepository;
        private const int MinCommentLength = 1;
        private const int MaxCommentLength = 2000;

        public CommentService(
            IArticleRepository articleRepository,
            ICommentRepository commentRepository,
            ICommentVoteRepository commentVoteRepository)
        {
            _articleRepository = articleRepository;
            _commentRepository = commentRepository;
            _commentVoteRepository = commentVoteRepository;
        }

        public async Task<(bool Success, string? ErrorMessage, Comment? Comment)> AddCommentAsync(int articleId, string content, string authorId)
        {
            var validationError = ValidateCommentData(content, authorId);
            if (validationError != null)
            {
                return (false, validationError, null);
            }

            if (articleId <= 0)
            {
                return (false, "Invalid article ID.", null);
            }

            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
            {
                return (false, "Article not found. Cannot add comment to non-existent article.", null);
            }

            content = content.Trim();

            var recentComments = await _commentRepository.GetByArticleIdAsync(articleId);
            var duplicateComment = recentComments.FirstOrDefault(c => 
                c.AuthorId == authorId && 
                c.Content == content && 
                c.CreatedAt > DateTime.Now.AddMinutes(-1));

            if (duplicateComment != null)
            {
                return (false, "You just posted this comment. Please wait before posting it again.", null);
            }

            var comment = new Comment
            {
                Content = content,
                AuthorId = authorId,
                ArticleId = articleId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };

            await _commentRepository.AddAsync(comment);

            var commentWithAuthor = await _commentRepository.GetByIdWithAuthorAsync(comment.CommentId);
            if (commentWithAuthor == null)
            {
                return (false, "Comment was created but could not be retrieved.", null);
            }

            return (true, null, commentWithAuthor);
        }

        public async Task<(bool Success, string? ErrorMessage, Comment? Comment)> UpdateCommentAsync(int commentId, string content, string userId)
        {
            var validationError = ValidateCommentData(content, userId);
            if (validationError != null)
            {
                return (false, validationError, null);
            }

            if (commentId <= 0)
            {
                return (false, "Invalid comment ID.", null);
            }

            var comment = await _commentRepository.GetByIdWithAuthorAsync(commentId);
            if (comment == null)
            {
                return (false, "Comment not found.", null);
            }

            if (comment.AuthorId != userId)
            {
                return (false, "Unauthorized. You can only edit your own comments.", null);
            }

            content = content.Trim();

            if (comment.Content == content)
            {
                return (false, "No changes detected. The comment content is the same.", null);
            }

            var hoursSinceCreation = (DateTime.Now - comment.CreatedAt).TotalHours;
            if (hoursSinceCreation > 24)
            {
                return (false, "Comments can only be edited within 24 hours of posting.", null);
            }

            comment.Content = content;
            comment.UpdatedAt = DateTime.Now;
            await _commentRepository.UpdateAsync(comment);

            return (true, null, comment);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteCommentAsync(int commentId, string userId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, "User information is missing.");
            }

            if (commentId <= 0)
            {
                return (false, "Invalid comment ID.");
            }

            var comment = await _commentRepository.GetByIdWithAuthorAsync(commentId);
            if (comment == null)
            {
                return (false, "Comment not found.");
            }

            if (comment.AuthorId != userId && !isAdmin)
            {
                return (false, "Unauthorized. You can only delete your own comments.");
            }

            var commentVotesCount = await _commentVoteRepository.GetNetScoreAsync(commentId);

            await _commentRepository.DeleteAsync(commentId);

            return (true, null);
        }

        private string? ValidateCommentData(string content, string authorId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "Comment content is required.";
            }

            if (content.Trim().Length < MinCommentLength)
            {
                return $"Comment must be at least {MinCommentLength} character long.";
            }

            if (content.Length > MaxCommentLength)
            {
                return $"Comment cannot exceed {MaxCommentLength} characters.";
            }

            if (string.IsNullOrWhiteSpace(authorId))
            {
                return "Author information is missing.";
            }

            if (IsSpamContent(content))
            {
                return "Comment appears to be spam and cannot be posted.";
            }

            return null;
        }

        private bool IsSpamContent(string content)
        {
            var lowerContent = content.ToLower();
            
            if (content.Length > 10)
            {
                var firstTenChars = content.Substring(0, 10);
                var repetitionCount = (content.Length - content.Replace(firstTenChars, "").Length) / firstTenChars.Length;
                if (repetitionCount > 5)
                {
                    return true;
                }
            }

            var uppercaseCount = content.Count(char.IsUpper);
            if (content.Length > 20 && uppercaseCount > content.Length * 0.7)
            {
                return true;
            }

            return false;
        }
    }
}


