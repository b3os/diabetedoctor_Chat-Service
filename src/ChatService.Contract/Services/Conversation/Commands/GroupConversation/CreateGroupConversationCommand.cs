using ChatService.Contract.Services.Conversation.Responses;

namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record CreateGroupConversationCommand : ICommand<Response<CreateGroupConversationResponse>>
{
    public string OwnerId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public HashSet<string> Members { get; init; } = [];
}