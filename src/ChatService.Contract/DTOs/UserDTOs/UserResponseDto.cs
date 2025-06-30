using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.UserDTOs;

[BsonIgnoreExtraElements]
public record UserResponseDto
{
    [BsonElement("_id")]
    public string Id { get; init; } = null!;
    
    [BsonElement("avatar")]
    public string Avatar { get; init; } = null!;
    
    [BsonElement("fullname")]
    public string FullName { get; init; } = null!;
    
    [BsonElement("role")]
    public int Role { get; init; }
}