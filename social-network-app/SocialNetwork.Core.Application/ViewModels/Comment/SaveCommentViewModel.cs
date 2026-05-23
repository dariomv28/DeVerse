namespace SocialNetwork.Core.Application.ViewModels.Comment
{
    public class SaveCommentViewModel
    {
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime? Created { get; set; }
    }
}
