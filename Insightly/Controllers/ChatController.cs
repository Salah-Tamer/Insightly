using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Insightly.Repositories;
using Microsoft.AspNetCore.Identity;
using Insightly.Models;
using System.Linq;

namespace Insightly.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            
            var chats = await _unitOfWork.Chats.GetChats(user.Id);

            
            var followers = await _unitOfWork.Follows.GetFollowersAsync(user.Id);

            var following = await _unitOfWork.Follows.GetFollowingAsync(user.Id);

            ViewBag.Followers = followers;
            ViewBag.Following = following;

            return View(chats);
        }
    }
}