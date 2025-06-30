using System.Text.Json;
using ChatService.Contract.EventBus.Abstractions.Message;

namespace ChatService.Application.Helpers;

public static class OutboxEventExtension
{
    public static OutboxEvent ToOutboxEvent<TEvent>(string topic, TEvent @event, int retryCount = 0) where TEvent : IntegrationEvent
    {
        var id = ObjectId.GenerateNewId();
        var type = typeof(TEvent).Name;
        var json = JsonSerializer.Serialize(@event);
        var retryMinutes = retryCount switch
        {
            1 => 10,
            2 => 30,
            _ => 60
        };
        return OutboxEvent.Create(id,  topic, type, json, retryCount, retryMinutes);
    }
}