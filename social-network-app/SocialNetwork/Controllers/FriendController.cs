using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Friend;
using SocialNetwork.Core.Application.ViewModels.User;
using SocialNetwork.Middlewares;

namespace SocialNetwork.Controllers
{
    [Authorize]
    public class FriendController : Controller
    {
        private readonly IUserService _userService;
        private readonly IFriendService _friendService;
        private readonly ValidateUserSession _validateUserSession;

        public FriendController(IUserService userService, IFriendService friendService, ValidateUserSession validateUserSession)
        {
            _userService = userService;
            _friendService = friendService;
            _validateUserSession = validateUserSession;
        }

        public async Task<IActionResult> Index()
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            ViewBag.Friends = await _friendService.GetAllFriendViewModel();
            ViewBag.Posts = await _friendService.GetAllFriendPostsViewModel();

            return View(new SaveFriendViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(SaveFriendViewModel vm)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            UserViewModel userVm = await _userService.GetByUsernameAsync(vm.Username);
            if (userVm.HasError == true)
            {
                vm.HasError = userVm.HasError;
                vm.Error = userVm.Error;
                return View("Index", vm);
            }

            vm.UserReceptorId = userVm.Id;
            SaveFriendViewModel friendVm = await _friendService.Add(vm);
            if (friendVm.HasError == true)
            {
                vm.HasError = friendVm.HasError;
                vm.Error = friendVm.Error;
                return View("Index", vm);
            }

            return RedirectToRoute(new { controller = "Friend", action = "Index" });
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            var friend = await _friendService.GetByIdSaveViewModel(id);
            return View("Delete", friend);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFriend(SaveFriendViewModel vm)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            await _friendService.Delete(vm.Id.Value);
            return RedirectToRoute(new { controller = "Friend", action = "Index" });

        }
    }
}
