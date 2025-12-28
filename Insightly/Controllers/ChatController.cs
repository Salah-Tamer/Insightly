using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Insightly.Repositories;
using Insightly.Services;
using Insightly.ViewModels;
using Microsoft.AspNetCore.Identity;
using Insightly.Models;
using System.Linq;

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

            // Map to ViewModels using AutoMapper
            var chatViewModels = _mapper.Map<List<ChatViewModel>>(chats);
            
            // Extract users from Follow entities and map to ViewModels
            var followerUsers = followers.Select(f => f.Follower).Where(u => u != null);
            var followingUsers = following.Select(f => f.Following).Where(u => u != null);
            var followerViewModels = _mapper.Map<List<UserListItemViewModel>>(followerUsers);
            var followingViewModels = _mapper.Map<List<UserListItemViewModel>>(followingUsers);

            ViewBag.Followers = followerViewModels;
            ViewBag.Following = followingViewModels;

            return View(chatViewModels);
        }
    }
}