namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record AddDoctorToGroupCommand : ICommand<Response>
{
    public string StaffId { get; init; } = null!;
    public ObjectId ConversationId { get; init; }
    public string DoctorId { get; init; } = null!;
}