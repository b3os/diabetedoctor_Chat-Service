using ChatService.Contract.DTOs.EnumDtos;

namespace ChatService.Contract.DTOs.MessageDtos;

public record MessageCreateDto
{
    public string? Content {get; init;} = null!;
    public MessageTypeDto Type {get; init;}
    public HashSet<string> ReadBy { get; init; } = [];
}