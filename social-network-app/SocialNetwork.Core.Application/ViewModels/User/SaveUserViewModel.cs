using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Core.Application.ViewModels.User
{
    public class SaveUserViewModel
    {
        public string? Id { get; set; }
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "First name")]
        [DataType(DataType.Text)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Last name")]
        [DataType(DataType.Text)]
        public string? LastName { get; set; }

        [Display(Name = "Username")]
        [DataType(DataType.Text)]
        public string? Username { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Phone number")]
        public string? Phone { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Profile picture")]
        public IFormFile? File { get; set; }
        public string? ProfilePicture { get; set; }
        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
