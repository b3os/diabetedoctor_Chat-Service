using ChatService.Contract.DTOs;

namespace ChatService.Application.Mapping;

public static class UserIdMapper
{
    public static UserId ToDomain(this UserIdDto dto) => UserId.Of(dto.Id);
}