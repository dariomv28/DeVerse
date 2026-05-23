using SocialNetwork.Core.Application.ViewModels.Post;
using SocialNetwork.Core.Application.ViewModels.Reply;

namespace SocialNetwork.Core.Application.ViewModels.Comment
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
        public string Content { get; set; }
        public DateTime? Created { get; set; }
        public PostViewModel? Post { get; set; }
        public ICollection<ReplyViewModel>? Replies { get; set; }
    }
}
