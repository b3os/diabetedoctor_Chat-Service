using MongoDB.Bson;

namespace ChatService.Contract.Services.MessageReadStatus.DomainEvents;

public record MessageReadStatusUpsertEvent : IDomainEvent
{
    public ObjectId MessageId { get; init; }
    public ObjectId GroupId { get; init; }
    public string UserId { get; init; } = default!;
}