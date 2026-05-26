using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Interfaces.Repositories
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<(bool IsLiked, int LikeCount)> ToggleLikeAsync(int postId, string userId);
    }
}
