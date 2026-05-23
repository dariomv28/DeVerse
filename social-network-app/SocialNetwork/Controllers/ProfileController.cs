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
        private readonly AuthenticationResponse userViewModel;
        private readonly IUserService _userService;

        public ProfileController(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            userViewModel = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
        }

        public async Task<IActionResult> Edit()
        {
            SaveUserViewModel vm = await _userService.GetByIdAsync(userViewModel.Id);
            return View("Edit", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveUserViewModel vm)
        {
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
    }
}
