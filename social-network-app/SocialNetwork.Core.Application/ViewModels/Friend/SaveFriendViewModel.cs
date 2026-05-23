using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Core.Application.ViewModels.Friend
{
    public class SaveFriendViewModel
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "You must enter the username")]
        [DataType(DataType.Text)]
        public string Username { get; set; }
        public string? UserSenderId { get; set; }
        public string? UserReceptorId { get; set; }
        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
