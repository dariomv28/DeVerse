using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Application.Dtos.Account;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.User;
using SocialNetwork.Core.Application.Helpers;
using SocialNetwork.Middlewares;

namespace WebApp.SocialNetwork.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [ServiceFilter(typeof(LoginAuthorize))]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [ServiceFilter(typeof(LoginAuthorize))]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            AuthenticationResponse userVm = await _userService.LoginAsync(vm);
            if (userVm != null && userVm.HasError != true)
            {
                HttpContext.Session.Set<AuthenticationResponse>("user", userVm);
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }
            else
            {
                vm.HasError = userVm.HasError;
                vm.Error = userVm.Error;
                return View(vm);
            }
        }

        public async Task<IActionResult> LogOut()
        {
            await _userService.SignOutAsync();
            HttpContext.Session.Remove("user");
            return RedirectToRoute(new { controller = "User", action = "Login" });
        }

        [ServiceFilter(typeof(LoginAuthorize))]
        public IActionResult Register()
        {
            return View(new SaveUserViewModel());
        }

        [HttpPost]
        [ServiceFilter(typeof(LoginAuthorize))]
        public async Task<IActionResult> Register(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var origin = Request.Headers["origin"];
            RegisterResponse response = await _userService.RegisterAsync(vm, origin);
            if (response.HasError)
            {
                vm.HasError = response.HasError;
                vm.Error = response.Error;
                return View(vm);
            }

            if (response.Id != null)
            {
                vm.ProfilePicture = UploadFilesHelper.UploadFile(vm.File, response.Id);
                await _userService.UpdateAsync(vm, response.Id);
            }

            return RedirectToRoute(new { controller = "User", action = "Login" });
        }

        [ServiceFilter(typeof(LoginAuthorize))]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            string response = await _userService.ConfirmEmailAsync(userId, token);
            return View("ConfirmEmail", response);
        }

        [ServiceFilter(typeof(LoginAuthorize))]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ServiceFilter(typeof(LoginAuthorize))]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var origin = Request.Headers["origin"];
            ForgotPasswordResponse response = await _userService.ForgotPasswordAsync(vm, origin);

            if (response.HasError)
            {
                vm.HasError = response.HasError;
                vm.Error = response.Error;
                return View(vm);
            }

            return RedirectToRoute(new { controller = "User", action = "Login" });
        }

        [ServiceFilter(typeof(LoginAuthorize))]
        public IActionResult ResetPassword(string token)
        {
            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ServiceFilter(typeof(LoginAuthorize))]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            ResetPasswordResponse response = await _userService.ResetPasswordAsync(vm);
            if (response.HasError)
            {
                vm.HasError = response.HasError;
                vm.Error = response.Error;
                return View(vm);
            }

            return RedirectToRoute(new { controller = "User", action = "Login" });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
