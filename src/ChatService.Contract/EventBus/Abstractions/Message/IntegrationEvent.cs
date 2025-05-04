namespace ChatService.Contract.EventBus.Abstractions.Message;

public record IntegrationEvent : INotification
{
    public Guid EventId { get; private set; } = new UuidV7().Value;
    public DateTime CreationDate { get; private set; } = DateTime.UtcNow;
}