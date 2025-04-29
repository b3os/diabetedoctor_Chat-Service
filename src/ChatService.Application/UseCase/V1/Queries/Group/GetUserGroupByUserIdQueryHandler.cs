using ChatService.Application.Infrastructure.Abstractions;
using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.Services.Group.Queries;
using ChatService.Contract.Services.Group.Responses;
using ChatService.Persistence;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Queries.Group;

public class
    GetUserGroupByUserIdQueryHandler : IQueryHandler<GetUserGroupByUserIdQuery, GetUserGroupResponse>
{
    private readonly MongoDbContext _mongoDbContext;
    private readonly IClaimsService _claimsService;

    public GetUserGroupByUserIdQueryHandler(MongoDbContext mongoDbContext, IClaimsService claimsService)
    {
        _mongoDbContext = mongoDbContext;
        _claimsService = claimsService;
    }

    public async Task<Result<GetUserGroupResponse>> Handle(GetUserGroupByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _claimsService.GetCurrentUserId;
        var pageSize = request.Filter.PageSize is > 0 ? request.Filter.PageSize.Value : 10;
        
        var pipeline = new List<BsonDocument>
        {
            new()
            {
                { "$match", new BsonDocument { { "members", new BsonDocument { { "$in", new BsonArray { userId } } } } } }
            }
        };

        if (!string.IsNullOrWhiteSpace(request.Filter.Search))
        {
            pipeline.Add(new BsonDocument
            {
                { "$match", new BsonDocument { { "name", request.Filter.Search } } }
            });
        }

        if (!string.IsNullOrEmpty(request.Filter.Cursor) && ObjectId.TryParse(request.Filter.Cursor, out var cursorId))
        {
            pipeline.Add(new BsonDocument
            {
                { "$match", new BsonDocument { { "_id", new BsonDocument { { "$gt", cursorId } } } } }
            });
        }

        pipeline.Add(GetNewestMessageOfEachGroup(userId));
        
        pipeline.Add(new BsonDocument
        {
            { "$sort", new BsonDocument { { "message._id", -1 } } }
        });

        // make array[1] to 1 object
        pipeline.Add(new BsonDocument("$unwind", new BsonDocument
        {
            { "path", "$message" },
            { "preserveNullAndEmptyArrays", true }
        }));

        pipeline.Add(new BsonDocument("$limit", pageSize));
        
        var result = await _mongoDbContext.Groups.Aggregate<GroupDto>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        return Result.Success(new GetUserGroupResponse{Groups = PagedList<GroupDto>.Create(result, result.Count, pageSize, result[^1].Id)});
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
                                    { "createdAt", -1 }
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
                            new BsonDocument("$project", new BsonDocument
                            {
                                { "_id", 1 },
                                { "content", 1 },
                                { "is_read", 1 }
                            })
                        }
                    },
                    { "as", "message" }
                }
            }
        };

    }
}