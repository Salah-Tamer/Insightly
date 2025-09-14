using Insightly.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Search - Browse all articles or search results
        public async Task<IActionResult> Index(string query)
        {
            var articlesQuery = _context.Articles
                .Include(a => a.Author)
                .AsQueryable();

            // If search query is provided, filter articles
            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchTerm = query.Trim().ToLower();
                articlesQuery = articlesQuery.Where(a => 
                    a.Title.ToLower().Contains(searchTerm) ||
                    a.Content.ToLower().Contains(searchTerm) ||
                    a.Author.Name.ToLower().Contains(searchTerm)
                );
            }

            var articles = await articlesQuery
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.SearchQuery = query;
            ViewBag.HasSearchQuery = !string.IsNullOrWhiteSpace(query);

            return View(articles); 
        }

        [HttpGet]
        public async Task<IActionResult> SearchAjax(string query)
        {
            var articlesQuery = _context.Articles
                .Include(a => a.Author)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchTerm = query.Trim().ToLower();
                articlesQuery = articlesQuery.Where(a => 
                    a.Title.ToLower().Contains(searchTerm) ||
                    a.Content.ToLower().Contains(searchTerm) ||
                    a.Author.Name.ToLower().Contains(searchTerm)
                );
            }

            var articles = await articlesQuery
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new {
                    articleId = a.ArticleId,
                    title = a.Title,
                    content = a.Content,
                    createdAt = a.CreatedAt,
                    author = new {
                        name = a.Author.Name,
                        id = a.AuthorId
                    }
                })
                .ToListAsync();

            return Json(new { 
                articles = articles,
                query = query,
                hasQuery = !string.IsNullOrWhiteSpace(query),
                count = articles.Count
            });
        }
    }
}
