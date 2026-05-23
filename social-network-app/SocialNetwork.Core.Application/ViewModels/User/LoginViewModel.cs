using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Core.Application.ViewModels.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "You must enter a username")]
        [Display(Name = "Username")]
        [DataType(DataType.Text)]
        public string Username { get; set; }
        [Required(ErrorMessage = "You must enter a password")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
