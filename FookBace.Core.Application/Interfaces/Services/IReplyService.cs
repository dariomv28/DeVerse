using SocialNetwork.Core.Application.ViewModels.Reply;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Interfaces.Services
{
    public interface IReplyService : IGenericService<SaveReplyViewModel, ReplyViewModel, Reply>
    {

    }
}
