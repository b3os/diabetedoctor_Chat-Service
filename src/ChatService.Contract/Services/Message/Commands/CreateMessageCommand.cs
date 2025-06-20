using ChatService.Contract.DTOs.EnumDtos;

namespace ChatService.Contract.Services.Message.Commands;

public record CreateMessageCommand : ICommand<Response>
{
    public ObjectId ConversationId { get; init; }
    public ConversationTypeDto ConversationType { get; init; }
    public string UserId { get; init; } = null!;
    public string? Content {get; init;}
    public MessageTypeDto MessageType {get; init;}
}