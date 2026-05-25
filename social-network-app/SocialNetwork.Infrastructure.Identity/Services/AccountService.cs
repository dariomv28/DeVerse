using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SocialNetwork.Core.Application.Dtos.Account;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Infrastructure.Identity.Entities;
using System.Text;

namespace SocialNetwork.Infrastructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            AuthenticationResponse response = new();

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                response.HasError = true;
                response.Error = $"No Accounts registered with {request.Username}";
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = $"Invalid credential for {request.Username}";
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Error = $"Account no confirmed for {request.Username}";
                return response;
            }

            response.Id = user.Id;
            response.Username = user.UserName;
            response.Email = user.Email;
            response.IsVerified = user.EmailConfirmed;
            response.ProfilePicture = user.ProfilePicture;

            return response;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string origin)
        {
            RegisterResponse response = new()
            {
                HasError = false
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(request.Username);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Error = $"Username '{request.Username}' is already taken.";
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Error = $"Email '{request.Email}' is already registered.";
                return response;
            }

            var user = new ApplicationUser
            {
                UserName = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.Phone,
                ProfilePicture = request.ProfilePicture
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                response.Id = user.Id;
                var verificationUri = await SendVerificationEmailUri(user, origin);
                await _emailService.SendAsync(new Core.Application.Dtos.Email.EmailRequest
                {
                    To = user.Email,
                    Body = $"Please confirm your account visiting this URL {verificationUri}",
                    Subject = "Confirm Registration"
                });
            }
            else
            {
                response.HasError = true;
                response.Error = $"An error ocurred trying to register the user.";
                return response;
            }

            return response;
        }

        public async Task<string> UpdateUserAsync(RegisterRequest request, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "User not found.";
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.Phone;
            user.ProfilePicture = request.ProfilePicture;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return $"Update succesfully.";
            }
            else
            {
                return $"An error ocurred trying to update the user.";
            }
        }

        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "No accounts registered with this user";
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return $"Account confirm for {user.Email}. You can now use the app.";
            }
            else
            {
                return $"An error ocurred while confirming {user.Email}.";
            }
        }

        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            ForgotPasswordResponse response = new()
            {
                HasError = false
            };

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                response.HasError = true;
                response.Error = $"No Accounts registered with {user.UserName}";
                return response;
            }

            var verificationUri = await SendForgotPasswordUri(user, origin);
            await _emailService.SendAsync(new Core.Application.Dtos.Email.EmailRequest
            {
                To = user.Email,
                Body = $"Please reset your password visiting this URL {verificationUri}",
                Subject = "Reset Registration"
            });

            return response;
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            ResetPasswordResponse response = new()
            {
                HasError = false
            };

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                response.HasError = true;
                response.Error = $"No Accounts registered with {user.UserName}";
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = $"An error occurred while reset password";
                return response;
            }

            return response;
        }

        public async Task<AuthenticationResponse> GetUserByIdAsync(string id)
        {
            AuthenticationResponse response = new();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                response.HasError = true;
                response.Error = "User not found";
                return response;
            }

            response.Id = user.Id;
            response.FirstName = user.FirstName;
            response.LastName = user.LastName;
            response.Username = user.UserName;
            response.Email = user.Email;
            response.Phone = user.PhoneNumber;
            response.IsVerified = user.EmailConfirmed;
            response.ProfilePicture = user.ProfilePicture;

            return response;
        }

        public async Task<AuthenticationResponse> GetUserByUsernameAsync(string username)
        {
            AuthenticationResponse response = new();

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                response.HasError = true;
                response.Error = $"User not found";
                return response;
            }

            response.Id = user.Id;
            response.FirstName = user.FirstName;
            response.LastName = user.LastName;
            response.Username = user.UserName;
            response.Email = user.Email;
            response.Phone = user.PhoneNumber;
            response.IsVerified = user.EmailConfirmed;
            response.ProfilePicture = user.ProfilePicture;

            return response;
        }


        private async Task<string> SendVerificationEmailUri(ApplicationUser user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "User/ConfirmEmail";
            var Uri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(Uri.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "token", code);

            return verificationUri;
        }

        private async Task<string> SendForgotPasswordUri(ApplicationUser user, string origin)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "User/ResetPassword";
            var Uri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(Uri.ToString(), "token", code);

            return verificationUri;
        }

    }

}
