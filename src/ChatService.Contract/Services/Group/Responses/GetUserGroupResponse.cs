using ChatService.Contract.DTOs.GroupDtos;

namespace ChatService.Contract.Services.Group.Responses;

public record GetUserGroupResponse
{
    public PagedList<GroupDto> Groups { get; init; } = default!;
}