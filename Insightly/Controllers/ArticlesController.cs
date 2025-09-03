using Microsoft.AspNetCore.Mvc;

namespace Insightly.Controllers
{
    public class ArticlesController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }
    }
}
