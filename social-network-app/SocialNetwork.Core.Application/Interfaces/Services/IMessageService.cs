using SocialNetwork.Core.Application.ViewModels.Message;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Interfaces.Services
{
    public interface IMessageService : IGenericService<SaveMessageViewModel, MessageViewModel, Message>
    {
        Task<SaveMessageViewModel> SendMessage(SaveMessageViewModel vm);

        Task<List<MessageViewModel>> GetConversationWithUser(string friendId);
    }
}