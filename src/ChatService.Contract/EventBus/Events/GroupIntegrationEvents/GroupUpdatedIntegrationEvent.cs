namespace ChatService.Contract.EventBus.Events.GroupIntegrationEvents;

public record GroupUpdatedIntegrationEvent : IntegrationEvent
{
    public string GroupId { get; init; } = null!;
    public string? Name { get; init; }
    public string? Avatar { get; init; }   
}