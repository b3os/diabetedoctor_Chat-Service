namespace ChatService.Contract.EventBus.Events.UserIntegrationEvents;

public class UserCreatedIntegrationEvent : IntegrationEvent
{
    public string Id { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Avatar { get; set; } = default!;
}