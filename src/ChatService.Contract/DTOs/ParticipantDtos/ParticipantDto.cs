using ChatService.Contract.DTOs.UserDTOs;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ParticipantDtos;

[BsonIgnoreExtraElements]
public record ParticipantDto
{
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.String)]
    public string? Id { get; init; }
    
    [BsonElement("conversation_id")]
    public ObjectId? ConversationId { get; init; }
    
    [BsonElement("role")]
    public int Role { get; init; }
    
    [BsonElement("invited_by")]
    public string InvitedBy { get; init; } = null!;
    
    [BsonElement("user")]
    public UserDto User { get; init; } = null!;
};