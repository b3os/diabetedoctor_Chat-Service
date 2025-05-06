namespace ChatService.Contract.EventBus.Events.ChatIntegrationEvents;

public record ChatCreatedIntegrationEvent : IntegrationEvent
{
    public string? Id { get; init; }
    public string? FullName { get; init; }
    public string? GroupName { get; init; }
    public string? Content { get; init; }
}