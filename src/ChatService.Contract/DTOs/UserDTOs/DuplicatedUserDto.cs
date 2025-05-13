using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.UserDTOs;

public record DuplicatedUserDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string GroupId { get; init; } = default!;

    [BsonElement("matchCount")] public int MatchCount { get; init; } = default!;

    [BsonElement("duplicatedUser")] public List<UserDto> Users { get; init; } = default!;
}