using ChatService.Contract.EventBus.Abstractions.Message;

namespace ChatService.Contract.EventBus.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(string? topic, TEvent @event) where TEvent: IntegrationEvent;
}