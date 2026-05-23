using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Post;
using SocialNetwork.Middlewares;

namespace SocialNetwork.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPostService _postService;
        private readonly ValidateUserSession _validateUserSession;

        public HomeController(IPostService postService, ValidateUserSession validateUserSession)
        {
            _postService = postService;
            _validateUserSession = validateUserSession;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            if (id.HasValue)
            {
                var post = await _postService.GetByIdSaveViewModel(id.Value);
                ViewBag.Posts = await _postService.GetAllViewModelWithInclude();
                return View(post);
            }
            else
            {
                ViewBag.Posts = await _postService.GetAllViewModelWithInclude();
                return View(new SavePostViewModel());
            }
        }
    }
}
