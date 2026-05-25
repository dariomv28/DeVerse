using SocialNetwork.Core.Application.ViewModels.Comment;

namespace SocialNetwork.Core.Application.ViewModels.Reply
{
    public class ReplyViewModel
    {
        public int CommentId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }

        //navegation property
        public CommentViewModel? Comment { get; set; }
    }
}
