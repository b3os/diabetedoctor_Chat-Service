using ChatService.Contract.DTOs.ParticipantDtos;
using ChatService.Contract.DTOs.ValueObjectDtos;
using ChatService.Contract.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.ConversationDtos;

public record ConversationWithParticipantDto
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonElement("name")]
    public string Name { get; init; } = null!;
    
    [BsonElement("avatar")]
    public ImageDto Avatar { get; init; } = null!;
    
    [BsonElement("type")]
    public ConversationTypeEnum ConversationType { get; init; }
    
    [BsonElement("status")]
    public ConversationStatusEnum Status { get; private set; }
    
    [BsonElement("member")]
    public ParticipantWithUserDto? Member { get; init; }
}