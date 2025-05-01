using ChatService.Contract.DTOs.GroupDtos;

namespace ChatService.Contract.Services.Group.Commands;

public record CreateGroupCommand : ICommand
{
    public string OwnerId { get; init; } = default!;
    public GroupCreateDto Group { get; init; } = default!;
}