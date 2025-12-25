using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Insightly.Repositories;
using Insightly.Services;
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
        private readonly IChatService _chatService;

        public ChatController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IChatService chatService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _chatService = chatService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var (chats, followers, following) = await _chatService.GetUserChatDataAsync(user.Id);

            ViewBag.Followers = followers;
            ViewBag.Following = following;

            return View(chats);
        }
    }
}