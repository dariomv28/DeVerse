namespace SocialNetwork.Core.Application.ViewModels.Message
{
    public class MessageViewModel
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public bool IsMine { get; set; }
    }
}