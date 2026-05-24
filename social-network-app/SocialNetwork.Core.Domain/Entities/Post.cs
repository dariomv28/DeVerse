using SocialNetwork.Core.Domain.Common;

namespace SocialNetwork.Core.Domain.Entities
{
    public class Post : AuditableBaseEntity
    {
        public string UserId { get; set; }
        public string Content { get; set; }
        public string? Attachment { get; set; }

        //navegation properties
        public ICollection<Comment>? Comments { get; set; }
    }
}
