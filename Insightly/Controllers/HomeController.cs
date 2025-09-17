using Insightly.Models;
using Insightly.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Insightly.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IArticleRepository _articleRepository;

        public HomeController(ILogger<HomeController> logger, IArticleRepository articleRepository)
        {
            _logger = logger;
            _articleRepository = articleRepository;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _articleRepository.GetLatestAsync(3);
            return View(articles);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreArticles(int skip = 3, int take = 3)
        {
            var articles = await _articleRepository.GetLatestAsync(skip, take);
            var result = articles.Select(a => new
            {
                ArticleId = a.ArticleId,
                Title = a.Title,
                Content = a.Content,
                CreatedAt = a.CreatedAt,
                Author = new { Name = a.Author.Name }
            });

            return Json(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
