namespace ChatService.Contract.EventBus.Abstractions.Message;

public class EventEnvelope
{
    public string EventTypeName { get; init; } = default!;
    public string Message { get; init; } = default!;

    public EventEnvelope() {}
    
    public EventEnvelope(Type type, string eventMessage): this(type.Name!, eventMessage) {}

    private EventEnvelope(string eventTypeName, string eventMessage)
    {
        EventTypeName = eventTypeName ?? throw new ArgumentNullException(nameof(eventTypeName));
        Message = eventMessage ?? throw new ArgumentNullException(nameof(eventMessage));
    }
}

public class EventEnvelope<T>
{
    public string EventTypeName { get; init; } = default!;
    public T? Message { get; init; } = default!;

    public EventEnvelope() {}
    
    public EventEnvelope(Type type, T eventMessage): this(type.Name!, eventMessage) {}

    private EventEnvelope(string eventTypeName, T eventMessage)
    {
        EventTypeName = eventTypeName ?? throw new ArgumentNullException(nameof(eventTypeName));
        Message = eventMessage ?? throw new ArgumentNullException(nameof(eventMessage));
    }
}