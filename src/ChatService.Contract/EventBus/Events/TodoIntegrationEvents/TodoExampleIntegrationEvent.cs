using ChatService.Contract.EventBus.Abstractions.Message;

namespace ChatService.Contract.EventBus.Events.TodoIntegrationEvents;

public class TodoExampleIntegrationEvent : IntegrationEvent
{
    public Guid Id { get; set; }
    public string Example { get; set; } = default!;
}