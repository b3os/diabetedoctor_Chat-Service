using ChatService.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.ValueObjects;

public record MessageType
{
    [BsonElement("value")]
    public MessageTypeEnum Value { get; init; }

    public static MessageType Of(MessageTypeEnum value)
    {
        if (!Enum.IsDefined(typeof(MessageTypeEnum), value))
            throw new ArgumentException("Invalid message type.");

        return new MessageType { Value = value };
    }
}