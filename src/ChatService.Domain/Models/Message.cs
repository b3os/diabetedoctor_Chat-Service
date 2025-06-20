using ChatService.Domain.Abstractions;
using ChatService.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class Message : DomainEntity<ObjectId>
{
    [BsonElement("conversation_id")]
    public ObjectId ConversationId { get; private set; }
    [BsonElement("sender_id")]
    public UserId? SenderId { get; private set; } = null!;
    [BsonElement("content")]
    public string? Content { get; private set; } = null!;
    [BsonElement("message_type")]
    public MessageTypeEnum Type { get; private set; }

    public static Message Create (ObjectId id, ObjectId conversationId, UserId? senderId, string content, MessageTypeEnum type)
    {
        return new Message()
        {
            Id = id,
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            Type = type,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
    
    public static Message CreateFromEvent (ObjectId id, ObjectId conversationId, UserId? senderId, string? content, DateTime? createdDate, MessageTypeEnum type)
    {
        return new Message()
        {
            Id = id,
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            Type = type,
            CreatedDate = createdDate,
            IsDeleted = false
        };
    }
}