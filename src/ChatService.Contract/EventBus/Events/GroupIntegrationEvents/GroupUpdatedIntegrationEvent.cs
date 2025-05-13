namespace ChatService.Contract.EventBus.Events.GroupIntegrationEvents;

public record GroupUpdatedIntegrationEvent : IntegrationEvent
{
    public string GroupId { get; init; } = default!;
    public string? Name { get; init; }
    public string? Avatar { get; init; }   
}