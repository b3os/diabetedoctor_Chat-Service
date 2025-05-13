using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.Services.Message.Queries;
using ChatService.Contract.Services.Message.Responses;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Models;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Queries.Message;

public class GetGroupMessageByGroupIdQueryHandler(IMongoDbContext mongoDbContext)
    : IQueryHandler<GetGroupMessageByIdQuery, GetGroupMessageResponse>
{
    public async Task<Result<GetGroupMessageResponse>> Handle(GetGroupMessageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.Filter.PageSize is > 0 ? request.Filter.PageSize.Value : 10;

        if (!ObjectId.TryParse(request.GroupId, out var groupId))
        {
            throw new Exception();
        }

        var groupExist = await mongoDbContext.Groups
            .Find(x => x.Id == groupId && x.Members.Any(member => member.UserId.Id == request.UserId))
            .AnyAsync(cancellationToken);

        if (!groupExist)
        {
            throw new GroupExceptions.GroupAccessDeniedException();
        }

        var cursorId = ObjectId.TryParse(request.Filter.Cursor, out var parsedId) ? parsedId : ObjectId.Empty;

        var filters = BuildFilters(groupId, cursorId);

        if (cursorId == ObjectId.Empty)
        {
            var lastReadMessageId = await GetLastReadMessageId(request.UserId, groupId, cancellationToken);
            if (lastReadMessageId != ObjectId.Empty)
            {
                return await GetUnreadAndReadMessages(request.UserId, groupId, lastReadMessageId, pageSize, cancellationToken);
            }

            var allMessages = await ExecuteMessageAggregate(request.UserId, filters, true, pageSize, cancellationToken);
            return Result.Success(new GetGroupMessageResponse
            {
                Messages = PagedList<MessageDto>.Create(allMessages, allMessages.Count, pageSize, string.Empty, false)
            });
        }

        var readResult = await ExecuteMessageAggregate(request.UserId, filters, false, pageSize + 1, cancellationToken);
        
        var readHasNext = readResult.Count > pageSize;

        if (readHasNext)
        {
            readResult.RemoveRange(pageSize, readResult.Count - pageSize);
        }

        readResult.Reverse();

        return Result.Success(new GetGroupMessageResponse
        {
            Messages = PagedList<MessageDto>.Create(readResult, readResult.Count, pageSize, readResult[0].Id, readHasNext)
        });
    }
    
    private FilterDefinition<Domain.Models.Message> BuildFilters(ObjectId groupId, ObjectId cursorId)
    {
        var builder = Builders<Domain.Models.Message>.Filter;
        var filters = new List<FilterDefinition<Domain.Models.Message>> { builder.Eq(m => m.GroupId, groupId) };

        if (cursorId != ObjectId.Empty)
            filters.Add(builder.Lt(m => m.Id, cursorId));

        return builder.And(filters);
    }
    
    private BsonDocument AddUserRoleStage()
    {
        return new BsonDocument("$addFields", new BsonDocument
        {
            {
                "user", new BsonDocument("$mergeObjects", new BsonArray
                {
                    "$user",
                    new BsonDocument("$let", new BsonDocument
                    {
                        {
                            "vars", new BsonDocument("matchedMember", new BsonDocument("$first", new BsonDocument
                            {
                                {
                                    "$filter", new BsonDocument
                                    {
                                        { "input", "$group.members" },
                                        { "as", "member" },
                                        { "cond", new BsonDocument("$eq", new BsonArray { "$$member.user_id._id", "$sender_id._id" }) }
                                    }
                                }
                            }))
                        },
                        {
                            "in", new BsonDocument
                            {
                                { "role", "$$matchedMember.role" }
                            }
                        }
                    })
                })
            }
        });
    }
    
    private BsonDocument BuildProjection(string userId)
    {
        return new BsonDocument
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
                    userId,
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
                    { "avatar", "$user.avatar.public_url" },
                    { "role", "$user.role" }
                }
            }
        };
    }
    
    private async Task<ObjectId> GetLastReadMessageId(string userId, ObjectId groupId, CancellationToken cancellationToken)
    {
        return await mongoDbContext.MessageReadStatuses
            .Find(s => s.GroupId == groupId && s.UserId.Id == userId)
            .Project(s => s.LastReadMessageId)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    private async Task<List<MessageDto>> ExecuteMessageAggregate(string userId,
        FilterDefinition<Domain.Models.Message> filter, bool isAscending, int? limit, CancellationToken cancellationToken)
    {
        var sorter = isAscending
            ? Builders<Domain.Models.Message>.Sort.Ascending(m => m.Id)
            : Builders<Domain.Models.Message>.Sort.Descending(m => m.Id);

        var pipeline = mongoDbContext.Messages
            .Aggregate()
            .Match(filter)
            .Sort(sorter);

        if (limit.HasValue)
        {
            pipeline = pipeline.Limit(limit.Value);
        }
        
        var documents = await pipeline
            .Lookup<Domain.Models.Message, MessageDto>(
                foreignCollectionName: nameof(User),
                localField: "sender_id._id",
                foreignField: "user_id._id",
                @as: "user")
            .Unwind("user", new AggregateUnwindOptions<MessageDto>
            {
                PreserveNullAndEmptyArrays = true
            })
            .Lookup(
                foreignCollectionName: "Group",
                localField: "group_id",
                foreignField: "_id",
                @as: "group")
            .Unwind("group", new AggregateUnwindOptions<MessageDto>
            {
                PreserveNullAndEmptyArrays = true
            })
            .AppendStage<BsonDocument>(AddUserRoleStage())
            .Project(BuildProjection(userId))
            .As<MessageDto>()
            .ToListAsync(cancellationToken: cancellationToken);

        return documents;
    }
    
    private async Task<Result<GetGroupMessageResponse>> GetUnreadAndReadMessages(string userId, ObjectId groupId, ObjectId cursor,
        int pageSize, CancellationToken cancellationToken)
    {
        var builder = Builders<Domain.Models.Message>.Filter;

        var unreadFilter = builder.And(
            builder.Eq(m => m.GroupId, groupId),
            builder.Gt(m => m.Id, cursor)
        );

        var unreadMessages = await ExecuteMessageAggregate(userId, unreadFilter, true, pageSize, cancellationToken);

        if (unreadMessages.Count >= pageSize)
        {
            return Result.Success(new GetGroupMessageResponse
            {
                Messages = PagedList<MessageDto>.Create(unreadMessages, unreadMessages.Count, pageSize,
                    unreadMessages[0].Id, true)
            });
        }

        var readFilter = builder.And(
            builder.Eq(m => m.GroupId, groupId),
            builder.Lte(m => m.Id, cursor)
        );

        var readMessages = await ExecuteMessageAggregate(userId, readFilter, false,
            pageSize - unreadMessages.Count + 1, cancellationToken);

        var hasNext = readMessages.Count > (pageSize - unreadMessages.Count);
        if (hasNext)
        {
            readMessages.RemoveRange(pageSize - unreadMessages.Count, readMessages.Count - (pageSize - unreadMessages.Count));
        }

        readMessages.Reverse();

        var allMessages = readMessages.Concat(unreadMessages).ToList();

        return Result.Success(new GetGroupMessageResponse
        {
            Messages = PagedList<MessageDto>.Create(allMessages, allMessages.Count, pageSize,
                allMessages[0].Id, hasNext)
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