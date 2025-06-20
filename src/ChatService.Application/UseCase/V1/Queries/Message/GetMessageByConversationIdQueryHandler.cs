using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.Services.Message.Queries;
using ChatService.Contract.Services.Message.Responses;
using ChatService.Contract.Settings;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Queries.Message;

public class GetMessageByConversationIdQueryHandler(
    IMongoDbContext mongoDbContext,
    IOptions<AppDefaultSettings> settings)
    : IQueryHandler<GetMessageByConversationIdQuery, GetMessagesResponse>
{
    public async Task<Result<GetMessagesResponse>> Handle(GetMessageByConversationIdQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.Filter.PageSize is > 0 ? request.Filter.PageSize.Value : 50;

        var checkPermissionResult = await CheckConversationPermissionAsync(request.ConversationId, request.UserId, cancellationToken);
        if (checkPermissionResult.IsFailure)
        {
            return Result.Failure<GetMessagesResponse>(checkPermissionResult.Error);
        }

        var builder = Builders<Domain.Models.Message>.Filter;
        var sorter = Builders<Domain.Models.Message>.Sort;
        
        var filters = new List<FilterDefinition<Domain.Models.Message>>
        {
            builder.Eq(m => m.ConversationId, request.ConversationId)
        };
        
        if (!string.IsNullOrEmpty(request.Filter.Cursor) && ObjectId.TryParse(request.Filter.Cursor, out var cursor))
        {
            filters.Add(builder.Lt(m => m.Id, cursor));
        }
        
        var participantLookup = ParticipantLookup();
        var addParticipantInfoStage = AddParticipantInfoWithFallback(settings.Value.UserAvatarDefault);
        var messageProjection = BuildMessageProjection();
        var result = await mongoDbContext.Messages
            .Aggregate()
            .Match(builder.And(filters))
            .Sort(sorter.Descending(m => m.Id))
            .Limit(pageSize + 1)
            .Lookup<Participant, Participant, IEnumerable<Participant>, Domain.Models.Message>(
                foreignCollection: mongoDbContext.Participants,
                let: new BsonDocument("senderId", "$sender_id"),
                lookupPipeline: participantLookup,
                @as: "participant_info")
            .AppendStage<BsonDocument>(addParticipantInfoStage)
            .Project(messageProjection)
            .As<MessageDto>()
            .ToListAsync(cancellationToken: cancellationToken);
        
        var hasNext = result.Count > pageSize;

        if (hasNext)
        {
            result.RemoveRange(pageSize, result.Count - pageSize);
        }

        result.Reverse();
        
        return Result.Success(new GetMessagesResponse()
        {
            Messages =
                PagedList<MessageDto>.Create(result, 0, pageSize, hasNext ? result[^1].Id : string.Empty, hasNext)
        });
    }

    private async Task<Result> CheckConversationPermissionAsync(ObjectId conversationId,
        string userId,
        CancellationToken cancellationToken)
    {
        var isConversationExisted = await mongoDbContext.Conversations.Find(
            c => c.Id == conversationId
            && c.Members.Any(member => member.Id == userId))
            .AnyAsync(cancellationToken);

        return isConversationExisted
            ? Result.Success()
            : Result.Failure(ConversationErrors.NotFound);
    }
    
    private static PipelineDefinition<Participant, Participant> ParticipantLookup()
    {
        return new EmptyPipelineDefinition<Participant>()
            .Match(new BsonDocument
                { { "$expr", new BsonDocument("$eq", new BsonArray { "$user_id", "$$senderId" }) } })
            .Limit(1);
    }
    
    private static BsonDocument AddParticipantInfoWithFallback(string avatar)
    {
        return new BsonDocument
        {
            {
                "$addFields", new BsonDocument("participant_info",
                    new BsonDocument("$cond", new BsonDocument
                    {
                        {
                            "if",
                            new BsonDocument("$eq", new BsonArray { "$last_message", BsonNull.Value })
                        },
                        { "then", BsonNull.Value },
                        {
                            "else", new BsonDocument("$cond", new BsonDocument
                            {
                                {
                                    "if",
                                    new BsonDocument("$eq",
                                        new BsonArray { new BsonDocument("$size", "$participant_info"), 0 })
                                },
                                {
                                    "then", new BsonDocument
                                    {
                                        { "full_name", "Người dùng không xác định" },
                                        { "avatar", avatar },
                                        { "role", -1 }
                                    }
                                },
                                {
                                    "else",
                                    new BsonDocument("$arrayElemAt", new BsonArray { "$participant_info", 0 })
                                }
                            })
                        }
                    })
                )
            }
        };
    }
    
    private BsonDocument BuildMessageProjection()
    {
        return new BsonDocument
        {
            { "_id", 1 },
            { "content", 1 },
            { "type", 1 },
            {
                "created_date", new BsonDocument("$dateToString", new BsonDocument
                {
                    { "format", "%Y-%m-%dT%H:%M:%S.%L%z" },
                    { "date", "$created_date" },
                    { "timezone", "+07:00" }
                })
            },
            {
                "participant_info", new BsonDocument
                {
                    { "_id", "$participant_info.user_id._id" },
                    { "full_name", "$participant_info.full_name" },
                    { "avatar", "$participant_info.avatar.public_url" },
                    { "role", "$participant_info.role" }
                }
            }
        };
    }
}


// if (!string.IsNullOrWhiteSpace(request.Filter.Search))
// {
//     pipeline.Add(new BsonDocument
//     {
//         { "$match", new BsonDocument { { "content", new BsonDocument("$regex", request.Filter.Search) }, { "$options", "i" } } }
//     });
// }