namespace ChatService.Contract.Services.Conversation.Commands;

public record CreatePersonalConversationCommand
{
    public string? UserId { get; init; }
    public string? DoctorId { get; init; }
}