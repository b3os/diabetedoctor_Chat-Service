using ChatService.Contract.DTOs.UserDTOs;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.MessageDtos;


[BsonIgnoreExtraElements]
public class MessageDto
{
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;
    
    [BsonElement("content")]
    public string Content { get; set; } = default!;
    
    [BsonElement("type")]
    public int Type { get; set; } = default!;
    
    [BsonElement("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = default!;
    
    [BsonElement("is_read")]
    public bool IsRead { get; set; }
    
    [BsonElement("user")]
    public UserDto User { get; set; } = default!;
}