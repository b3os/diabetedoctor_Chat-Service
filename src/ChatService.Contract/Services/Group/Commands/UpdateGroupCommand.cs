using ChatService.Contract.DTOs.GroupDtos;

namespace ChatService.Contract.Services.Group.Commands;

public record UpdateGroupCommand : ICommand
{
    public required string GroupId { get; init; }
    public required GroupUpdateDto GroupUpdateDto { get; init; }
}