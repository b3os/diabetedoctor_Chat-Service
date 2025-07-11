namespace ChatService.Domain.Abstractions;

public abstract class DomainEntity<TKey>
{
    [BsonId]
    [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
    public TKey Id { get; protected set; } = default!;

    [BsonElement("created_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? CreatedDate { get; protected set; }

    [BsonElement("modified_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ModifiedDate { get; protected set; }

    [BsonElement("is_deleted"), BsonRepresentation(BsonType.Boolean)]
    public bool? IsDeleted { get; protected set; }
    
    // /// <summary>
    // /// True if domain entity has an identity
    // /// </summary>
    // /// <returns></returns>
    //
    // public bool IsTransient()
    // {
    //     return Id.Equals(default(TKey));
    // }
}