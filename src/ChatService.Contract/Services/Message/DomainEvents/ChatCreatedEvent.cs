namespace ChatService.Contract.Services.Message.DomainEvents;

public record ChatCreatedEvent : IDomainEvent
{
    public string SenderId { get; init; } = default!;
    public string GroupId { get; init; } = default!;
    public string MessageId { get; init; } = default!;
    public string MessageContent { get; init; } = default!;
};