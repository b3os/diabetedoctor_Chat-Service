using ChatService.Contract.DTOs.UserDTOs;

namespace ChatService.Contract.Services.Conversation.Responses;

public record AddMemberToGroupResponse
{
    public int MatchCount { get; init; }
    public List<UserDto> DuplicatedUser { get; init; } = [];
}