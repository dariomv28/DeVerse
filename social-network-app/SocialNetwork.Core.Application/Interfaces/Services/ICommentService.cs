using SocialNetwork.Core.Application.ViewModels.Comment;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Interfaces.Services
{
    public interface ICommentService : IGenericService<SaveCommentViewModel, CommentViewModel, Comment>
    {

    }
}
