using ChatService.Contract.DTOs.ValueObjectDtos;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.UserDTOs;

[BsonIgnoreExtraElements]
public record UserDto
{
    [BsonElement("user_id")]
    public UserIdDto UserId { get; init; } = null!;
    
    [BsonElement("avatar")]
    public ImageDto Avatar { get; init; } = null!;
    
    [BsonElement("display_name")]
    public string FullName { get; init; } = null!;
    
    [BsonElement("role")]
    public int Role { get; init; }
}

