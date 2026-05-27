using SocialNetwork.Core.Application.Dtos.Account;
using SocialNetwork.Core.Application.ViewModels.User;

namespace SocialNetwork.Core.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<string> ConfirmEmailAsync(string userId, string token);
        Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordViewModel vm, string origin);
        Task<SaveUserViewModel> GetByIdAsync(string id);
        Task<UserViewModel> GetByUsernameAsync(string username);
        Task<AuthenticationResponse> LoginAsync(LoginViewModel vm);
        Task<RegisterResponse> RegisterAsync(SaveUserViewModel vm, string origin);
        Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordViewModel vm);
        Task SignOutAsync();
        Task UpdateAsync(SaveUserViewModel vm, string id);
    }
}
