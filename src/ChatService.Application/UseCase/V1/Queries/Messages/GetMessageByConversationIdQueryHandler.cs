using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.Services.Message.Queries;
using ChatService.Contract.Services.Message.Responses;

namespace ChatService.Application.UseCase.V1.Queries.Messages;

public sealed class GetMessageByConversationIdQueryHandler(
    IMongoDbContext mongoDbContext,
    IOptions<AppDefaultSettings> settings)
    : IQueryHandler<GetMessageByConversationIdQuery, GetMessagesResponse>
{
    public async Task<Result<GetMessagesResponse>> Handle(GetMessageByConversationIdQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.CursorFilter.PageSize is > 0 ? request.CursorFilter.PageSize.Value : 50;

        var checkPermissionResult =
            await CheckConversationPermissionAsync(request.ConversationId, request.UserId, cancellationToken);
        if (checkPermissionResult.IsFailure)
        {
            return Result.Failure<GetMessagesResponse>(checkPermissionResult.Error);
        }

        var builder = Builders<Message>.Filter;
        var sorter = Builders<Message>.Sort;

        var filters = new List<FilterDefinition<Message>>
        {
            builder.Eq(m => m.ConversationId, request.ConversationId)
        };

        if (!string.IsNullOrEmpty(request.CursorFilter.Cursor) && ObjectId.TryParse(request.CursorFilter.Cursor, out var cursor))
        {
            filters.Add(builder.Lt(m => m.Id, cursor));
        }

        var participantLookup = ParticipantLookup();
        var addParticipantInfoStage = AddParticipantInfo();
        var messageProjection = BuildMessageProjection(settings.Value.UserAvatarDefault);

        var result = await mongoDbContext.Messages
            .Aggregate()
            .Match(builder.And(filters))
            .Sort(sorter.Descending(m => m.Id))
            .Limit(pageSize + 1)
            .Lookup<Participant, BsonDocument, IEnumerable<BsonDocument>, BsonDocument>(
                foreignCollection: mongoDbContext.Participants,
                let: new BsonDocument("senderId", "$sender_id"),
                lookupPipeline: participantLookup,
                @as: "participant_info")
            .AppendStage<BsonDocument>(addParticipantInfoStage)
            .Project(messageProjection)
            .As<MessageResponseDto>()
            .ToListAsync(cancellationToken: cancellationToken);

        var hasNext = result.Count > pageSize;

        if (hasNext)
        {
            result.RemoveRange(pageSize, result.Count - pageSize);
        }

        result.Reverse();

        return Result.Success(new GetMessagesResponse
        {
            Messages =
                PagedList<MessageResponseDto>.Create(result, 0, pageSize, hasNext ? result[^1].Id : string.Empty,
                    hasNext)
        });
    }

    private async Task<Result> CheckConversationPermissionAsync(ObjectId conversationId,
        string userId,
        CancellationToken cancellationToken)
    {
        var isConversationExisted = await mongoDbContext.Conversations.Find(c => c.Id == conversationId
                && c.Members.Any(member => member.Id == userId))
            .AnyAsync(cancellationToken);

        return isConversationExisted
            ? Result.Success()
            : Result.Failure(ConversationErrors.NotFound);
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
                            new BsonDocument("$eq", new BsonArray { new BsonDocument("$size", "$participant_info"), 0 })
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

    private BsonDocument BuildMessageProjection(string avatarDefault)
    {
        return new BsonDocument
        {
            { "_id", 1 },
            { "content", 1 },
            { "type", 1 },
            { "file_attachment", 1 },
            { "created_date", 1 },
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