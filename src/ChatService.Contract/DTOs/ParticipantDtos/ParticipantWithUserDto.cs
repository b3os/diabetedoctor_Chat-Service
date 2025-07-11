using ChatService.Contract.DTOs.ValueObjectDtos;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ParticipantDtos;

[BsonIgnoreExtraElements]
public record ParticipantWithUserDto
{
    [BsonElement("user_id")] 
    public UserIdDto UserId { get; init; } = null!;
    
    [BsonElement("avatar")]
    public string? Avatar { get; init; }
    
    [BsonElement("full_name")]
    public string? FullName { get; init; }
    
    [BsonElement("status")]
    public int Status { get; init; }
    
    [BsonElement("is_deleted"), BsonRepresentation(BsonType.Boolean)]
    public bool? IsDeleted { get; init; }
}