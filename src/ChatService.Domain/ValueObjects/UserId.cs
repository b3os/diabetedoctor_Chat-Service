using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.ValueObjects;

public record UserId
{
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; } = default!;

    public static UserId Of(string id)
    {
        return new UserId {Id = id};
    }

    public static List<UserId> All(IEnumerable<string> ids)
    {
        return ids.Select(Of).ToList();
    }
}