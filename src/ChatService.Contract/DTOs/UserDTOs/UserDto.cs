using ChatService.Contract.DTOs.MediaDTOs;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.UserDTOs;

[BsonIgnoreExtraElements]
public record UserDto
{
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = default!;
    
    [BsonElement("avatar")]
    public string Avatar { get; init; } = default!;
    
    [BsonElement("fullname")]
    public string FullName { get; init; } = default!;
}

