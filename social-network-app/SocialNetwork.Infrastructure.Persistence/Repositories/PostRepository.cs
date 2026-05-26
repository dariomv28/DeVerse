using Microsoft.EntityFrameworkCore;
using SocialNetwork.Core.Application.Interfaces.Repositories;
using SocialNetwork.Core.Domain.Entities;
using SocialNetwork.Infrastructure.Persistence.Contexts;

namespace SocialNetwork.Infrastructure.Persistence.Repositories
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PostRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsLiked, int LikeCount)> ToggleLikeAsync(int postId, string userId)
        {
            Post? post = await _dbContext.Posts
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                throw new InvalidOperationException("Post not found");
            }

            PostLike? existingLike = post.Likes?.FirstOrDefault(l => l.UserId == userId);

            if (existingLike != null)
            {
                _dbContext.PostLikes.Remove(existingLike);
                post.LikeCount = Math.Max(0, post.LikeCount - 1);

                await _dbContext.SaveChangesAsync();
                return (false, post.LikeCount);
            }

            PostLike like = new()
            {
                PostId = postId,
                UserId = userId
            };

            await _dbContext.PostLikes.AddAsync(like);
            post.LikeCount += 1;

            await _dbContext.SaveChangesAsync();
            return (true, post.LikeCount);
        }
    }
}
