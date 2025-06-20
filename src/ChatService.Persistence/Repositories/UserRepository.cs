namespace ChatService.Persistence.Repositories;

public class UserRepository(IMongoDbContext context) : RepositoryBase<User>(context), IUserRepository
{
   
}