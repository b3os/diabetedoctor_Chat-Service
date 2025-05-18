using System;
using ChatService.Contract.Helpers;
using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class User : DomainEntity<ObjectId>
{
    [BsonElement("user_id")]
    public UserId UserId { get; private set; } = null!;
    [BsonElement("fullname")]
    public string Fullname { get; private set; } = null!;
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = null!;

    public static User Create(ObjectId id, UserId userId, string fullname, Image avatar)
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
        var isChanged = false;
        
        if (!string.IsNullOrWhiteSpace(fullname) && !fullname.Equals(Fullname))
        {
            Fullname = fullname;
            isChanged = true;
        }
        
        if (avatar != null)
        {
            Avatar = avatar;
            isChanged = true;
        }

        if (isChanged)
        {
            ModifiedDate = CurrentTimeService.GetCurrentTime();
        }
    }
}