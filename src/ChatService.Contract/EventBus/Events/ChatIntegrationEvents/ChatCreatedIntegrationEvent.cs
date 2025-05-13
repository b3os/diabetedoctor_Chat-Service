namespace ChatService.Contract.EventBus.Events.ChatIntegrationEvents;

public record ChatCreatedIntegrationEvent : IntegrationEvent
{
    public string? SenderId { get; init; }
    public string? GroupId { get; init; }
    public string? MessageId { get; init; }
    public string? MessageContent { get; init; }
}