using ChatService.Domain.Abstractions;
using ChatService.Domain.Enums;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class Group : DomainEntity<ObjectId>
{
    [BsonElement("name")]
    public string Name { get; private set; } = default!;
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = default!;
    [BsonElement("members")]
    public List<Member> Members {get; private set;} = default!;

    public static Group Create(ObjectId id, string name, Image avatar, UserId ownerId, List<UserId> memberIds)
    {
        var members = new List<Member> { Member.Create(ownerId, GroupRoleEnum.Owner) };

        members.AddRange(Member.CreateMany(memberIds));
        
        return new Group()
        {
            Id = id,
            Name = name,
            Avatar = avatar, 
            Members = members,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
}

public record Member
{
    [BsonElement("user_id")]
    public UserId UserId { get; private set; } = default!;
    [BsonElement("role")]
    public GroupRoleEnum Role { get; private set; }
    
    public static Member Create(UserId userId, GroupRoleEnum role)
    {
        return new Member
        {
            UserId = userId,
            Role = role
        };
    }
    
    public static IEnumerable<Member> CreateMany(IEnumerable<UserId> userIds)
    {
        return userIds.Select(userId => Create(userId, GroupRoleEnum.Member)).ToList();
    }
}