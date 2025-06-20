using ChatService.Domain.Abstractions;
using ChatService.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class Participant : DomainEntity<ObjectId>
{
    [BsonElement("user_id")]
    public UserId UserId { get; private set; } = null!;
    
    [BsonElement("conversation_id")]
    public ObjectId ConversationId { get; private set; }
    
    [BsonElement("full_name")]
    public string FullName { get; private set; } = null!;
    
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = null!;
    
    [BsonElement("role")]
    public MemberRoleEnum Role { get; private set; }
    
    [BsonElement("invited_by")]
    public UserId InvitedBy { get; private set; } = null!;
    
    // [BsonElement("last_seen_at")]
    // public DateTime? LastSeenAt { get; private set; }
    
    public static Participant Create(ObjectId id, UserId userId, ObjectId conversationId, string fullName, Image avatar ,MemberRoleEnum role, UserId invitedBy)
    {
        return new Participant()
        {
            Id = id,
            UserId = userId,
            ConversationId = conversationId,
            FullName = fullName,
            Avatar = avatar,
            // LastSeenAt = null,
            Role = role,
            InvitedBy = invitedBy,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }

    public void ChangeRole(MemberRoleEnum role)
    {
        Role = role;
        ModifiedDate = CurrentTimeService.GetCurrentTime();
    }
}