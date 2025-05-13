using ChatService.Contract.DTOs.EnumDtos;

namespace ChatService.Contract.DTOs.MessageDtos;

public record MessageCreateDto
{
    public string? Content {get; init;} = default!;
    public MessageTypeDto Type {get; init;}
}