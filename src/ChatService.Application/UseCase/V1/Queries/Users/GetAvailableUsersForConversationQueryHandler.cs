using ChatService.Contract.Common.Filters;
using ChatService.Contract.Enums;
using ChatService.Contract.Services.User.Queries;
using ChatService.Contract.Services.User.Responses;

namespace ChatService.Application.UseCase.V1.Queries.User;

public sealed class GetAvailableUsersForConversationQueryHandler(
    IMongoDbContext mongoDbContext)
    : IQueryHandler<GetAvailableUsersForConversationQuery, GetAvailableUsersResponse>
{
    public async Task<Result<GetAvailableUsersResponse>> Handle(GetAvailableUsersForConversationQuery request,
        CancellationToken cancellationToken)
    {
        var pageIndex = request.OffsetFilter.PageIndex is > 0 ? request.OffsetFilter.PageIndex.Value : 1;
        var pageSize = request.OffsetFilter.PageSize is > 0 ? request.OffsetFilter.PageSize.Value : 20;
        
        var filters = await BuildFilter(request.OffsetFilter, request.Role, request.UserId, cancellationToken);
        var sorter = BuildUserSort();
        var participantLookup = ParticipantLookup(request.ConversationId);
        var addStatusStage = AddStatusStage();
        var userProjection = BuildUserProjection();
        
        var result = await mongoDbContext.Users
            .Aggregate()
            .Match(filters)
            .Sort(sorter)
            .Skip((pageIndex - 1) * pageSize)
            .Limit(pageSize)
            .Lookup<Participant, Participant, IEnumerable<Participant>, Domain.Models.User>(
                foreignCollection: mongoDbContext.Participants,
                let: new BsonDocument("userId", "$user_id"),
                lookupPipeline: participantLookup,
                @as: "participant")
            .Unwind("participant", new AggregateUnwindOptions<Domain.Models.User>
            {
                PreserveNullAndEmptyArrays = true
            })
            .AppendStage<BsonDocument>(addStatusStage)
            .Project(userProjection)
            .As<UserResponseDto>()
            .ToListAsync(cancellationToken);
        
        return Result.Success(new GetAvailableUsersResponse
        {
            Users = PagedResult<UserResponseDto>.Create(result, pageIndex, pageSize, result.Count)
        });
    }

    private async Task<FilterDefinition<Domain.Models.User>> BuildFilter(QueryOffsetFilter offsetFilter, RoleEnum roleEnum, 
        string staffId, CancellationToken cancellationToken = default)
    {
        var role = roleEnum.ToEnum<RoleEnum, Role>();
        var builder = Builders<Domain.Models.User>.Filter;
        
        var filters = new List<FilterDefinition<Domain.Models.User>> { builder.Eq(u => u.Role, role) };
        
        if (role is Role.HospitalStaff or Role.Doctor)
        {
            var userId = UserId.Of(staffId); 
            var user = await mongoDbContext.Users.Find(u => u.UserId == userId).FirstOrDefaultAsync(cancellationToken);
            filters.Add(builder.Eq(u => u.HospitalId, user.HospitalId));   
        }
        
        if (!string.IsNullOrWhiteSpace(offsetFilter.Search))
        {
            var search = SearchHelper.NormalizeSearchText(offsetFilter.Search);
            filters.Add(builder.Or(
                builder.Regex(u => u.FullName, new BsonRegularExpression(search, "i")),
                builder.Regex(u => u.PhoneNumber, new BsonRegularExpression(search, "i"))));
        }

        return filters.Count > 0 ? builder.And(filters) : builder.Empty;
    }

    private static SortDefinition<Domain.Models.User> BuildUserSort()
    {
        var builder = Builders<Domain.Models.User>.Sort;
        return builder.Combine(
            builder.Ascending(u => u.FullName.FirstName),
            builder.Ascending(u => u.FullName.MiddleName),
            builder.Ascending(u => u.FullName.LastName),
            builder.Descending(u => u.Id)
        );
    }

    private static PipelineDefinition<Participant, Participant> ParticipantLookup(ObjectId conversationId)
    {
        return new EmptyPipelineDefinition<Participant>()
            .Match(new BsonDocument
            {
                {
                    "$expr", new BsonDocument
                    {
                        {
                            "$and", new BsonArray
                            {
                                new BsonDocument("$eq", new BsonArray { "$user_id", "$$userId" }),
                                new BsonDocument("$eq", new BsonArray { "$conversation_id", conversationId })
                            }
                        }
                    }
                }
            })
            .Limit(1);
    }

    private static BsonDocument AddStatusStage()
    {
        return new BsonDocument("$addFields", new BsonDocument
        {
            {
                "status", new BsonDocument("$switch", new BsonDocument
                {
                    {
                        "branches", new BsonArray
                        {
                            // banned (user level)
                            new BsonDocument
                            {
                                { "case", new BsonDocument("$eq", new BsonArray { "$is_deleted", true }) },
                                { "then", ConversationUserStatusEnum.SystemBanned }
                            },

                            // available (no participant)
                            new BsonDocument
                            {
                                {
                                    "case", new BsonDocument("$not", new BsonArray
                                    {
                                        new BsonDocument("$ifNull", new BsonArray { "$participant._id", false })
                                    })
                                },
                                { "then", ConversationUserStatusEnum.Available }
                            },

                            // available (rejoinable)
                            new BsonDocument
                            {
                                {
                                    "case", new BsonDocument("$and", new BsonArray
                                    {
                                        new BsonDocument("$eq", new BsonArray { "$participant.is_deleted", true }),
                                        new BsonDocument("$eq",
                                            new BsonArray { "$participant.status", Status.Active })
                                    })
                                },
                                { "then", ConversationUserStatusEnum.Available }
                            },

                            // banned
                            new BsonDocument
                            {
                                {
                                    "case",
                                    new BsonDocument("$eq",
                                        new BsonArray { "$participant.status", Status.LocalBan })
                                },
                                { "then", ConversationUserStatusEnum.Banned }
                            }
                        }
                    },
                    { "default", ConversationUserStatusEnum.AlreadyInGroup }
                })
            }
        });
    }

    private static BsonDocument BuildUserProjection()
    {
        return new BsonDocument
        {
            { "_id", "$user_id._id" },
            { "avatar", "$avatar.public_url" },
            { "full_name", "$display_name" },
            { "status", 1 },
            { "role", 1 }
        };
    }
}