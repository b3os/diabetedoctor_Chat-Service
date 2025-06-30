using ChatService.Contract.DTOs.UserDTOs;

namespace ChatService.Contract.Services.Conversation.Responses;

public record AddMemberToGroupResponse
{
    public int MatchCount { get; init; }
    public List<UserResponseDto> DuplicatedUser { get; init; } = [];
    public List<UserResponseDto> BannedUser { get; init; } = [];
}