using ChatService.Contract.Helpers;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class ParticipantRepository(IMongoDbContext context)
    : RepositoryBase<Participant>(context), IParticipantRepository
{
    public async Task<List<BsonDocument>> CheckDuplicatedParticipantsAsync(ObjectId conversationId, IEnumerable<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<Participant>.Filter;
        var filter = builder.And(
            builder.Eq(participant => participant.ConversationId, conversationId),
            builder.In(participant => participant.UserId.Id, userIds)
        );

        var userLookup = new EmptyPipelineDefinition<User>()
            .Match(new BsonDocument
            {
                {
                    "$expr", new BsonDocument("$eq", new BsonArray { "$user_id", "$$userId" })
                }
            })
            .Limit(1);
        
        var projection = new BsonDocument
        {
            { "user_id", "$user_id" },
            { "full_name", "$user.display_name" },
            { "avatar", "$user.avatar.public_url" },
            { "status", 1 },
            { "is_deleted", 1 }
        };
        
        var dupParticipants = await DbSet.Aggregate()
            .Match(filter)
            .Lookup<User, User, IEnumerable<User>, BsonDocument>(
                foreignCollection: context.Users,
                let: new BsonDocument("userId", "$user_id"),
                lookupPipeline: userLookup,
                @as: "user")
            .Unwind("user", new AggregateUnwindOptions<BsonDocument>
            {
                PreserveNullAndEmptyArrays = true
            })
            .Project(projection)
            .ToListAsync(cancellationToken);

        return dupParticipants;
    }

    public async Task<UpdateResult> RejoinToConversationAsync(IClientSessionHandle session, ObjectId? conversationId, IEnumerable<UserId> participantIds,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<Participant>.Filter;
        var filter = builder.And(
            builder.Eq(p => p.ConversationId, conversationId),
            builder.In(p => p.UserId, participantIds)
            );
        var update = Builders<Participant>.Update
            .Set(p => p.IsDeleted, false)
            .Set(p => p.ModifiedDate, CurrentTimeService.GetCurrentTime());
        var options = new UpdateOptions { IsUpsert = false };
        
        return await DbSet.UpdateManyAsync(session, filter, update, options, cancellationToken);
    }
    
    // public async Task<Participant?> GetParticipantInfo(ObjectId conversationId, string participantId, CancellationToken cancellationToken = default)
    // {
    //     var builder = Builders<Participant>.Filter;
    //     var filter = builder.And(
    //         builder.Eq(participant => participant.ConversationId, conversationId),
    //         builder.Eq(participant => participant.UserId.Id, participantId));
    //     
    //     var participant = await DbSet.Aggregate()
    //         .Match(filter)
    //      
    //         .FirstOrDefaultAsync(cancellationToken);
    //     
    //     return participant;
    // }
}