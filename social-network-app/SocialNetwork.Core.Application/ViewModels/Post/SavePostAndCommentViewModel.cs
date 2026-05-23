using SocialNetwork.Core.Application.ViewModels.Comment;

namespace SocialNetwork.Core.Application.ViewModels.Post
{
    public class SavePostAndCommentViewModel
    {
        public SavePostViewModel SavePostViewModel { get; set; } = new();
        public SaveCommentViewModel SaveCommentViewModel { get; set; } = new();
    }
}
