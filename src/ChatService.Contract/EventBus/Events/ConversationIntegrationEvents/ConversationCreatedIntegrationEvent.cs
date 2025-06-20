namespace ChatService.Contract.EventBus.Events.ConversationIntegrationEvents;

public record ConversationCreatedIntegrationEvent : IntegrationEvent
{
    public string ConversationId { get; init; } = null!;
    public string ConversationName { get; init; } = null!;
    public string Avatar { get; init; } = null!;
    public IEnumerable<string> Members {get; init;} = null!;
}