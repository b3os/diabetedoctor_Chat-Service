namespace ChatService.Contract.EventBus.Abstractions.Message;

public interface IIntegrationEventHandler<in TEvent> : INotificationHandler<TEvent>
where TEvent : IntegrationEvent
{
}