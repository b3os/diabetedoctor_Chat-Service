using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.DTOs.ParticipantDtos;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ConversationDtos;


[BsonIgnoreExtraElements]
public record ConversationResponseDto
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonElement("name")]
    public string Name { get; init; } = null!;
    
    [BsonElement("avatar")]
    public string Avatar { get; init; } = null!;
    
    [BsonElement("type")]
    public int ConversationType { get; init; }
    
    [BsonElement("last_message")]
    public MessageDto? Message { get; init; }
    
    [BsonElement("members")]
    public IEnumerable<ParticipantResponseDto> Members { get; init; } = [];
    
    [BsonElement("modified_date")]
    public DateTime ModifiedDate { get; init; }
}