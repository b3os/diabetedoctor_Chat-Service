namespace ChatService.Contract.EventBus.Events.GroupIntegrationEvents;

public record GroupCreatedIntegrationEvent : IntegrationEvent
{
    public string GroupId { get; init; } = null!;
    public string Name { get; init; } = default!;
    // public string Avatar { get; init; } = default!;
    public IEnumerable<string> Members {get; init;} = default!;
}