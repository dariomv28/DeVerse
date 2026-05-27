using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Core.Application.ViewModels.Message
{
    public class SaveMessageViewModel
    {
        public int? Id { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }

        [Required]
        public string Content { get; set; }

        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}