using System;
using System.Threading.Tasks;

namespace ChatService.Infrastructure.EventBus.Kafka;

public class KafkaEventPublisher(IProducer<string, EventEnvelope> producer, ILogger logger) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(string? topic, TEvent @event) where TEvent : IntegrationEvent
    {
        var json = JsonSerializer.Serialize(@event);
        logger.LogInformation("Publishing event {type} to topic {topic}: {event}", @event.GetType().Name, topic, json);
        
        try
        {
            await producer.ProduceAsync(topic, new Message<string, EventEnvelope> { Key = @event.EventId.ToString(), 
                Value = new EventEnvelope(typeof(TEvent), json) }
            );

            logger.LogInformation("Published event {@event}", @event.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event {@event}", @event.EventId);
        }
    }
}