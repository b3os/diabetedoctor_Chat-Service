using ChatService.Contract.DTOs.GroupDtos;

namespace ChatService.Contract.Services.Group.Commands;

public record CreateGroupCommand : ICommand
{
    public GroupCreateDto Group { get; init; } = default!;
}