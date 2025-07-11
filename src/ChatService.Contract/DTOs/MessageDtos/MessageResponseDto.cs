using ChatService.Contract.DTOs.ParticipantDtos;
using ChatService.Contract.DTOs.ValueObjectDtos;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.MessageDtos;

[BsonIgnoreExtraElements]
public class MessageResponseDto
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonElement("content")]
    public string Content { get; set; } = null!;
    
    [BsonElement("type")]
    public int Type { get; set; }
    
    [BsonElement("file_attachment")]
    public FileAttachmentResponseDto? FileAttachment { get; set; }
    
    [BsonElement("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = default!;
    
    [BsonElement("participant_info")]
    public ParticipantResponseDto Participant { get; set; } = null!;
}