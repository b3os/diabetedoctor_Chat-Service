using System;
using System.Collections.Generic;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Enums;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class Message : DomainEntity<ObjectId>
{
    [BsonElement("group_id")]
    public ObjectId GroupId { get; private set; } = default!;
    [BsonElement("sender_id")]
    public UserId SenderId { get; private set; } = default!;
    [BsonElement("content")]
    public string Content { get; private set; } = default!;
    [BsonElement("message_type")]
    public MessageTypeEnum Type { get; private set; } = default!;
    [BsonElement("read_by")]
    public List<UserId> ReadBy { get; private set; } = default!;

    public static Message Create (ObjectId id, ObjectId groupId, UserId senderId, string content, MessageTypeEnum type)
    {
        return new Message()
        {
            Id = id,
            GroupId = groupId,
            SenderId = senderId,
            Content = content,
            Type = type,
            ReadBy = [senderId],
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
}