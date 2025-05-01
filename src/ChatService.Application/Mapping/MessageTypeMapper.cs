using ChatService.Contract.DTOs.EnumDtos;
using ChatService.Domain.Enums;

namespace ChatService.Application.Mapping;

public static class MessageTypeMapper
{
    public static MessageTypeEnum ToDomain(MessageTypeDto messageTypeDto)
    {
        return messageTypeDto switch
        {
            MessageTypeDto.Picture => MessageTypeEnum.Picture,
            MessageTypeDto.Text => MessageTypeEnum.Text,
            _ => throw new ArgumentOutOfRangeException(nameof(messageTypeDto) ,"Invalid message type")
        };
    }
}