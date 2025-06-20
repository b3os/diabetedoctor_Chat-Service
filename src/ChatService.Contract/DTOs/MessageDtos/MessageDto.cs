using ChatService.Contract.DTOs.ParticipantDtos;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.MessageDtos;


[BsonIgnoreExtraElements]
public class MessageDto
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonElement("content")]
    public string Content { get; set; } = null!;
    
    [BsonElement("type")]
    public int Type { get; set; } = 0!;
    
    [BsonElement("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = default!;
    
    [BsonElement("participant_info")]
    public ParticipantDto Participant { get; set; } = null!;
}