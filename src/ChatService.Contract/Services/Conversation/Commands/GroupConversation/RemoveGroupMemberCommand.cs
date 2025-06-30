namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record RemoveGroupMemberCommand : ICommand<Response>
{
    public string AdminId { get; init; } = null!;
    public ObjectId ConversationId { get; init; }
    public string MemberId { get; init; } = null!;
}