namespace ChatService.Contract.EventBus.Events.ConversationIntegrationEvents;

public record GroupMemberRemovedIntegrationEvent : IntegrationEvent
{
    public string ConversationId { get; init; } = null!;
    public string MemberId { get; init; } = null!;
}