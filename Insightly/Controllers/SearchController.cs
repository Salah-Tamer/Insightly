using Insightly.Models;
using Insightly.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    public class SearchController : Controller
    {
        private readonly IArticleRepository _articleRepository;

        public SearchController(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository;
        }


        [HttpGet]
        public async Task<IActionResult> SearchAjax(string query)
        {
            var articles = await _articleRepository.SearchAsync(query);
            var result = articles.Select(a => new {
                articleId = a.ArticleId,
                title = a.Title,
                content = a.Content,
                createdAt = a.CreatedAt,
                author = new {
                    name = a.Author.Name,
                    id = a.AuthorId
                }
            }).ToList();

            return Json(new { 
                articles = result,
                query = query,
                hasQuery = !string.IsNullOrWhiteSpace(query),
                count = result.Count
            });
        }
    }
}
