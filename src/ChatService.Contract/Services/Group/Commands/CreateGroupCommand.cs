using ChatService.Contract.DTOs.GroupDtos;

namespace ChatService.Contract.Services.Group.Commands;

public record CreateGroupCommand : ICommand
{
    public string? OwnerId { get; init; }
    public string? Name { get; init; }
    public string? Avatar { get; init; }
    public HashSet<string>? Members { get; init; } = [];
}