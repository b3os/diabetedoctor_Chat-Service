using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.DTOs.UserDTOs;
using ChatService.Contract.Services.Group.Queries;
using ChatService.Contract.Services.Group.Responses;
using ChatService.Domain.Models;

namespace ChatService.Application.UseCase.V1.Queries.Group;

public class GetGroupMemberByGroupIdQueryHandler(IMongoDbContext context)
    : IQueryHandler<GetGroupMemberByGroupIdQuery, GetGroupMemberResponse>
{
    public async Task<Result<GetGroupMemberResponse>> Handle(GetGroupMemberByGroupIdQuery request,
        CancellationToken cancellationToken)
    {
        
        
        var userLookupPipeline = new EmptyPipelineDefinition<User>()
            .Match(new BsonDocument
                { { "$expr", new BsonDocument("$eq", new BsonArray { "$user_id._id", "$$userId" }) } })
            .Project(new BsonDocument
            {
                { "_id", "$user_id._id" },
                { "fullname", "$fullname" },
                { "avatar", "$avatar.public_url" },
            })
            .As<User, BsonDocument, UserDto>();

        var group = await context.Groups
            .Aggregate()
            .Match(group => group.Id == request.GroupId)
            .Unwind(group => group.Members)
            .Lookup<User, UserDto, IEnumerable<UserDto>, GroupDto>(
                foreignCollection: context.Users,
                let: new BsonDocument("userId", "$members.user_id._id"),
                lookupPipeline: userLookupPipeline,
                @as: "user")
            .Unwind("user")
            .Group(new BsonDocument
            {
                { "_id", "$_id" },
                { "name", new BsonDocument("$first", "$name") },
                {
                    "members", new BsonDocument("$push", new BsonDocument
                    {
                        { "_id", "$user._id" },
                        { "fullname", "$user.fullname" },
                        { "avatar", "$user.avatar" },
                        { "role", "$members.role" }
                    })
                }
            })
            .Project(new BsonDocument
            {
                { "_id", 1 },
                { "name", 1 },
                {
                    "members", new BsonDocument("$sortArray", new BsonDocument
                    {
                        { "input", "$members" },
                        {
                            "sortBy", new BsonDocument
                            {
                                { "role", 1 },
                                { "fullname", 1 },
                                { "_id", 1 }
                            }
                        }
                    })
                }
            })
            .As<GroupDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new GetGroupMemberResponse());
    }
}