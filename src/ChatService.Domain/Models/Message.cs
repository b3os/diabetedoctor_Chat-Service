using System;
using System.Collections.Generic;
using ChatService.Domain.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class Message : DomainEntity<ObjectId>
{
    [BsonElement("group_id")]
    public ObjectId GroupId { get; private set; } = default!;
    [BsonElement("sender_id")]
    public string SenderId { get; private set; } = default!;
    [BsonElement("content")]
    public string Content { get; private set; } = default!;
    [BsonElement("read_by")]
    public List<string> ReadBy { get; private set; } = default!;

    public static Message Create (ObjectId groupId, string senderId, string content)
    {
        return new Message()
        {
            Id = ObjectId.GenerateNewId(),
            GroupId = groupId,
            SenderId = senderId,
            Content = content,
            ReadBy = [],
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
}