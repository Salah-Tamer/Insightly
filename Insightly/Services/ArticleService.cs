using Insightly.Models;
using Insightly.Repositories;

namespace Insightly.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IArticleReadRepository _articleReadRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IVoteRepository _voteRepository;
        private const int MinTitleLength = 3;
        private const int MaxTitleLength = 100;
        private const int MinContentLength = 10;
        private const int MaxContentLength = 50000;

        public ArticleService(
            IArticleRepository articleRepository,
            IArticleReadRepository articleReadRepository,
            ICommentRepository commentRepository,
            IVoteRepository voteRepository)
        {
            _articleRepository = articleRepository;
            _articleReadRepository = articleReadRepository;
            _commentRepository = commentRepository;
            _voteRepository = voteRepository;
        }

        public async Task<(bool Success, string? ErrorMessage, Article? Article)> CreateArticleAsync(string title, string content, string authorId, string? imagePath)
        {
            var validationError = ValidateArticleData(title, content, authorId);
            if (validationError != null)
            {
                return (false, validationError, null);
            }

            title = title.Trim();
            content = content.Trim();

            var existingArticles = await _articleRepository.GetByAuthorIdAsync(authorId);
            if (existingArticles.Any(a => a.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "You already have an article with this title. Please choose a different title.", null);
            }

            var article = new Article
            {
                Title = title,
                Content = content,
                AuthorId = authorId,
                CreatedAt = DateTime.Now,
                ImagePath = imagePath
            };

            await _articleRepository.AddAsync(article);

            return (true, null, article);
        }

        private string? ValidateArticleData(string title, string content, string authorId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return "Title is required.";
            }

            if (title.Trim().Length < MinTitleLength)
            {
                return $"Title must be at least {MinTitleLength} characters long.";
            }

            if (title.Length > MaxTitleLength)
            {
                return $"Title cannot exceed {MaxTitleLength} characters.";
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return "Content is required.";
            }

            if (content.Trim().Length < MinContentLength)
            {
                return $"Content must be at least {MinContentLength} characters long.";
            }

            if (content.Length > MaxContentLength)
            {
                return $"Content cannot exceed {MaxContentLength} characters.";
            }

            if (string.IsNullOrWhiteSpace(authorId))
            {
                return "Author information is missing.";
            }

            return null;
        }

        public async Task<Article?> GetArticleDetailsAsync(int id, string? userId)
        {
            return await _articleRepository.GetByIdWithAuthorAndCommentsAsync(id);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateArticleAsync(int id, string title, string content, string userId)
        {
            var existingArticle = await _articleRepository.GetByIdAsync(id);
            if (existingArticle == null)
            {
                return (false, "Article not found.");
            }

            if (existingArticle.AuthorId != userId)
            {
                return (false, "Unauthorized. You can only edit your own articles.");
            }

            var validationError = ValidateArticleData(title, content, userId);
            if (validationError != null)
            {
                return (false, validationError);
            }

            title = title.Trim();
            content = content.Trim();

            var existingArticles = await _articleRepository.GetByAuthorIdAsync(userId);
            if (existingArticles.Any(a => a.ArticleId != id && a.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "You already have another article with this title. Please choose a different title.");
            }

            // Check if anything actually changed
            if (existingArticle.Title == title && existingArticle.Content == content)
            {
                return (false, "No changes detected. Please modify the title or content before updating.");
            }

            existingArticle.Title = title;
            existingArticle.Content = content;
            existingArticle.UpdatedAt = DateTime.Now;

            await _articleRepository.UpdateAsync(existingArticle);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteArticleAsync(int id, string userId, bool isAdmin)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return (false, "Article not found.");
            }

            if (article.AuthorId != userId && !isAdmin)
            {
                return (false, "Unauthorized. You can only delete your own articles.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, "User information is missing.");
            }

            var commentsCount = await _commentRepository.GetCountByArticleAsync(id);
            var netScore = await _voteRepository.GetNetScoreAsync(id);

            await _articleRepository.DeleteAsync(id);

            return (true, null);
        }

        public async Task<(bool Success, bool IsSaved, string Message)> ToggleSaveArticleAsync(int articleId, string userId)
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

            var existingRead = await _articleReadRepository.GetByUserAndArticleAsync(userId, articleId);

            if (existingRead == null)
            {
                var articleRead = new ArticleRead
                {
                    ArticleId = articleId,
                    UserId = userId,
                    ReadAt = DateTime.Now
                };

                await _articleReadRepository.AddAsync(articleRead);
                return (true, true, "Article saved successfully!");
            }
            else
            {
                await _articleReadRepository.DeleteByUserAndArticleAsync(userId, articleId);
                return (true, false, "Article removed from saved items.");
            }
        }
    }
}

