namespace SocialNetwork.Core.Application.ViewModels.User
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public bool IsVerified { get; set; }
        public bool HasError { get; set; }
        public string Error { get; set; }
    }
}


