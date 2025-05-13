using ChatService.Contract.DTOs.EnumDtos;
using ChatService.Domain.Enums;

namespace ChatService.Application.Mapping;

public static class Mapper
{
    /// <summary>
    /// Map DTO sang ValueObject MessageType.
    /// </summary>
    public static MessageTypeEnum MapMessageType(MessageTypeDto dto)
    {
        return MessageTypeMapper.ToDomain(dto);
    }
}