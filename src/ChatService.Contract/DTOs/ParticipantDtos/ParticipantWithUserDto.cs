using ChatService.Contract.DTOs.UserDTOs;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ParticipantDtos;

public record ParticipantWithUserDto
{
    [BsonElement("_id")] 
    public string Id { get; init; } = null!;
    
    [BsonElement("conversation_id")]
    public ObjectId ConversationId { get; init; }
    
    [BsonElement("role")]
    public int Role { get; init; }
    
    [BsonElement("invited_by")]
    public string InvitedBy { get; init; } = null!;
    
    [BsonElement("status")]
    public int Status { get; init; }
    
    [BsonElement("is_deleted"), BsonRepresentation(BsonType.Boolean)]
    public bool? IsDeleted { get; init; }
    
    [BsonElement("user")]
    public UserDto User { get; init; } = null!; 
};