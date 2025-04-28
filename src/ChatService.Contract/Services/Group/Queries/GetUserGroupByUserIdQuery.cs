using ChatService.Contract.Services.Group.Responses;

namespace ChatService.Contract.Services.Group.Queries;

public record GetUserGroupByUserIdQuery : IQuery<GetUserGroupResponse>
{
    public QueryFilter Filter { get; init; }
}