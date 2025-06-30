namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record UpdateGroupConversationCommand : ICommand<Response>
{
    public string? AdminId { get; init; }
    public ObjectId? ConversationId { get; init; }
    public string? Name { get; init; }
    public string? AvatarId { get; init; }
}