namespace ChatService.Contract.DTOs.GroupDtos;

public record GroupUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
}