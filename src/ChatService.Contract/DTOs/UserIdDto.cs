using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs;

public record UserIdDto
{
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; } = null!;
};