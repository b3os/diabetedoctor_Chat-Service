namespace ChatService.Contract.EventBus.Events.GroupIntegrationEvents;

public record GroupMembersAddedIntegrationEvent : IntegrationEvent
{
    public string GroupId { get; init; } = null!;
    public IEnumerable<string> Members { get; init; } = null!;
}