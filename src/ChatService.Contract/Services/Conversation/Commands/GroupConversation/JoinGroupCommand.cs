namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record JoinGroupCommand : ICommand<Response>
{
    public ObjectId ConversationId { get; init; }
    public string InvitedBy { get; init; } = null!;
    public string UserId { get; init; } = null!;
};