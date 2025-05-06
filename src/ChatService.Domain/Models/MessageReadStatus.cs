using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class MessageReadStatus : DomainEntity<ObjectId>
{
    [BsonElement("user_id")]
    public UserId UserId { get; private set; } = default!;
    [BsonElement("group_id")]
    public ObjectId GroupId { get; private set; }
    [BsonElement("last_read_message_id")]
    public ObjectId LastReadMessageId { get; private set; }
    
    public static MessageReadStatus Create(ObjectId id, UserId userId, ObjectId groupId, ObjectId messageId)
    {
        return new MessageReadStatus()
        {
            Id = id,
            UserId = userId,
            GroupId = groupId,
            LastReadMessageId = messageId,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }

    public void Update(ObjectId messageId)
    {
        Changes["last_read_message_id"] = messageId;
        Changes["modified_date"] = CurrentTimeService.GetCurrentTime();
    }
}