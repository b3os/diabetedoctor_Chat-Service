namespace ChatService.Contract.DTOs.MessageDtos;

public record MessageCreateDto
{
    public required string Content {get; init;}
}