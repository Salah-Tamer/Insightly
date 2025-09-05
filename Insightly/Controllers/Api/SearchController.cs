using Insightly.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Controllers.Api
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Articles/Search?query=health
        public async Task<IActionResult> Index(string query)
        {
            var articles = await _context.Articles
                .Where(a => string.IsNullOrEmpty(query) || a.Title.Contains(query))
                .ToListAsync();

            return View(articles); 
        }
    }
}
