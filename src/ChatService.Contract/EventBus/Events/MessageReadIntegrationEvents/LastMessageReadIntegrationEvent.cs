namespace ChatService.Contract.EventBus.Events.MessageReadIntegrationEvents;

public record LastMessageReadIntegrationEvent : IntegrationEvent
{
    public string UserId { get; init; }
    public string GroupId { get; init; }
    public string LastReadMessageId { get; init; }

}