using ChatService.Contract.DTOs;
using ChatService.Contract.DTOs.EnumDtos;

namespace ChatService.Application.Mapping;

public static class Mapper
{
    /// <summary>
    /// Map DTO sang ValueObject UserId.
    /// </summary>
    public static UserId MapUserId(UserIdDto dto)
    {
        return dto.ToDomain();
    }
}