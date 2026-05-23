using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Core.Application.ViewModels.User
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "You must enter the email")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must enter a token")]
        [DataType(DataType.Text)]
        public string Token { get; set; }

        [Required(ErrorMessage = "You must enter a password")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
