using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.MediaDTOs;

public record ImageDTO
{
    [BsonElement("public_url")]
    public string ImageUrl { get; set; }
}
