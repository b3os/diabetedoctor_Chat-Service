using ChatService.Domain.Abstractions;
using ChatService.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

[BsonIgnoreExtraElements]
public class User : DomainEntity<ObjectId>
{
    [BsonElement("user_id")]
    public UserId UserId { get; private set; } = null!;
    [BsonElement("full_name")]
    public string FullName { get; private set; } = null!;
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = null!;
    [BsonElement("role")]
    public RoleEnum Role { get; private set; } = default!;

    public static User Create(ObjectId id, UserId userId, string fullname, Image avatar)
    {
        return new User()
        {
            Id = id,
            UserId = userId,
            Avatar = avatar,
            FullName = fullname,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }

    public void Modify(string? fullname, Image? avatar)
    {
        var isChanged = false;
        
        if (!string.IsNullOrWhiteSpace(fullname) && !fullname.Equals(FullName))
        {
            FullName = fullname;
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