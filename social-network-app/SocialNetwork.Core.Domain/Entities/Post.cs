using SocialNetwork.Core.Domain.Common;

namespace SocialNetwork.Core.Domain.Entities
{
    public class Post : AuditableBaseEntity
    {
        public string UserId { get; set; }
        public string Content { get; set; }
        public string? Attachment { get; set; }
        public int LikeCount { get; set; }
        public int? SharedPostId { get; set; }

        //navegation properties
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<PostLike>? Likes { get; set; }
        public Post? SharedPost { get; set; }
        public ICollection<Post>? SharedByPosts { get; set; }
    }
}
