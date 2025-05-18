using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.UserDTOs;

public record DuplicatedUserDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? GroupId { get; init; } = null!;

    [BsonElement("matchCount")] public int MatchCount { get; init; } = 0!;

    [BsonElement("duplicatedUser")] public List<UserDto>? DuplicatedUsers { get; init; } = null!;
}