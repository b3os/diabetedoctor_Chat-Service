using System.Globalization;
using ChatService.Contract.DTOs.ConversationDtos;
using ChatService.Contract.Services.Conversation.Queries;
using ChatService.Contract.Settings;
using Microsoft.Extensions.Options;

namespace ChatService.Application.UseCase.V1.Queries.Conversation;

public sealed class GetUserConversationByUserIdQueryHandler(
    IMongoDbContext mongoDbContext,
    IOptions<AppDefaultSettings> settings)
    : IQueryHandler<GetUserConversationsByUserIdQuery, GetUserConversationsResponse>
{
    public async Task<Result<GetUserConversationsResponse>> Handle(GetUserConversationsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.Filter.PageSize is > 0 ? request.Filter.PageSize.Value : 10;

        var builder = Builders<Domain.Models.Conversation>.Filter;
        var sorter = Builders<Domain.Models.Conversation>.Sort;

        var filters = new List<FilterDefinition<Domain.Models.Conversation>>
        {
            builder.ElemMatch(c => c.Members, userId => userId.Id == request.UserId),
            builder.Eq(c => c.ConversationType, ConversationType.Group)
        };

        if (!string.IsNullOrEmpty(request.Filter.Cursor)
            && DateTime.TryParseExact(
                request.Filter.Cursor,
                "O",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var cursor))
        {
            filters.Add(builder.Lt(c => c.ModifiedDate, cursor));
        }

        // var totalCount = await mongoDbContext.Conversations.CountDocumentsAsync(builder.And(filters), cancellationToken: cancellationToken);

        var participantLookup = ParticipantLookup();
        var addParticipantInfoStage = AddParticipantInfoWithFallback(settings.Value.UserAvatarDefault);
        var conversationProjection = BuildConversationProjection();

        var result = await mongoDbContext.Conversations
            .Aggregate()
            .Match(builder.And(filters))
            .Sort(sorter
                .Descending(c => c.ModifiedDate)
                .Descending(c => c.Id))
            .Limit(pageSize + 1)
            .Lookup<Participant, Participant, IEnumerable<Participant>, Domain.Models.Conversation>(
                foreignCollection: mongoDbContext.Participants,
                let: new BsonDocument("senderId", "$last_message.sender_id"),
                lookupPipeline: participantLookup,
                @as: "participant_info")
            .AppendStage<BsonDocument>(addParticipantInfoStage)
            .Project(conversationProjection)
            .As<ConversationResponseDto>()
            .ToListAsync(cancellationToken: cancellationToken);

        var hasNext = result.Count > pageSize;

        if (hasNext)
        {
            result.RemoveRange(pageSize, result.Count - pageSize);
        }

        return Result.Success(new GetUserConversationsResponse
        {
            Conversations =
                PagedList<ConversationResponseDto>.Create(result, 0, pageSize, hasNext ? result[^1].ModifiedDate.ToString("O", CultureInfo.InvariantCulture) : string.Empty, hasNext)
        });
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

    private static BsonDocument BuildConversationProjection()
    {
        return new BsonDocument
        {
            { "_id", 1 },
            { "name", 1 },
            { "avatar", "$avatar.public_url" },
            { "type", 1 },
            {
                "last_message", new BsonDocument("$cond", new BsonDocument
                {
                    { "if", new BsonDocument("$eq", new BsonArray { "$last_message", BsonNull.Value }) },
                    { "then", BsonNull.Value },
                    {
                        "else", new BsonDocument
                        {
                            { "_id", "$last_message._id" },
                            { "content", "$last_message.content" },
                            { "type", "$last_message.message_type" },
                            { "file_attachment", "$last_message.file_attachment" },
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
                        }
                    }
                })
            },
            { "modified_date", 1}
        };
    }
}