using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ValueObjectDtos;

[BsonIgnoreExtraElements]
public record FullNameDto
{
    [BsonElement("last_name")]
    public string LastName { get; init; } = null!;
    
    [BsonElement("middle_name")]
    public string? MiddleName { get; init; }
    
    [BsonElement("first_name")]
    public string FirstName { get; init; } = null!;
}