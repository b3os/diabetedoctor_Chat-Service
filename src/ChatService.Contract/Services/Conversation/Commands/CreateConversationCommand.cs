using ChatService.Contract.Services.Conversation.Responses;

namespace ChatService.Contract.Services.Conversation.Commands;

public record CreateConversationCommand : ICommand<Response<CreateConversationResponse>>
{
    public string? OwnerId { get; init; }
    public string? Name { get; init; }
    public string? Avatar { get; init; }
    public HashSet<string> Members { get; init; } = [];
}