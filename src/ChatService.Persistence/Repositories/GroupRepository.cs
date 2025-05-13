
using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class GroupRepository(IMongoDbContext context) : RepositoryBase<Group>(context), IGroupRepository
{
    public async Task<BsonDocument?> CheckDuplicatedUsersAsync(ObjectId groupId, IEnumerable<string> userIds, CancellationToken cancellationToken = default)
    {
        var userLookupPipeline = new EmptyPipelineDefinition<User>()
            .Match(new BsonDocument("$expr",
                new BsonDocument("$in", new BsonArray
                {
                    "$user_id._id", "$$dupUserIds"
                })
            ))
            .As<User, User, BsonDocument>();
        
        var documents = await DbSet.Aggregate()
            .Match(group => group.Id == groupId)
            .Unwind<Group, BsonDocument>(group => group.Members)
            .Match(Builders<BsonDocument>.Filter.In("members.user_id._id", userIds))
            .Group(new BsonDocument
            {
                { "_id", "$_id" },
                { "matchedUserIds", new BsonDocument("$push", "$members.user_id._id") },
                { "matchCount", new BsonDocument("$sum", 1) }
            })
            .Lookup<User, BsonDocument, IEnumerable<BsonDocument>, BsonDocument>(
                foreignCollection: context.Users,
                let: new BsonDocument("dupUserIds", "$matchedUserIds"),
                lookupPipeline: userLookupPipeline,
                @as: "matchedUsers")
            .Project(new BsonDocument
            {
                { "_id", 1 },
                { "matchCount", 1 },
                {
                    "duplicatedUser", new BsonDocument
                    {
                        { "$map", new BsonDocument
                            {
                                { "input", "$matchedUsers" },
                                { "as", "user" },
                                { "in", new BsonDocument
                                    {
                                        { "_id", "$$user.user_id._id" },
                                        { "fullname", "$$user.fullname" },
                                        { "avatar", "$$user.avatar.public_url" }
                                    }
                                }
                            }
                        }
                    }
                }
            })
            .FirstOrDefaultAsync(cancellationToken);
        return documents;
    }

    public async Task<BsonDocument?> GetGroupMemberInfoAsync(ObjectId groupId, string memberId, CancellationToken cancellationToken = default)
    {
        var document = await DbSet.Aggregate()
            .Match(group => group.Id == groupId)
            .Unwind<Group, BsonDocument>(group => group.Members)
            .Match(Builders<BsonDocument>.Filter.Eq("members.user_id._id", memberId))
            .ReplaceRoot<BsonDocument>("$members")
            .FirstOrDefaultAsync(cancellationToken);
        
        return document;
    }

    public async Task<Dictionary<string, int>> GetExecutorAndTarget(ObjectId groupId, string executorId, string targetId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Or(
            Builders<BsonDocument>.Filter.Eq("members.user_id._id", "executorId"),
            Builders<BsonDocument>.Filter.Eq("members.user_id._id", "targetId")
        );

        var members = await DbSet.Aggregate()
            .Match(group => group.Id == groupId)
            .Unwind<Group, BsonDocument>(group => group.Members)
            .Match(filter)
            .Project(new BsonDocument
            {
                { "_id", 0 },
                { "userId", "$members.user_id._id" },
                { "role", "$members.role" }
            })
            .ToListAsync(cancellationToken);

        return members.ToDictionary(
            m => m["userId"].ToString()!,
            m => m["role"].AsInt32
        );
    }
}