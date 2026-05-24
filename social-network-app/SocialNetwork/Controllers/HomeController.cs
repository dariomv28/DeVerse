using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Message;
using SocialNetwork.Core.Application.ViewModels.Post;
using SocialNetwork.Middlewares;

namespace SocialNetwork.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPostService _postService;
        private readonly IMessageService _messageService;
        private readonly IFriendService _friendService;
        private readonly ValidateUserSession _validateUserSession;

        public HomeController(
            IPostService postService,
            IMessageService messageService,
            IFriendService friendService,
            ValidateUserSession validateUserSession)
        {
            _postService = postService;
            _messageService = messageService;
            _friendService = friendService;
            _validateUserSession = validateUserSession;
        }

        public async Task<IActionResult> Index(int? id, string friendId)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            ViewBag.Posts = await _postService.GetAllViewModelWithInclude();
            ViewBag.Friends = await _friendService.GetAllFriendViewModel();
            ViewBag.FriendId = friendId;

            if (!string.IsNullOrEmpty(friendId))
            {
                ViewBag.Messages = await _messageService.GetConversationWithUser(friendId);
            }
            else
            {
                ViewBag.Messages = new List<MessageViewModel>();
            }

            if (id.HasValue)
            {
                var post = await _postService.GetByIdSaveViewModel(id.Value);
                return View(post);
            }

            return View(new SavePostViewModel());
        }

        public async Task<IActionResult> GetConversation(string friendId)
        {
            var messages = await _messageService.GetConversationWithUser(friendId);
            return Json(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            SaveMessageViewModel vm = new()
            {
                ReceiverId = receiverId,
                Content = content
            };

            var result = await _messageService.SendMessage(vm);

            if (result.HasError)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }
    }
}