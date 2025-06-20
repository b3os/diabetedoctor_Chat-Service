namespace ChatService.Contract.Services.Conversation.Commands;

public record AddMemberToGroupCommand : ICommand<Response>
{
    public string AdminId { get; init; } = null!;
    public ObjectId ConversationId { get; init; }
    public HashSet<string> UserIds { get; init; } = [];
}