namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record AddAdminToGroupCommand : ICommand<Response>
{
    public string StaffId { get; init; } = null!;
    public ObjectId ConversationId { get; init; }
    public string UserId { get; init; } = null!;
};