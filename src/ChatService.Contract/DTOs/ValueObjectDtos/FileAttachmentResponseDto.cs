using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ValueObjectDtos;

[BsonIgnoreExtraElements]
public record FileAttachmentResponseDto
{
    [BsonElement("public_url")]
    public string PublicUrl { get; init; } = null!;
    
    [BsonElement("file_type")]
    public int Type { get; init; }
}