namespace ChatService.Contract.EventBus.Events.UserIntegrationEvents;

public record UserUpdatedIntegrationEvent : IntegrationEvent
{
    public string UserId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Avatar { get; set; } = null!;
}