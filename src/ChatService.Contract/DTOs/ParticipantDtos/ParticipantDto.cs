using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ParticipantDtos;

[BsonIgnoreExtraElements]
public record ParticipantDto
{
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.String)]
    public string? Id { get; private set; }
    
    [BsonElement("conversation_id")]
    public ObjectId? ConversationId { get; private set; }
    
    [BsonElement("full_name")]
    public string FullName { get; private set; } = null!;
    
    [BsonElement("avatar")]
    public string Avatar { get; private set; } = null!;
    
    [BsonElement("role")]
    public int Role { get; private set; }
    
    [BsonElement("invited_by")]
    public string InvitedBy { get; private set; } = null!;
}