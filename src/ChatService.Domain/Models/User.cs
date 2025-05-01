using System;
using ChatService.Contract.Helpers;
using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class User : DomainEntity<ObjectId>
{
    [BsonElement("user_id")]
    public UserId UserId { get; private set; } = default!;
    [BsonElement("fullname")]
    public string Fullname { get; private set; } = default!;
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = default!;

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
        
        if (fullname != null && !fullname.Equals(Fullname))
        {
            Changes["fullname"] = fullname;
            isChanged = true;
        }
        
        if (avatar != null)
        {
            Changes["avatar"] = avatar;
            isChanged = true;
        }

        if (isChanged)
        {
            Changes["modified_date"] = CurrentTimeService.GetCurrentTime();
        }
        
        // Fullname = fullname ?? Fullname;
        // Avatar =  avatar ?? Avatar;
        // ModifiedDate = CurrentTimeService.GetCurrentTime();
    }
}