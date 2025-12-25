using AutoMapper;
using Insightly.Models;
using Insightly.Repositories;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    public class SearchController : Controller
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IMapper _mapper;

        public SearchController(IArticleRepository articleRepository, IMapper mapper)
        {
            _articleRepository = articleRepository;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> SearchAjax(string query)
        {
            var articles = await _articleRepository.SearchAsync(query);
            var articleDtos = _mapper.Map<List<ArticleJsonDto>>(articles);

            return Json(new { 
                articles = articleDtos,
                query = query,
                hasQuery = !string.IsNullOrWhiteSpace(query),
                count = articleDtos.Count
            });
        }
    }
}
