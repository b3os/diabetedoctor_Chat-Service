namespace ChatService.Domain.ValueObjects;

public sealed class UserId : ValueObject
{
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; private init; } = null!;

    private UserId(){}

    private UserId(string id)
    {
        Id = id;
    }
    
    public static UserId Of(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id bắt buộc phải có");
        }
        return new UserId(id);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }

    public override string ToString() => Id;
}