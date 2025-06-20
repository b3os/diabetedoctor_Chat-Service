using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class ParticipantRepository(IMongoDbContext context)
    : RepositoryBase<Participant>(context), IParticipantRepository
{
    public async Task<List<User>> CheckDuplicatedParticipantAsync(ObjectId groupId, IEnumerable<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<Participant>.Filter;
        var filter = builder.And(
            builder.Eq(participant => participant.ConversationId, groupId),
            builder.In(participant => participant.UserId.Id, userIds)
        );

        var dupParticipants = await DbSet.Aggregate()
            .Match(filter)
            .Lookup(
                foreignCollectionName: nameof(User),
                localField: "userId",
                foreignField: "userId",
                @as: "user")
            .Unwind("user")
            .Project(new BsonDocument()
            {
                { "_id", 1 },
                { "user_id", 1 },
                { "fullname", 1 },
                { "avatar", 1 }
            })
            .As<User>()
            .ToListAsync(cancellationToken);

        return dupParticipants;
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