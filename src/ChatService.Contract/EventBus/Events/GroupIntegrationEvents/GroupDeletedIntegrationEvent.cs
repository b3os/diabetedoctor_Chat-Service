namespace ChatService.Contract.EventBus.Events.GroupIntegrationEvents;

public record GroupDeletedIntegrationEvent : IntegrationEvent
{
    public string GroupId { get; init; } = null!;
};