using System.Globalization;
using ChatService.Contract.DTOs.ConversationDtos;
using ChatService.Contract.DTOs.ConversationDtos.Responses;
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
        var pageSize = request.CursorFilter.PageSize is > 0 ? request.CursorFilter.PageSize.Value : 10;

        var builder = Builders<Domain.Models.Conversation>.Filter;
        var sorter = Builders<Domain.Models.Conversation>.Sort;

        var filters = new List<FilterDefinition<Domain.Models.Conversation>>
        {
            builder.ElemMatch(c => c.Members, userId => userId.Id == request.UserId),
            builder.Eq(c => c.ConversationType, ConversationType.Group)
        };

        if (!string.IsNullOrWhiteSpace(request.CursorFilter.Cursor)
            && DateTime.TryParseExact(
                request.CursorFilter.Cursor,
                "O",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var cursor))
        {
            filters.Add(builder.Lt(c => c.ModifiedDate, cursor));
        }

        // var totalCount = await mongoDbContext.Conversations.CountDocumentsAsync(builder.And(filters), cancellationToken: cancellationToken);

        var participantLookup = ParticipantLookup();
        var addParticipantInfoStage = AddParticipantInfo();
        var conversationProjection = BuildConversationProjection(settings.Value.UserAvatarDefault);

        var result = await mongoDbContext.Conversations
            .Aggregate()
            .Match(builder.And(filters))
            .Sort(sorter.Combine(
                sorter.Descending(c => c.ModifiedDate),
                sorter.Descending(c => c.Id)))
            .Limit(pageSize + 1)
            .Lookup<Participant, BsonDocument, IEnumerable<BsonDocument>, BsonDocument>(
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
                PagedList<ConversationResponseDto>.Create(result, 0, pageSize,
                    hasNext ? result[^1].ModifiedDate.ToString("O", CultureInfo.InvariantCulture) : string.Empty,
                    hasNext)
        });
    }

    private static PipelineDefinition<Participant, BsonDocument> ParticipantLookup()
    {
        return PipelineDefinition<Participant, BsonDocument>.Create(
            new BsonDocument("$match", new BsonDocument
            {
                { "$expr", new BsonDocument("$eq", new BsonArray { "$user_id", "$$senderId" }) }
            }),
            new BsonDocument("$limit", 1),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", nameof(User) },
                { "let", new BsonDocument("userId", "$user_id") },
                {
                    "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "$expr", new BsonDocument("$eq", new BsonArray { "$user_id", "$$userId" }) }
                        }),
                        new BsonDocument("$project", new BsonDocument
                        {
                            { "full_name", 1 },
                            { "avatar", 1 }
                        })
                    }
                },
                { "as", "user_info" }
            }),
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$user_info" },
                { "preserveNullAndEmptyArrays", true }
            }));
    }

    private static BsonDocument AddParticipantInfo()
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
                            "else", 
                            new BsonDocument("$arrayElemAt", new BsonArray { "$participant_info", 0 })
                        }
                    })
                )
            }
        };
    }

    private static BsonDocument BuildConversationProjection(string avatarDefault)
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
                        { "created_date", "$last_message.created_date" },
                        {
                            "participant_info", new BsonDocument
                            {
                                { "_id", "$participant_info.user_id._id" },
                                {
                                    "full_name", new BsonDocument("$ifNull", new BsonArray
                                    {
                                        "$participant_info.user_info.full_name",
                                        "Người dùng không xác định"
                                    })
                                },
                                {
                                    "avatar", new BsonDocument("$ifNull", new BsonArray
                                    {
                                        "$participant_info.user_info.avatar.public_url",
                                        avatarDefault
                                    })
                                },
                                {
                                    "role", new BsonDocument("$ifNull", new BsonArray
                                    {
                                        "$participant_info.role",
                                        -1
                                    })
                                }
                            }
                        }
                    }
                }
            })
        },
            { "modified_date", 1 }
        };
    }
}