using SocialNetwork.Core.Application.ViewModels.Comment;

namespace SocialNetwork.Core.Application.ViewModels.Post
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
        public string Content { get; set; }
        public string? Attachment { get; set; }
        public DateTime? Created { get; set; }
        public ICollection<CommentViewModel>? Comments { get; set; }
    }
}
