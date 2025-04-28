namespace ChatService.Contract.DTOs.GroupDtos;

public record GroupCreateDto
{
    public required string Name { get; init; }
    public required string Avatar { get; init; }
    public required string Owner { get; init; }
    public required List<string> Members { get; set; }
}