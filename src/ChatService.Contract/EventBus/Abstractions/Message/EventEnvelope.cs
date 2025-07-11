namespace ChatService.Contract.EventBus.Abstractions.Message;

public class EventEnvelope
{
    public string EventTypeName { get; init; } = null!;
    public string Message { get; init; } = null!;
    public int RetryCount { get; init; }

    public EventEnvelope() {}
    
    public EventEnvelope(Type type, string eventMessage, int retryCount): this(type.Name!, eventMessage, retryCount) {}

    private EventEnvelope(string eventTypeName, string eventMessage, int retryCount)
    {
        EventTypeName = eventTypeName ?? throw new ArgumentNullException(nameof(eventTypeName));
        Message = eventMessage ?? throw new ArgumentNullException(nameof(eventMessage));
        RetryCount = retryCount;
    }
}

public class EventEnvelope<T>
{
    public string EventTypeName { get; init; } = null!;
    public T? Message { get; init; }

    public EventEnvelope() {}
    
    public EventEnvelope(Type type, T eventMessage): this(type.Name!, eventMessage) {}

    private EventEnvelope(string eventTypeName, T eventMessage)
    {
        EventTypeName = eventTypeName ?? throw new ArgumentNullException(nameof(eventTypeName));
        Message = eventMessage ?? throw new ArgumentNullException(nameof(eventMessage));
    }
}