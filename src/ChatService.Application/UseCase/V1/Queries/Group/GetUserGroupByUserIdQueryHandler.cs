using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.DTOs.UserDTOs;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services.Group.Queries;
using ChatService.Contract.Services.Group.Responses;
using ChatService.Domain.Models;
using ChatService.Domain.ValueObjects;
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

        var builder = Builders<Domain.Models.Group>.Filter;
        var sorter = Builders<Domain.Models.Group>.Sort;
        var groupProjection = new BsonDocument
        {
            { "_id", 1 },
            { "name", 1 },
            { "avatar", "$avatar.public_url" },
            { "message", 1 },
        };
        var filters = new List<FilterDefinition<Domain.Models.Group>>()
        {
            builder.ElemMatch(group => group.Members, id => id.Id.Equals(request.UserId))
        };

        // tạm thời đang dùng chung query với search sau này sẽ đổi thành elastic với plugin của duydo
        if (!string.IsNullOrWhiteSpace(request.Filter.Search))
        {
            filters.Add(builder.Regex(group => group.Name,
                new BsonRegularExpression(NormalizeToRegex.NormalizeInput(request.Filter.Search), "ix")));
        }

        var total = await mongoDbContext.Groups.CountDocumentsAsync(builder.And(filters),
            cancellationToken: cancellationToken);

        if (total == 0)
        {
            return Result.Success(new GetUserGroupResponse
                { Groups = PagedList<GroupDto>.Create([], total, pageSize, string.Empty, false) });
        }

        if (!string.IsNullOrEmpty(request.Filter.Cursor) && ObjectId.TryParse(request.Filter.Cursor, out var cursorId))
        {
            filters.Add(builder.Gt(group => group.Id, cursorId));
        }
        
        var result = await mongoDbContext.Groups
            .Aggregate()
            .Match(builder.And(filters))
            .Sort(sorter.Descending(group => group.Id))
            .Limit(pageSize + 1)
            .Lookup<Domain.Models.Message, MessageDto, IEnumerable<MessageDto>, GroupDto>(
                foreignCollection: mongoDbContext.Messages,
                let: new BsonDocument("groupId", "$_id"),
                lookupPipeline: MessageLookup(request.UserId),
                @as: "message" )
            .Unwind(groupDto => groupDto.Message, new AggregateUnwindOptions<BsonDocument>
            {
                PreserveNullAndEmptyArrays = true
            })
            .Project(groupProjection)
            .As<GroupDto>()
            .ToListAsync(cancellationToken: cancellationToken);
        
        var hasNext = result.Count > pageSize;

        if (hasNext)
        {
            result.RemoveRange(pageSize, result.Count - pageSize);
        }

        return Result.Success(new GetUserGroupResponse
            { Groups = PagedList<GroupDto>.Create(result, total, pageSize, result[^1].Id, hasNext) });
    }

    private PipelineDefinition<Domain.Models.Message, MessageDto> MessageLookup(string userId)
    {
        var messageProjection = new BsonDocument
        {
            { "_id", 1 },
            { "content", 1 },
            { "type", "$message_type.value" },
            { "is_read", 1 },
            {
                "created_date", new BsonDocument("$dateToString", new BsonDocument
                {
                    { "format", "%Y-%m-%dT%H:%M:%S.%L%z" },
                    { "date", "$created_date" },
                    { "timezone", "+07:00" }
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
        
        return new EmptyPipelineDefinition<Domain.Models.Message>()
            .Match(new BsonDocument
                { { "$expr", new BsonDocument("$eq", new BsonArray { "$group_id", "$$groupId" }) } })
            .Sort(Builders<Domain.Models.Message>.Sort.Descending(message => message.Id))
            .Limit(1)
            .AppendStage(
                new BsonDocumentPipelineStageDefinition<Domain.Models.Message, Domain.Models.Message>(
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
                    })
                )
            )
            .Lookup<Domain.Models.Message, Domain.Models.Message, User, UserDto>(
                foreignCollection: mongoDbContext.Users,
                localField: message => message.SenderId.Id,
                foreignField: user => user.UserId.Id,
                @as: user => user
            )
            .Unwind(userDto => userDto, new AggregateUnwindOptions<BsonDocument>
            {
                PreserveNullAndEmptyArrays = true
            })
            .Project(messageProjection)
            .As<Domain.Models.Message, BsonDocument, MessageDto>();
    }

    // private BsonDocument GetNewestMessageOfEachGroup(string userId)
    // {
    //     return new BsonDocument
    //     {
    //         {
    //             "$lookup", new BsonDocument
    //             {
    //                 { "from", "Message" },
    //                 { "let", new BsonDocument { { "groupId", "$_id" } } },
    //                 {
    //                     "pipeline", new BsonArray
    //                     {
    //                         new BsonDocument("$match", new BsonDocument
    //                             {
    //                                 { "$expr", new BsonDocument("$eq", new BsonArray { "$group_id", "$$groupId" }) }
    //                             }
    //                         ),
    //                         new BsonDocument("$sort", new BsonDocument
    //                             {
    //                                 { "created_date", -1 }
    //                             }
    //                         ),
    //                         new BsonDocument("$limit", 1),
    //                         new BsonDocument("$addFields", new BsonDocument
    //                         {
    //                             {
    //                                 "is_read", new BsonDocument
    //                                 {
    //                                     {
    //                                         "$cond", new BsonArray
    //                                         {
    //                                             new BsonDocument("$in", new BsonArray { userId, "$read_by" }),
    //                                             true,
    //                                             false
    //                                         }
    //                                     }
    //                                 }
    //                             }
    //                         }),
    //                         new BsonDocument("$lookup", new BsonDocument
    //                         {
    //                             { "from", "User" },
    //                             { "localField", "sender_id._id" },
    //                             { "foreignField", "user_id._id" },
    //                             { "as", "user" }
    //                         }),
    //                         new BsonDocument("$unwind", new BsonDocument
    //                         {
    //                             { "path", "$user" },
    //                             { "preserveNullAndEmptyArrays", true }
    //                         }),
    //                         new BsonDocument("$project", new BsonDocument
    //                         {
    //                             { "_id", 1 },
    //                             { "content", 1 },
    //                             { "type", "$message_type.value" },
    //                             { "is_read", 1 },
    //                             {
    //                                 "created_date", new BsonDocument("$dateToString", new BsonDocument
    //                                 {
    //                                     { "format", "%Y-%m-%dT%H:%M:%S.%L%z" },
    //                                     { "date", "$created_date" },
    //                                     { "timezone", "+07:00" }
    //                                 })
    //                             },
    //                             {
    //                                 "user", new BsonDocument
    //                                 {
    //                                     { "_id", "$user.user_id._id" },
    //                                     { "fullname", "$user.fullname" },
    //                                     { "avatar", "$user.avatar.public_url" }
    //                                 }
    //                             }
    //                         })
    //                     }
    //                 },
    //                 { "as", "message" }
    //             }
    //         }
    //     };
    // }
}