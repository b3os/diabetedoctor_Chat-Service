using ChatService.Contract.DTOs.GroupDtos;

namespace ChatService.Contract.Services.Group.Responses;

public record GetGroupMemberResponse
{
    public GroupDto Group { get; init; }
}