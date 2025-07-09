namespace ChatService.Domain.ValueObjects;

public sealed class HospitalId : ValueObject
{
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; private init; } = null!;

    private HospitalId() {}

    private HospitalId(string id)
    {
        Id = id;    
    }

    public static HospitalId Of(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id bắt buộc phải có");
        }
        return new HospitalId(id);
    }
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }

    public override string ToString() => Id;
}