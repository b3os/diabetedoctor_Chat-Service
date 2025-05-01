using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.Models;
using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class UserRepository(MongoDbContext context) : RepositoryBase<User>(context), IUserRepository
{
    public async Task<UpdateResult> UpdateUserAsync(IClientSessionHandle session, string id, User user,
        CancellationToken cancellationToken = default)
    {
        var updates = user.Changes.Select(change => Builders<User>.Update.Set(change.Key, change.Value)).ToList();

        var combineUpdate = Builders<User>.Update.Combine(updates);
        
        return await DbSet.UpdateOneAsync(session,
            Builders<User>.Filter.Eq(x => x.UserId.Id, id),
            combineUpdate,
            new UpdateOptions { IsUpsert = false }, cancellationToken
        );
    }
}