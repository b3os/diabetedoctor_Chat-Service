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

    // [BsonElement("nickname")] 
    // public string? Nickname { get; private set; }
    
    [BsonElement("role")]
    public MemberRole Role { get; private set; }
    
    [BsonElement("invited_by")]
    public UserId InvitedBy { get; private set; } = null!;
    
    [BsonElement("status")]
    public Status Status { get; private set; }
    
    public static Participant CreateOwner(ObjectId id, UserId userId, ObjectId conversationId, UserId invitedBy)
    {
        return new Participant()
        {
            Id = id,
            UserId = userId,
            ConversationId = conversationId,
            Role = MemberRole.Owner,
            InvitedBy = invitedBy,
            Status = Status.Active,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
    
    public static Participant CreateAdmin(ObjectId id, UserId userId, ObjectId conversationId, UserId invitedBy)
    {
        return new Participant()
        {
            Id = id,
            UserId = userId,
            ConversationId = conversationId,
            Role = MemberRole.Admin,
            InvitedBy = invitedBy,
            Status = Status.Active,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
    
    public static Participant CreateDoctor(ObjectId id, UserId userId, ObjectId conversationId, UserId invitedBy)
    {
        return new Participant()
        {
            Id = id,
            UserId = userId,
            ConversationId = conversationId,
            Role = MemberRole.Doctor,
            InvitedBy = invitedBy,
            Status = Status.Active,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
    
    public static Participant CreateMember(ObjectId id, UserId userId, ObjectId conversationId, UserId invitedBy)
    {
        return new Participant()
        {
            Id = id,
            UserId = userId,
            ConversationId = conversationId,
            Role = MemberRole.Member,
            InvitedBy = invitedBy,
            Status = Status.Active,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }
    
    public void Ban()
    {
        Status = Status.LocalBan;
        ModifiedDate = CurrentTimeService.GetCurrentTime();
    }

    public void Unban()
    {
        Status = Status.Active;
        ModifiedDate = CurrentTimeService.GetCurrentTime();
    }
}