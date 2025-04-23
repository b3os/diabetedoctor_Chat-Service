using ChatService.Domain.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class Group : DomainEntity<ObjectId>
{
    [BsonElement("name")]
    public string Name { get; private set; } = default!;
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = default!;
    [BsonElement("owner_by")]
    public string Owner { get; private set; } = default!;
    [BsonElement("admins")]
    public List<string> Admins { get; private set; } = default!;
    [BsonElement("members")]
    public List<string> Members {get; private set;} = default!;

    public static Group Create(ObjectId id, string name, Image avatar, string owner, List<string> members)
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
        Name = name ?? Name;
        Avatar = avatar ?? Avatar;
    }
}