namespace ChatService.Contract.Services.Conversation.Commands.PersonalConversation;

public record CreatePersonalConversationCommand
{
    public string? UserId { get; init; }
    public string? DoctorId { get; init; }
}