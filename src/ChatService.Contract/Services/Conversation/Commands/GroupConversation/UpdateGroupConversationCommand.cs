namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record UpdateGroupConversationCommand : ICommand<Response>
{
    public string StaffId { get; init; } = null!;
    public ObjectId ConversationId { get; init; }
    public string? Name { get; init; }
    public string? AvatarId { get; init; }
}