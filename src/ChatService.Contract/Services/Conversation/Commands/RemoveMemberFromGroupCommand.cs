namespace ChatService.Contract.Services.Conversation.Commands;

public record RemoveMemberFromGroupCommand : ICommand<Response>
{
    public string AdminId { get; init; } = null!;
    public ObjectId ConversationId { get; init; }
    public string MemberId { get; init; } = null!;
}