using ChatService.Contract.Enums;

namespace ChatService.Contract.Services.Message.Commands;

public record CreateMessageCommand : ICommand<Response>
{
    public ObjectId ConversationId { get; init; }
    public ConversationTypeEnum ConversationType { get; init; }
    public string UserId { get; init; } = null!;
    public string? Content {get; init;}
    public string? MediaId { get; init; }
    public MessageTypeEnum MessageType { get; init; }
}