using System;
using ChatService.Contract.Helpers;
using ChatService.Domain.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class User : DomainEntity<ObjectId>
{
    [BsonElement("user_id")]
    public string UserId { get; private set; } = default!;
    [BsonElement("fullname")]
    public string Fullname { get; private set; } = default!;
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = default!;

    public static User Create(ObjectId id, string userId, string fullname, Image avatar)
    {
        return new User()
        {
            Id = id,
            UserId = userId,
            Avatar = avatar,
            Fullname = fullname,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }

    public void Modify(string? fullname, Image? avatar)
    {
        Fullname = fullname ?? Fullname;
        Avatar =  avatar ?? Avatar;
        ModifiedDate = CurrentTimeService.GetCurrentTime();
    }
}