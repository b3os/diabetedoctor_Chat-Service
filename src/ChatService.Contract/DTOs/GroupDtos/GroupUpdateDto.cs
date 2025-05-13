namespace ChatService.Contract.DTOs.GroupDtos;

public record GroupUpdateDto
{
    public string? Name { get; init; }
    public string? Avatar { get; init; }
}