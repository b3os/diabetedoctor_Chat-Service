namespace ChatService.Contract.EventBus.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(string? topic, TEvent @event, int retry, CancellationToken cancellationToken = default) where TEvent: IntegrationEvent;
}