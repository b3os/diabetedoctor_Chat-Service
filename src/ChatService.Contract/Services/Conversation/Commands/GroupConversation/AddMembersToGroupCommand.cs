namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record AddMembersToGroupCommand : ICommand<Response>
{
    public string? AdminId { get; init; }
    public ObjectId? ConversationId { get; init; }
    public HashSet<string> UserIds { get; init; } = [];
}