using SocialNetwork.Core.Domain.Common;

namespace SocialNetwork.Core.Domain.Entities
{
    public class Reply : AuditableBaseEntity
    {
        public int CommentId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }

        //navegation property
        public Comment? Comments { get; set; }
    }
}
