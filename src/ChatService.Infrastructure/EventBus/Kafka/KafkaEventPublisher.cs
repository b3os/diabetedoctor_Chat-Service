namespace ChatService.Infrastructure.EventBus.Kafka;

public class KafkaEventPublisher(IProducer<string, EventEnvelope> producer, ILogger<KafkaEventPublisher> logger)
    : IEventPublisher
{
    public async Task PublishAsync<TEvent>(string? topic, TEvent @event, int retry, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
    {
        var type = @event.GetType();
        var json = JsonSerializer.Serialize(@event, type);
        logger.LogInformation("Publishing event {type} to topic {topic}: {event}", type.Name, topic, json);
        
        try
        {
            await producer.ProduceAsync(topic, 
                new Message<string, EventEnvelope> { Key = @event.EventId.ToString(), 
                Value = new EventEnvelope(type, json, retry) }, 
                cancellationToken);

            logger.LogInformation("Published event {@event}", @event.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event {@event}", @event.EventId);
        }
    }
}