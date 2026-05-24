using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Message;
using SocialNetwork.Middlewares;

namespace SocialNetwork.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly ValidateUserSession _validateUserSession;

        public MessageController(
            IMessageService messageService,
            ValidateUserSession validateUserSession
        )
        {
            _messageService = messageService;
            _validateUserSession = validateUserSession;
        }

        public async Task<IActionResult> Index(string friendId)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            ViewBag.FriendId = friendId;

            var messages = await _messageService.GetConversationWithUser(friendId);

            return View(messages);
        }
    }
}