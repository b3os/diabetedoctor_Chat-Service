namespace ChatService.Contract.Services.Message.DomainEvents;

public record ChatCreatedEvent : IDomainEvent
{
    public string? SenderId { get; init; }
    public string? SenderFullName { get; init; }
    public string? SenderAvatar { get; init; }
    public string GroupId { get; init; } = null!;
    public string MessageId { get; init; } = null!;
    public string MessageContent { get; init; } = null!;
    public int Type { get; init; }
    public DateTime? CreatedDate { get; init; }
    public List<string> ReadBy { get; init; } = [];
};