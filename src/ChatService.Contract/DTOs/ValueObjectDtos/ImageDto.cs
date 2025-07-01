using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ValueObjectDtos;

[BsonIgnoreExtraElements]
public class ImageDto
{
    [BsonElement("public_id")] 
    public string PublicId { get; private set; } = null!;
    
    [BsonElement("public_url")]
    public string PublicUrl { get; private init; } = null!;
}