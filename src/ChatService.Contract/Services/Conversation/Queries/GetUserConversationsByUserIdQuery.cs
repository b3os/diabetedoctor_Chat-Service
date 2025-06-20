using ChatService.Contract.Services.Conversation.Responses;

namespace ChatService.Contract.Services.Conversation.Queries;

public record GetUserConversationsByUserIdQuery : IQuery<GetUserConversationsResponse>
{
    public string UserId { get; init; } = string.Empty;
    public QueryFilter Filter { get; init; } = null!;
}