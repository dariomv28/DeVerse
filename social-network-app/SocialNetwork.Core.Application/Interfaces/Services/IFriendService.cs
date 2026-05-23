using SocialNetwork.Core.Application.ViewModels.Friend;
using SocialNetwork.Core.Application.ViewModels.Post;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Interfaces.Services
{
    public interface IFriendService : IGenericService<SaveFriendViewModel, FriendViewModel, Friend>
    {
        Task<List<PostViewModel>> GetAllFriendPostsViewModel();
        Task<List<FriendViewModel>> GetAllFriendViewModel();
        // Mới thêm ngày 23/12/2026 để xử lý chấp nhận lời mời kết bạn 12-14
        Task AcceptFriendRequest(int friendshipId);
        Task<List<FriendViewModel>> GetPendingFriendRequests();

    }
}
