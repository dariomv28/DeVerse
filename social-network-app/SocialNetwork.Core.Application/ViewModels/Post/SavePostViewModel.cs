using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Core.Application.ViewModels.Post
{
    public class SavePostViewModel
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; }
        public string? Attachment { get; set; }
        public DateTime? Created { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Photo")]
        public IFormFile? File { get; set; }
    }
}
