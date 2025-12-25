using AutoMapper;
using Insightly.Repositories;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Insightly.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IArticleRepository _articleRepository;
        private readonly IMapper _mapper;

        public HomeController(ILogger<HomeController> logger, IArticleRepository articleRepository, IMapper mapper)
        {
            _logger = logger;
            _articleRepository = articleRepository;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _articleRepository.GetLatestAsync(3);
            var viewModels = _mapper.Map<List<ArticleListItemViewModel>>(articles);

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreArticles(int skip = 3, int take = 3)
        {
            var articles = await _articleRepository.GetLatestAsync(skip, take);
            var result = _mapper.Map<List<ArticleJsonDto>>(articles);

            return Json(result);
        }

        // Removed unused Privacy action

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
