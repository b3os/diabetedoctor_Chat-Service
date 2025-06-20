namespace ChatService.Contract.EventBus.Events.ConversationIntegrationEvents;

public record GroupMembersAddedIntegrationEvent : IntegrationEvent
{
    public string ConversationId { get; init; } = null!;
    public IEnumerable<string> Members { get; init; } = null!;
}