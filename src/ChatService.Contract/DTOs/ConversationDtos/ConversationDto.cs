using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.DTOs.ParticipantDtos;
using ChatService.Contract.DTOs.ValueObjectDtos;
using ChatService.Contract.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ConversationDtos;

public class ConversationDto
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonElement("name")]
    public string Name { get; init; } = null!;
    
    [BsonElement("avatar")]
    public ImageDto Avatar { get; init; } = null!;
    
    [BsonElement("type")]
    public ConversationTypeEnum ConversationType { get; init; }
    
    [BsonElement("last_message")]
    public MessageDto? Message { get; init; }
    
    [BsonElement("modified_date")]
    public DateTime ModifiedDate { get; init; }
    
    [BsonElement("members")]
    public IEnumerable<ParticipantDto> Members { get; init; } = [];
}