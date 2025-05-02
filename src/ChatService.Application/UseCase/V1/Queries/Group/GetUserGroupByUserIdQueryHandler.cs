using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services.Group.Queries;
using ChatService.Contract.Services.Group.Responses;
using ChatService.Persistence;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Queries.Group;

public class
    GetUserGroupByUserIdQueryHandler(MongoDbContext mongoDbContext)
    : IQueryHandler<GetUserGroupByUserIdQuery, GetUserGroupResponse>
{
    public async Task<Result<GetUserGroupResponse>> Handle(GetUserGroupByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.Filter.PageSize is > 0 ? request.Filter.PageSize.Value : 10;
        
        var pipeline = new List<BsonDocument>
        {
            new()
            {
                { "$match", new BsonDocument { { "members", new BsonDocument { { "$elemMatch", new BsonDocument { {"_id", request.UserId} } } } } } }
            }
        };

        // tạm thời đang dùng chung query với search sau này sẽ đổi thành elastic với plugin của duydo
        if (!string.IsNullOrWhiteSpace(request.Filter.Search))
        {
            pipeline.Add(new BsonDocument
            {
                { "$match", new BsonDocument { { "name", new BsonDocument("$regex",request.Filter.Search) }, { "$options", "i" } } }
            });
        }

        var countPipeline = pipeline.Append(new BsonDocument("$count", "total")).ToList();

        var total = (await mongoDbContext.Groups.Aggregate<BsonDocument>(countPipeline, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken))?["total"].AsInt32 ?? 0;

        if (total <= 0)
        {
            return Result.Success(new GetUserGroupResponse{Groups = PagedList<GroupDto>.Create([], total, pageSize, string.Empty, false)});
        }
        
        if (!string.IsNullOrEmpty(request.Filter.Cursor) && ObjectId.TryParse(request.Filter.Cursor, out var cursorId))
        {
            pipeline.Add(new BsonDocument
            {
                { "$match", new BsonDocument { { "_id", new BsonDocument { { "$gt", cursorId } } } } }
            });
        }
        
        pipeline.Add(new BsonDocument
        {
            { "$sort", new BsonDocument { { "message._id", -1 } } }
        });

        pipeline.Add(new BsonDocument("$limit", pageSize + 1));
        
        pipeline.Add(GetNewestMessageOfEachGroup(request.UserId));

        // make array[1] to 1 object
        pipeline.Add(new BsonDocument("$unwind", new BsonDocument
        {
            { "path", "$message" },
            { "preserveNullAndEmptyArrays", true }
        }));

        pipeline.Add(new BsonDocument("$project", new BsonDocument
        {
            { "_id", 1 },
            { "name", 1 },
            { "avatar", "$avatar.public_url" },
            { "message", 1 }, 
        }));
        
        var result = await mongoDbContext.Groups.Aggregate<GroupDto>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        var hasNext = result.Count > pageSize;

        if (hasNext)
        {
            result.RemoveRange(pageSize, result.Count - pageSize);
        }
        
        return Result.Success(new GetUserGroupResponse{Groups = PagedList<GroupDto>.Create(result, total, pageSize, result[^1].Id, hasNext)});
    }
    
    private BsonDocument GetNewestMessageOfEachGroup(string userId)
    {
        return new BsonDocument
        {
            {
                "$lookup", new BsonDocument
                {
                    { "from", "Message" },
                    { "let", new BsonDocument { { "groupId", "$_id" } } },
                    {
                        "pipeline", new BsonArray
                        {
                            new BsonDocument("$match", new BsonDocument
                                {
                                    { "$expr", new BsonDocument("$eq", new BsonArray { "$group_id", "$$groupId" }) }
                                }
                            ),
                            new BsonDocument("$sort", new BsonDocument
                                {
                                    { "created_date", -1 }
                                }
                            ),
                            new BsonDocument("$limit", 1),
                            new BsonDocument("$addFields", new BsonDocument
                            {
                                {
                                    "is_read", new BsonDocument
                                    {
                                        {
                                            "$cond", new BsonArray
                                            {
                                                new BsonDocument("$in", new BsonArray { userId, "$read_by" }),
                                                true,
                                                false
                                            }
                                        }
                                    }
                                }
                            }),
                            new BsonDocument("$lookup", new BsonDocument
                            {
                                { "from", "User" },
                                { "localField", "sender_id._id" },
                                { "foreignField", "user_id._id" },
                                { "as", "user" }
                            }),
                            new BsonDocument("$unwind", new BsonDocument
                            {
                                { "path", "$user" },
                                { "preserveNullAndEmptyArrays", true }
                            }),
                            new BsonDocument("$project", new BsonDocument
                            {
                                { "_id", 1 },
                                { "content", 1 },
                                { "type", "$message_type.value"},
                                { "is_read", 1 },
                                { "created_date", new BsonDocument("$dateToString", new BsonDocument
                                    {
                                        { "format", "%Y-%m-%dT%H:%M:%S.%L%z" },
                                        { "date", "$created_date" },
                                        { "timezone", "+07:00" }
                                    })
                                },
                                { "user", new BsonDocument
                                    {
                                        { "_id", "$user.user_id._id" },
                                        { "fullname", "$user.fullname" },
                                        { "avatar", "$user.avatar.public_url" }
                                    }
                                }
                            })
                        }
                    },
                    { "as", "message" }
                }
            }
        };

    }
}