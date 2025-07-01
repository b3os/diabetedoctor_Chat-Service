using ChatService.Contract.Enums;

namespace ChatService.Contract.DTOs.MessageDtos;

public record MessageCreateDto
{
    public ConversationTypeEnum ConversationType { get; init; }
    public string? Content {get; init;}
    public string? MediaId { get; init; }
    public MessageTypeEnum MessageType { get; init; }
}