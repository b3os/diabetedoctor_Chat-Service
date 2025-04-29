using ChatService.Application.Infrastructure.Abstractions;
using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.Services.Message.Queries;
using ChatService.Contract.Services.Message.Response;
using ChatService.Persistence;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Queries.Message;

public class GetGroupMessageByGroupIdQueryHandler : IQueryHandler<GetGroupMessageByIdQuery, GetGroupMessageResponse>
{
    private readonly MongoDbContext _mongoDbContext;
    private readonly IClaimsService _claimsService;

    public GetGroupMessageByGroupIdQueryHandler(MongoDbContext mongoDbContext, IClaimsService claimsService)
    {
        _mongoDbContext = mongoDbContext;
        _claimsService = claimsService;
    }

    public async Task<Result<GetGroupMessageResponse>> Handle(GetGroupMessageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _claimsService.GetCurrentUserId;
        var pageSize = request.Filter.PageSize is > 0 ? request.Filter.PageSize.Value : 10;

        if (!ObjectId.TryParse(request.GroupId, out var groupId))
        {
            throw new Exception();
        }

        var pipeline = new List<BsonDocument>
        {
            new()
            {
                { "$match", new BsonDocument() { { "group_id", groupId } } }
            }
        };

        if (!string.IsNullOrWhiteSpace(request.Filter.Search))
        {
            pipeline.Add(new BsonDocument
            {
                { "$match", new BsonDocument { { "content", new BsonDocument("$regex", request.Filter.Search) } } }
            });
        }

        if (!string.IsNullOrEmpty(request.Filter.Cursor) && ObjectId.TryParse(request.Filter.Cursor, out var cursorId))
        {
            pipeline.Add(new BsonDocument
            {
                { "$match", new BsonDocument { { "_id", new BsonDocument { { "$lt", cursorId } } } } }
            });
        }

        pipeline.Add(new BsonDocument
        {
            { "$sort", new BsonDocument { { "_id", -1 } } }
        });

        pipeline.Add(new BsonDocument("$limit", pageSize));

        pipeline.Add(new BsonDocument("$project", new BsonDocument
        {
            { "_id", 1 },
            { "content", 1 },
            { "is_read", new BsonDocument("$in", new BsonArray { userId, "$read_by" }) }
        }));

        var result = await _mongoDbContext.Messages
            .Aggregate<MessageDto>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

        result.Reverse();

        return Result.Success(new GetGroupMessageResponse()
            { Messages = PagedList<MessageDto>.Create(result, result.Count, pageSize, result[0].Id) });
    }
}