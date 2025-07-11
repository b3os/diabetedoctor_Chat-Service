using ChatService.Contract.Common.Filters;
using ChatService.Contract.Enums;
using ChatService.Contract.Services.User.Responses;

namespace ChatService.Contract.Services.User.Queries;

public record GetAvailableUsersForConversationQuery
    : IQuery<GetAvailableUsersResponse>
{
    public string UserId { get; init; } = string.Empty;
    public ObjectId ConversationId { get; init; }
    public RoleEnum Role { get; init; }
    public QueryOffsetFilter OffsetFilter { get; init; } = new();
}