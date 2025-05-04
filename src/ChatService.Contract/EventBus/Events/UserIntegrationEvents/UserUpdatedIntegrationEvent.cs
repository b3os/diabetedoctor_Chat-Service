namespace ChatService.Contract.EventBus.Events.UserIntegrationEvents;

public record UserUpdatedIntegrationEvent : IntegrationEvent
{
    public string UserId { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Avatar { get; set; } = default!;
}