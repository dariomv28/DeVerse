using SocialNetwork.Core.Application.Interfaces.Repositories;
using SocialNetwork.Core.Domain.Entities;
using SocialNetwork.Infrastructure.Persistence.Contexts;

namespace SocialNetwork.Infrastructure.Persistence.Repositories
{
    public class FriendRepository : GenericRepository<Friend>, IFriendRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public FriendRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
