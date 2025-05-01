using ChatService.Contract.DTOs.MediaDTOs;
using ChatService.Contract.DTOs.MessageDtos;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.GroupDtos;


[BsonIgnoreExtraElements]
public record GroupDto
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;
    [BsonElement("name")]
    public string Name { get; init; } = default!;
    [BsonElement("avatar")]
    public string Avatar { get; init; } = default!;
    [BsonElement("message")]
    public MessageDto Message { get; init; } = default!;
}