using ChatService.Contract.Services.Conversation.Responses;

namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record CreateGroupConversationCommand : ICommand<Response<CreateGroupConversationResponse>>
{
    public string? OwnerId { get; init; }
    public string? Name { get; init; }
    public HashSet<string> Members { get; init; } = [];
}