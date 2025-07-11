namespace ChatService.Domain.Models;

public class OutboxEventConsumer
{
    [BsonElement("event_id")]
    public string EventId { get; private init; } = null!;
    
    [BsonElement("name")]
    public string Name { get; private set; } = string.Empty;

    public static OutboxEventConsumer Create(string eventId, string name)
    {
        return new OutboxEventConsumer
        {
            EventId = eventId,
            Name = name
        };
    }
}