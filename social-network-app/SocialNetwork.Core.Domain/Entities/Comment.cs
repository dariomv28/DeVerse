using SocialNetwork.Core.Domain.Common;

namespace SocialNetwork.Core.Domain.Entities
{
    public class Comment : AuditableBaseEntity
    {
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }

        //navegation properties
        public Post? Post { get; set; }
        public ICollection<Reply>? Replies { get; set; }
    }
}
