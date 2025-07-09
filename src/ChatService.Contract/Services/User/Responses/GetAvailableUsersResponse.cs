using ChatService.Contract.DTOs.UserDTOs;

namespace ChatService.Contract.Services.User.Responses;

public record GetAvailableUsersResponse
{
    public PagedResult<UserResponseDto> Users { get; init; } = null!;
}