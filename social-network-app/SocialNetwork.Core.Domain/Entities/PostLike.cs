using SocialNetwork.Core.Domain.Common;

namespace SocialNetwork.Core.Domain.Entities
{
    public class PostLike : AuditableBaseEntity
    {
        public int PostId { get; set; }
        public string UserId { get; set; }

        // navegation properties
        public Post? Post { get; set; }
    }
}
