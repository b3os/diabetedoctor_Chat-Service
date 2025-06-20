namespace ChatService.Contract.EventBus.Events.ConversationIntegrationEvents;

public record ConversationUpdatedIntegrationEvent : IntegrationEvent
{
    public string ConversationId { get; init; } = null!;
    public string? ConversationName { get; init; }
    public string? Avatar { get; init; }   
}