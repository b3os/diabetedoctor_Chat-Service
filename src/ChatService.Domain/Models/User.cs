using System;
using ChatService.Domain.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class User : DomainEntity<ObjectId>
{
    [BsonElement("user_id")]
    public string UserId { get; private set; } = default!;
    [BsonElement("device_id")]
    public string DeviceId { get; private set; } = default!;
    [BsonElement("fullname")]
    public string Fullname { get; private set; } = default!;
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = default!;

    public static User Create(ObjectId id, string userId, string deviceId, string fullname, Image avatar)
    {
        return new User()
        {
            Id = id,
            UserId = userId,
            DeviceId = deviceId,
            Avatar = avatar,
            Fullname = fullname,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public void Modify(string? deviceId, string? fullname, Image? avatar)
    {
        DeviceId = deviceId ?? DeviceId;
        Fullname = fullname ?? Fullname;
        Avatar = avatar ?? Avatar;
        ModifiedDate = DateTime.UtcNow;
    }
}