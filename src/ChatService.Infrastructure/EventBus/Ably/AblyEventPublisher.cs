using IO.Ably;

namespace ChatService.Infrastructure.EventBus.Ably;

public class AblyEventPublisher(AblyRealtime realtime, ILogger logger) : IAblyEventPublisher
{
    public async Task PublishAsync<TEvent>(string? channelName, string? eventName, TEvent @event) where TEvent : IntegrationEvent
    {
        var json = JsonSerializer.Serialize(@event);
        logger.LogInformation("Publishing event {type} to channel {channel}: {event}", @event.GetType().Name, channelName, json);

        try
        {
            var channel = realtime.Channels.Get(channelName);
            await channel.PublishAsync(eventName, new Message<string, EventEnvelope> { Key = @event.EventId.ToString(), 
                Value = new EventEnvelope(typeof(TEvent), json, 0)}
            );
            
            logger.LogInformation("Published event {@event}", @event.EventId);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event {@event}", @event.EventId);
        }
    }
}