using ChatService.Contract.Services.Group.Responses;
using ChatService.Contract.Services.Message.Responses;

namespace ChatService.Contract.Services.Message.Queries;

public record GetGroupMessageByIdQuery : IQuery<GetGroupMessageResponse>
{
    public string GroupId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public QueryFilter Filter { get; init; } = new();
};