using SocialNetwork.Core.Domain.Common;

namespace SocialNetwork.Core.Domain.Entities
{
    public class Friend : AuditableBaseEntity
    {
        public string UserSenderId { get; set; }
        public string UserReceptorId { get; set; }
        //public DateTime CreatedDate { get; set; }
        
        // Trạng thái lời mời: Pending / Accepted
        public string Status { get; set; }
    }
}
