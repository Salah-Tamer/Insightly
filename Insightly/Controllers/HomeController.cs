using Insightly.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Insightly.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
           
            var articles = _context.Articles
                                   .Include(a => a.Author)
                                   .OrderByDescending(a => a.CreatedAt)
                                   .Take(3)
                                   .ToList();

            return View(articles);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreArticles(int skip = 3, int take = 3)
        {
            var articles = await _context.Articles
                                        .Include(a => a.Author)
                                        .OrderByDescending(a => a.CreatedAt)
                                        .Skip(skip)
                                        .Take(take)
                                        .Select(a => new
                                        {
                                            ArticleId = a.ArticleId,
                                            Title = a.Title,
                                            Content = a.Content,
                                            CreatedAt = a.CreatedAt,
                                            Author = new { Name = a.Author.Name }
                                        })
                                        .ToListAsync();

            return Json(articles);
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
