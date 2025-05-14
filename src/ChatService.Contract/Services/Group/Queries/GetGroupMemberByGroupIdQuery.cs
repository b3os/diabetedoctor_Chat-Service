using ChatService.Contract.Services.Group.Responses;

namespace ChatService.Contract.Services.Group.Queries;

public record GetGroupMemberByGroupIdQuery : IQuery<GetGroupMemberResponse>
{
    public string UserId { get; set; } = default!;
    public ObjectId GroupId { get; set; }
    public QueryFilter Filter { get; init; } = default!;
}