using ChatService.Contract.Services.Group.Responses;

namespace ChatService.Contract.Services.Message.Queries;

public record GetGroupMessageByIdQuery : IQuery<PagedList<GetUserGroupResponse>>
{
    public string GroupId { get; init; } = string.Empty;
    public QueryFilter Filter { get; init; } = new();
};