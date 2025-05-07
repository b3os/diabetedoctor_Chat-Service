using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class Group : DomainEntity<ObjectId>
{
    [BsonElement("name")]
    public string Name { get; private set; } = default!;
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = default!;
    [BsonElement("owner_by")]
    public UserId Owner { get; private set; } = default!;
    [BsonElement("admins")]
    public List<UserId> Admins { get; private set; } = default!;
    [BsonElement("members")]
    public List<UserId> Members {get; private set;} = default!;

    public static Group Create(ObjectId id, string name, Image avatar, UserId owner, List<UserId> members)
    {
        return new Group()
        {
            Id = id,
            Name = name,
            Avatar = avatar, 
            Owner= owner,
            Admins = [owner],
            Members = members,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }

    public void Modify(string? name, Image? avatar)
    {
        var isChanged = false;
        
        if (!string.IsNullOrWhiteSpace(name) && !name.Equals(Name))
        {
            Changes["name"] = name;
            isChanged = true;
        }

        if (avatar != null)
        {
            Changes["avatar"] = new { public_url = avatar.PublicUrl };
            isChanged = true;
        }
        
        if (isChanged)
        {
            Changes["modified_date"] = CurrentTimeService.GetCurrentTime();
        }
        
        // Name = name ?? Name;
        // Avatar = avatar ?? Avatar;
    }

    public void AddAdmin(UserId admin)
    {
        Changes["admins"] = new List<UserId>{admin} ;
    }
    
    public void AddMembers(List<UserId> members)
    {
        Changes["members"] = members;
    }
}