using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Insightly.Services;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Identity;
using Insightly.Models;

namespace Insightly.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IChatService _chatService;
        private readonly IMapper _mapper;

        public ChatController(UserManager<ApplicationUser> userManager, IChatService chatService, IMapper mapper)
        {
            _userManager = userManager;
            _chatService = chatService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var (chats, followers, following) = await _chatService.GetUserChatDataAsync(user.Id);

            var chatViewModels = _mapper.Map<List<ChatViewModel>>(chats);

            ViewBag.Followers = followers;
            ViewBag.Following = following;

            return View(chatViewModels);
        }
    }
}