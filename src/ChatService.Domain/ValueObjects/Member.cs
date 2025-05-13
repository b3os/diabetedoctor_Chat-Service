using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.ValueObjects;

// public record Member
// {
//     [BsonElement("user_id")]
//     public UserId UserId { get; init; } = default!;
//     
//     [BsonElement("role")]
//     public     
//     
//     public static Member Of(string? publicUrl)
//     {
//         return new Member {PublicUrl = publicUrl ?? string.Empty};
//     }
// }