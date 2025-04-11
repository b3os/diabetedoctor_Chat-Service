using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.Models;

namespace ChatService.Persistence.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository 
{
    public UserRepository(MongoDbContext context) : base(context)
    {
    }
}