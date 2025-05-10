using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.Models;
using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class UserRepository(IMongoDbContext context) : RepositoryBase<User>(context), IUserRepository
{
}