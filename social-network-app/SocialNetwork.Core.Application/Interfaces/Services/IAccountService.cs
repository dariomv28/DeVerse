using SocialNetwork.Core.Application.Dtos.Account;

namespace SocialNetwork.Core.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);
        Task<AuthenticationResponse> GetUserByIdAsync(string id);
        Task<AuthenticationResponse> GetUserByUsernameAsync(string username);
        Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string origin);
        Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
        Task SignOutAsync();
        Task<string> UpdateUserAsync(RegisterRequest request, string userId);
    }
}