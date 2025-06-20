using ChatService.Contract.DTOs.EnumDtos;

namespace ChatService.Contract.DTOs.MessageDtos;

public record MessageCreateDto
{
    public ConversationTypeDto ConversationType { get; init; }
    public string? Content {get; init;} = null!;
    public MessageTypeDto Type {get; init;}
    
}