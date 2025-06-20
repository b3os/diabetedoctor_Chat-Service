using ChatService.Contract.Abstractions.Shared;
using ChatService.Contract.Common.DomainErrors;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class Conversation : DomainEntity<ObjectId>
{
    [BsonElement("name")]
    public string Name { get; private set; } = null!;
    
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = null!;
    
    [BsonElement("type")]
    public ConversationTypeEnum ConversationType { get; private set; }
    
    [BsonElement("members")]
    public List<UserId> Members { get; private set; } = [];
    
    [BsonElement("last_message")]
    public Message? LastMessage { get; private set; }

    public static Conversation CreateGroup(ObjectId id, string name, Image avatar, List<UserId> members)
    {
        return new Conversation()
        {
            Id = id,
            Name = name,
            Avatar = avatar,
            ConversationType = ConversationTypeEnum.Group,
            Members = members,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
    
    public static Conversation CreatePersonal(ObjectId id, string name, Image avatar, List<UserId> members)
    {
        return new Conversation()
        {
            Id = id,
            Name = name,
            Avatar = avatar,
            ConversationType = ConversationTypeEnum.Personal,
            Members = members,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }

    public Result Modify(string? name, Image? avatar)
    {
        var errors = new List<Error>();
        
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(ConversationErrors.SameAsCurrentName);
            }
    
            Name = name;
        }

        if (avatar != null)
        {
            Avatar = avatar;
        }
        
        return errors.Count > 0 ? ValidationResult.WithErrors(errors.AsEnumerable().ToArray()) : Result.Success();
    }
}