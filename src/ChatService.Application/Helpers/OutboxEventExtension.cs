using System.Text.Json;
using ChatService.Contract.EventBus.Abstractions.Message;

namespace ChatService.Application.Helpers;

public static class OutboxEventExtension
{
    public static OutboxEvent ToOutboxEvent<TEvent>(string topic, TEvent @event) where TEvent : IntegrationEvent
    {
        var id = ObjectId.GenerateNewId();
        var type = typeof(TEvent).Name;
        var json = JsonSerializer.Serialize(@event);
        return OutboxEvent.Create(id,  topic, type, json);
    }
}