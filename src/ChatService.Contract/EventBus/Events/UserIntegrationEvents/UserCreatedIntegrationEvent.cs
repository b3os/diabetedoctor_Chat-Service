namespace ChatService.Contract.EventBus.Events.UserIntegrationEvents;

public record UserCreatedIntegrationEvent : IntegrationEvent
{
    public string UserId { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Avatar { get; init; } = default!;
}