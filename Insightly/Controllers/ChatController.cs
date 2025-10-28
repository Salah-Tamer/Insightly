using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Insightly.Repositories; 
using Microsoft.AspNetCore.Identity;
using Insightly.Models; 

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

        
        public async Task<IActionResult> Index(string? receiverId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

          
           
                var receiver = await _userManager.FindByIdAsync(receiverId);
                ViewBag.ReceiverName = receiver?.UserName;
                ViewBag.CurrentUserId = currentUser?.Id;
                ViewBag.ReceiverId = receiverId;

                return View();
           
        }
    }
}
