using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.ValueObjects;

public record Image()
{
    [BsonElement("public_url")]
    public string PublicUrl { get; init; } = default!;
    public static Image Of(string? publicUrl)
    {
        return new Image {PublicUrl = publicUrl ?? string.Empty};
    }
}