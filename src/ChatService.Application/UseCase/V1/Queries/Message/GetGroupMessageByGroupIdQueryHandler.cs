using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services.Message.Queries;
using ChatService.Contract.Services.Message.Response;
using ChatService.Persistence;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Queries.Message;

public class GetGroupMessageByGroupIdQueryHandler : IQueryHandler<GetGroupMessageByIdQuery, GetGroupMessageResponse>
{
    private readonly MongoDbContext _mongoDbContext;

    public GetGroupMessageByGroupIdQueryHandler(MongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }

    public async Task<Result<GetGroupMessageResponse>> Handle(GetGroupMessageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.Filter.PageSize is > 0 ? request.Filter.PageSize.Value : 10;
        var total = 0;

        // code tạm thay thế fluent validator
        if (!ObjectId.TryParse(request.GroupId, out var groupId))
        {
            throw new Exception();
        }

        var groupExist = await _mongoDbContext.Groups
            .Find(x => x.Id == groupId && x.Members.Any(id => id.Id.Equals(request.UserId)))
            .AnyAsync(cancellationToken);

        if (!groupExist)
        {
            throw new GroupExceptions.GroupAccessDeniedException();
        }

        var pipeline = new List<BsonDocument>
        {
            new()
            {
                { "$match", new BsonDocument() { { "group_id", groupId } } }
            }
        };

        if (!string.IsNullOrWhiteSpace(request.Filter.Cursor) &&
            ObjectId.TryParse(request.Filter.Cursor, out var cursorId))
        {
            pipeline.Add(new BsonDocument
            {
                { "$match", new BsonDocument { { "_id", new BsonDocument { { "$lt", cursorId } } } } }
            });
        }
        else
        {
            var projection =
                Builders<Domain.Models.MessageReadStatus>.Projection.Include(status => status.LastReadMessageId);
            var cursor = await _mongoDbContext.MessageReadStatuses
                .Find(x => x.GroupId == groupId && x.UserId.Id == request.UserId).Project(projection)
                .FirstOrDefaultAsync(cancellationToken);

            if (cursor != null)
            {
                pipeline.Add(new BsonDocument
                {
                    { "$match", new BsonDocument { { "_id", new BsonDocument { { "$gt", cursor } } } } }
                });
            }

            var countPipeline = pipeline.Append(new BsonDocument("$count", "total")).ToList();

            total = (await _mongoDbContext.Messages
                .Aggregate<BsonDocument>(countPipeline, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken))?["total"].AsInt32 ?? 0;
        }

        pipeline.Add(new BsonDocument
        {
            { "$sort", new BsonDocument { { "_id", -1 } } }
        });

        if (total < pageSize)
        {
            pipeline.Add(new BsonDocument("$limit", pageSize + 1));
        }

        pipeline.Add(new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "User" },
            { "localField", "sender_id._id" },
            { "foreignField", "user_id._id" },
            { "as", "user" }
        }));

        pipeline.Add(new BsonDocument("$unwind", new BsonDocument
        {
            { "path", "$user" },
            { "preserveNullAndEmptyArrays", true }
        }));

        pipeline.Add(new BsonDocument("$project", new BsonDocument
        {
            { "_id", 1 },
            { "content", 1 },
            { "type", "$message_type.value" },
            {
                "created_date", new BsonDocument("$dateToString", new BsonDocument
                {
                    { "format", "%Y-%m-%dT%H:%M:%S.%L%z" },
                    { "date", "$created_date" },
                    { "timezone", "+07:00" }
                })
            },
            { "is_read", new BsonDocument("$in", new BsonArray { request.UserId, "$read_by" }) },
            {
                "user", new BsonDocument
                {
                    { "_id", "$user.user_id._id" },
                    { "fullname", "$user.fullname" },
                    { "avatar", "$user.avatar.public_url" }
                }
            }
        }));

        var result = await _mongoDbContext.Messages
            .Aggregate<MessageDto>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

        if (result.Count == 0)
        {
            return Result.Success(new GetGroupMessageResponse()
                { Messages = PagedList<MessageDto>.Create([], 0, pageSize, string.Empty, false) });
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            result.Reverse();

            return Result.Success(new GetGroupMessageResponse()
                { Messages = PagedList<MessageDto>.Create(result, result.Count, pageSize, result[0].Id, true) });
        }

        var hasNext = result.Count > pageSize;

        if (hasNext)
        {
            result.RemoveRange(pageSize, result.Count - pageSize);
        }

        result.Reverse();

        return Result.Success(new GetGroupMessageResponse()
            { Messages = PagedList<MessageDto>.Create(result, result.Count, pageSize, result[0].Id, hasNext) });
    }
}

// if (!string.IsNullOrWhiteSpace(request.Filter.Search))
// {
//     pipeline.Add(new BsonDocument
//     {
//         { "$match", new BsonDocument { { "content", new BsonDocument("$regex", request.Filter.Search) }, { "$options", "i" } } }
//     });
// }