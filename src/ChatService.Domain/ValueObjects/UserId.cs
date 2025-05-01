using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.ValueObjects;

public record UserId
{
    [BsonElement("_id")]
    public string Id { get; init; } = default!;

    public static UserId Of(string id)
    {
        return new UserId {Id = id};
    }

    public static List<UserId> All(List<string> ids)
    {
        return ids.Select(Of).ToList();
    }
}