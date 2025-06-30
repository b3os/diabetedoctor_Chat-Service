using Microsoft.AspNetCore.Mvc.Formatters;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs;

[BsonIgnoreExtraElements]
public record FileAttachmentDto
{
    [BsonElement("public_url")]
    public string PublicUrl { get; init; } = null!;
    
    [BsonElement("file_type")]
    public int Type { get; init; }
};