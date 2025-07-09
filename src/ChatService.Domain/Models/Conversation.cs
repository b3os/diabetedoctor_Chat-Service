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
    public ConversationType ConversationType { get; private set; }
    
    [BsonElement("status")]
    public ConversationStatus Status { get; private set; }
    
    [BsonElement("members")]
    public List<UserId> Members { get; private set; } = [];
    
    [BsonElement("last_message")]
    public Message? LastMessage { get; private set; }
    
    [BsonElement("hospital_id")]
    public HospitalId? HospitalId { get; private set; }

    public static Conversation CreateGroup(ObjectId id, string name, Image avatar, List<UserId> members, HospitalId hospitalId)
    {
        return new Conversation()
        {
            Id = id,
            Name = name,
            Avatar = avatar,
            ConversationType = ConversationType.Group,
            Members = members,
            Status = ConversationStatus.Open,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            HospitalId = hospitalId,
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
            ConversationType = ConversationType.Personal,
            Members = members,
            Status = ConversationStatus.Open,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }

    public Result Modify(string? name)
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
        
        return errors.Count > 0 ? ValidationResult.WithErrors(errors.AsEnumerable().ToArray()) : Result.Success();
    }
}