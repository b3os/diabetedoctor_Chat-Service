namespace ChatService.Contract.EventBus.Events.ChatIntegrationEvents;

public class ChatCreatedIntegrationEvent : IntegrationEvent
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public string? GroupName { get; set; }
    public string? Content { get; set; }
}