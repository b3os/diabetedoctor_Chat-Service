namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record DeleteGroupConversationCommand : ICommand<Response>
{
    public string StaffId { get; init; } = null!;
    public ObjectId ConversationId {get; init;}
}