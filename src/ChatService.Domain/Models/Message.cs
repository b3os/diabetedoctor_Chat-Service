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
    public MessageType Type { get; private set; }
    
    [BsonElement("file_attachment")]
    public FileAttachment? File { get; private set; } = null!;
    
    public static Message CreateText(ObjectId id, ObjectId conversationId, UserId? senderId, string content)
    {
        return new Message()
        {
            Id = id,
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            Type = MessageType.Text,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
    
    public static Message CreateFile(ObjectId id, ObjectId conversationId, UserId? senderId, string originalFileName, FileAttachment file)
    {
        return new Message()
        {
            Id = id,
            ConversationId = conversationId,
            SenderId = senderId,
            Content = originalFileName,
            Type = MessageType.File,
            File = file,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
    
    
    public static Message CreateFromEvent(ObjectId id, ObjectId conversationId, UserId? senderId, string? content, DateTime? createdDate, MessageType type, FileAttachment? file)
    {
        return new Message()
        {
            Id = id,
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            Type = type,
            File = file,
            CreatedDate = createdDate,
            IsDeleted = false
        };
    }
}