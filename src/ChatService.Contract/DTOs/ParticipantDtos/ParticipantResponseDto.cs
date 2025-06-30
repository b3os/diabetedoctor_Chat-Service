using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ParticipantDtos;

[BsonIgnoreExtraElements]
public record ParticipantResponseDto
{
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.String)]
    public string? Id { get; init; }
    
    [BsonElement("conversation_id")]
    public ObjectId? ConversationId { get; init; }
    
    [BsonElement("full_name")]
    public string FullName { get; init; } = null!;
    
    [BsonElement("avatar")]
    public string Avatar { get; init; } = null!;
    
    [BsonElement("role")]
    public int Role { get; init; }
    
    [BsonElement("invited_by")]
    public string InvitedBy { get; init; } = null!;
}