using ChatService.Contract.DTOs.MediaDTOs;
using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.DTOs.UserDTOs;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.GroupDtos;


[BsonIgnoreExtraElements]
public record GroupDto
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    [BsonElement("name")]
    public string Name { get; init; } = null!;
    [BsonElement("avatar")]
    public string Avatar { get; init; } = null!;
    [BsonElement("message")]
    public MessageDto Message { get; init; } = null!;
    [BsonElement("members")]
    public IEnumerable<UserDto> Members { get; init; } = [];
}