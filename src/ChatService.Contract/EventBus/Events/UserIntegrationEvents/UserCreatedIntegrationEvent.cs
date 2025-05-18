namespace ChatService.Contract.EventBus.Events.UserIntegrationEvents;

public record UserCreatedIntegrationEvent : IntegrationEvent
{
    public string UserId { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Avatar { get; init; } = null!;
}