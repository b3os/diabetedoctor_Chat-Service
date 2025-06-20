namespace ChatService.Contract.Services.Conversation.Commands;

public record UpdateConversationCommand : ICommand<Response>
{
    public string AdminId { get; init; } = null!;
    public ObjectId GroupId { get; init; }
    public string? Name { get; init; }
    public string? Avatar { get; init; }
    public int Version { get; init; }
}