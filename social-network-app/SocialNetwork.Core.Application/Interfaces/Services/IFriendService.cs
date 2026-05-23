using SocialNetwork.Core.Application.ViewModels.Friend;
using SocialNetwork.Core.Application.ViewModels.Post;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Interfaces.Services
{
    public interface IFriendService : IGenericService<SaveFriendViewModel, FriendViewModel, Friend>
    {
        Task<List<PostViewModel>> GetAllFriendPostsViewModel();
        Task<List<FriendViewModel>> GetAllFriendViewModel();
    }
}
