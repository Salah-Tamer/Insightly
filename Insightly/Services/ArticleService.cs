using Insightly.Models;
using Insightly.Repositories;

namespace Insightly.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ArticleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string? ErrorMessage, Article? Article)> CreateArticleAsync(string title, string content, string authorId, string? imagePath)
        {
            var article = new Article
            {
                Title = title,
                Content = content,
                AuthorId = authorId,
                CreatedAt = DateTime.Now,
                ImagePath = imagePath
            };

            await _unitOfWork.Articles.AddAsync(article);
            await _unitOfWork.SaveChangesAsync();

            return (true, null, article);
        }

        public async Task<Article?> GetArticleDetailsAsync(int id, string? userId)
        {
            return await _unitOfWork.Articles.GetByIdWithAuthorAndCommentsAsync(id);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateArticleAsync(int id, string title, string content, string userId)
        {
            var existingArticle = await _unitOfWork.Articles.GetByIdAsync(id);
            if (existingArticle == null)
            {
                return (false, "Article not found");
            }

            if (existingArticle.AuthorId != userId)
            {
                return (false, "Unauthorized");
            }

            existingArticle.Title = title;
            existingArticle.Content = content;
            existingArticle.UpdatedAt = DateTime.Now;

            await _unitOfWork.Articles.UpdateAsync(existingArticle);
            await _unitOfWork.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteArticleAsync(int id, string userId, bool isAdmin)
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
            if (article == null)
            {
                return (false, "Article not found");
            }

            if (article.AuthorId != userId && !isAdmin)
            {
                return (false, "Unauthorized");
            }

            await _unitOfWork.Articles.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, bool IsSaved, string Message)> ToggleSaveArticleAsync(int articleId, string userId)
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(articleId);
            if (article == null)
            {
                return (false, false, "Article not found");
            }

            var existingRead = await _unitOfWork.ArticleReads.GetByUserAndArticleAsync(userId, articleId);

            if (existingRead == null)
            {
                var articleRead = new ArticleRead
                {
                    ArticleId = articleId,
                    UserId = userId,
                    ReadAt = DateTime.Now
                };

                await _unitOfWork.ArticleReads.AddAsync(articleRead);
                await _unitOfWork.SaveChangesAsync();
                return (true, true, "Article saved!");
            }
            else
            {
                await _unitOfWork.ArticleReads.DeleteByUserAndArticleAsync(userId, articleId);
                await _unitOfWork.SaveChangesAsync();
                return (true, false, "Article unsaved!");
            }
        }
    }
}

