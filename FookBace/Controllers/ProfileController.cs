using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Application.ViewModels.User;
using SocialNetwork.Core.Application.Helpers;
using SocialNetwork.Core.Application.Dtos.Account;
using SocialNetwork.Core.Application.Interfaces.Services;

namespace SocialNetwork.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        // private readonly AuthenticationResponse userViewModel;
        private readonly IUserService _userService;
        private readonly IPostService _postService;

        public ProfileController(
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            IPostService postService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _postService = postService;
            // userViewModel = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
        }

        public async Task<IActionResult> Index(string? id)
        {
            var loggedUser = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
            if (loggedUser == null)
            {
                return RedirectToRoute(new
                {
                    controller = "User",
                    action = "Login"
                });
            }

            string targetUserId = string.IsNullOrWhiteSpace(id) ? loggedUser.Id : id;
            SaveUserViewModel profileUser = await _userService.GetByIdAsync(targetUserId);

            if (profileUser.HasError)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "Index"
                });
            }

            ViewBag.IsCurrentUser = loggedUser.Id == targetUserId;
            ViewBag.Posts = await _postService.GetPostsByUserViewModelWithIncludes(targetUserId);

            return View(profileUser);
        }

        public async Task<IActionResult> Edit()
        {
            var userViewModel = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
            if (userViewModel == null)
            {
                return RedirectToRoute(new
                {
                    controller = "User",
                    action = "Index"
                });
            }
            SaveUserViewModel vm = await _userService.GetByIdAsync(userViewModel.Id);
            return View("Edit", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveUserViewModel vm)
        {
            var userViewModel = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
            if (userViewModel == null)
            {
                return RedirectToRoute(new
                {
                    controller = "User",
                    action = "Index"
                });
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            SaveUserViewModel userVm = await _userService.GetByIdAsync(userViewModel.Id);
            if (userVm.HasError)
            {
                vm.HasError = userVm.HasError;
                vm.Error = userVm.Error;
                return View(vm);
            }

            vm.ProfilePicture = UploadFilesHelper.UploadFile(vm.File, vm.Id, "Posts", true, userVm.ProfilePicture);
            await _userService.UpdateAsync(vm, userVm.Id);

            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            var userViewModel = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
            if (userViewModel == null)
            {
                return RedirectToRoute(new
                {
                    controller = "User",
                    action = "Index"
                });
            }

            SaveUserViewModel userVm =
                await _userService.GetByIdAsync(userViewModel.Id);

            if (!string.IsNullOrEmpty(userVm.ProfilePicture))
            {
                string basePath = $"/Images/Users/{userVm.Id}";
                string path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    $"wwwroot{basePath}",
                    userVm.ProfilePicture);

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            userVm.ProfilePicture = null;

            await _userService.UpdateAsync(userVm, userVm.Id);

            return RedirectToAction("Edit");
        }
    }
}
