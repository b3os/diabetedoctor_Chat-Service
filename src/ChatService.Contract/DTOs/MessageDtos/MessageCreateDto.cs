using ChatService.Contract.DTOs.EnumDtos;

namespace ChatService.Contract.DTOs.MessageDtos;

public record MessageCreateDto
{
    public required string Content {get; init;}
    public required MessageTypeDto Type {get; init;}
}