using ChatService.Contract.DTOs.EnumDtos;

namespace ChatService.Application.Mapping;

public static class ValueObjectMapper
{
    /// <summary>
    /// Map DTO sang ValueObject MessageType.
    /// </summary>
    public static Domain.ValueObjects.MessageType MapMessageType(MessageTypeDto dto)
    {
        var enumValue = MessageTypeMapper.ToDomain(dto);
        return Domain.ValueObjects.MessageType.Of(enumValue);
    }
}