using ChatService.Domain.ValueObjects;
using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class UserRepository(IMongoDbContext context) : RepositoryBase<User>(context), IUserRepository
{
    public async Task<BsonDocument?> GetUserWithHospital(UserId userId, CancellationToken cancellationToken)
    {
        var filter = Builders<User>.Filter.Eq(u => u.UserId, userId);
        var hospitalLookup = new EmptyPipelineDefinition<Hospital>()
            .Match(new BsonDocument
            {
                {
                    "$expr", new BsonDocument
                    {
                        {
                            "$and", new BsonArray
                            {
                                new BsonDocument("$eq", new BsonArray { "$hospital_id", "$$hospitalId" }),
                                new BsonDocument("$eq", new BsonArray{ "$is_deleted", false})
                            }
                        }
                    }
                }
            })
            .Limit(1);
        
        var projection = new BsonDocument
        {
            { "user_id", 1 },
            { "hospital_id", "$hospital.hospital_id"},
            { "role", 1},
        };

        var document = await DbSet.Aggregate()
            .Match(filter)
            .Lookup<Hospital, Hospital, IEnumerable<Hospital>, BsonDocument>(
                foreignCollection: context.Hospitals,
                let: new BsonDocument("hospitalId", "$hospital_id"),
                lookupPipeline: hospitalLookup,
                @as: "hospital")
            .Unwind("hospital", new AggregateUnwindOptions<BsonDocument>
            {
                PreserveNullAndEmptyArrays = true
            })
            .Project(projection)
            .FirstOrDefaultAsync(cancellationToken);
        
        return document;
    }
}