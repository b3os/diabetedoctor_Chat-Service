using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.DTOs.UserDTOs;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services.Message.Queries;
using ChatService.Contract.Services.Message.Response;
using ChatService.Domain.Models;
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

        var builder = Builders<Domain.Models.Message>.Filter;
        var sorter = Builders<Domain.Models.Message>.Sort;
        var projection = new BsonDocument
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
            {
                "is_read", new BsonDocument("$in", new BsonArray
                {
                    request.UserId,
                    new BsonDocument("$map", new BsonDocument
                    {
                        { "input", "$read_by" },
                        { "as", "read" },
                        { "in", "$$read._id" }
                    })
                })
            },
            {
                "user", new BsonDocument
                {
                    { "_id", "$user.user_id._id" },
                    { "fullname", "$user.fullname" },
                    { "avatar", "$user.avatar.public_url" }
                }
            }
        };

        var filters = new List<FilterDefinition<Domain.Models.Message>>
            { builder.Eq(message => message.GroupId, groupId) };

        if (!string.IsNullOrWhiteSpace(request.Filter.Cursor) &&
            ObjectId.TryParse(request.Filter.Cursor, out var cursorId))
        {
            filters.Add(builder.Lt(message => message.Id, cursorId));
        }
        else
        {
            var cursor = await _mongoDbContext.MessageReadStatuses
                .Find(cursor => cursor.GroupId == groupId && cursor.UserId.Id == request.UserId)
                .Project(cursor => cursor.LastReadMessageId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cursor != ObjectId.Empty)
            {
                filters.Add(builder.Gt(message => message.Id, cursor));

                var unreadMessages = await _mongoDbContext.Messages
                    .Aggregate()
                    .Match(builder.And(filters))
                    .Sort(sorter.Ascending(message => message.Id))
                    .Lookup<Domain.Models.Message, MessageDto>(
                        foreignCollectionName: "User",
                        localField: "sender_id._id",
                        foreignField: "user_id._id",
                        @as: "user")
                    .Unwind("user", new AggregateUnwindOptions<MessageDto>
                    {
                        PreserveNullAndEmptyArrays = true
                    })
                    .Project(projection)
                    .As<MessageDto>()
                    .ToListAsync(cancellationToken);

                if (unreadMessages.Count > pageSize)
                {
                    return Result.Success(new GetGroupMessageResponse()
                    {
                        Messages = PagedList<MessageDto>.Create(unreadMessages, unreadMessages.Count, pageSize,
                            unreadMessages[0].Id, true)
                    });
                }

                var readFilter = builder.And(
                    builder.Eq(message => message.GroupId, groupId),
                    builder.Lte(message => message.Id, cursor)
                );

                var readMessages = await _mongoDbContext.Messages
                    .Aggregate()
                    .Match(readFilter)
                    .Sort(sorter.Descending(message => message.Id))
                    .Limit(pageSize - unreadMessages.Count + 1)
                    .Lookup<Domain.Models.Message, MessageDto>(
                        foreignCollectionName: "User",
                        localField: "sender_id._id",
                        foreignField: "user_id._id",
                        @as: "user")
                    .Unwind("user", new AggregateUnwindOptions<MessageDto>
                    {
                        PreserveNullAndEmptyArrays = true
                    })
                    .Project(projection)
                    .As<MessageDto>()
                    .ToListAsync(cancellationToken: cancellationToken);

                var unreadHasNext = readMessages.Count > pageSize - unreadMessages.Count;

                if (unreadHasNext)
                {
                    readMessages.RemoveRange(pageSize, readMessages.Count - (pageSize - unreadMessages.Count));
                }

                readMessages.Reverse();

                var unreadResult = readMessages.Concat(unreadMessages).ToList();
                return Result.Success(new GetGroupMessageResponse()
                {
                    Messages = PagedList<MessageDto>.Create(unreadResult, unreadResult.Count, pageSize,
                        unreadResult[0].Id, unreadHasNext)
                });
            }
        }

        var readResult = await _mongoDbContext.Messages
            .Aggregate()
            .Match(builder.And(filters))
            .Sort(sorter.Descending(message => message.Id))
            .Limit(pageSize + 1)
            .Lookup<Domain.Models.Message, MessageDto>(
                foreignCollectionName: nameof(User),
                localField: "sender_id._id",
                foreignField: "user_id._id",
                @as: "user")
            .Unwind("user", new AggregateUnwindOptions<MessageDto>
            {
                PreserveNullAndEmptyArrays = true
            })
            .Project(projection)
            .As<MessageDto>()
            .ToListAsync(cancellationToken: cancellationToken);


        if (readResult.Count == 0)
        {
            return Result.Success(new GetGroupMessageResponse()
                { Messages = PagedList<MessageDto>.Create([], 0, pageSize, string.Empty, false) });
        }

        var readHasNext = readResult.Count > pageSize;

        if (readHasNext)
        {
            readResult.RemoveRange(pageSize, readResult.Count - pageSize);
        }

        readResult.Reverse();

        return Result.Success(new GetGroupMessageResponse()
        {
            Messages = PagedList<MessageDto>.Create(readResult, readResult.Count, pageSize, readResult[0].Id, readHasNext)
        });
    }
}

// if (!string.IsNullOrWhiteSpace(request.Filter.Search))
// {
//     pipeline.Add(new BsonDocument
//     {
//         { "$match", new BsonDocument { { "content", new BsonDocument("$regex", request.Filter.Search) }, { "$options", "i" } } }
//     });
// }