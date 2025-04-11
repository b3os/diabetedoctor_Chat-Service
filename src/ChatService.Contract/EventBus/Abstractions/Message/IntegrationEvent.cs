namespace ChatService.Contract.EventBus.Abstractions.Message;

public class IntegrationEvent : INotification
{
    public Guid EventId { get; private set; } = new UuidV7().Value;
    public DateTime CreationDate { get; private set; } = DateTime.UtcNow;
}