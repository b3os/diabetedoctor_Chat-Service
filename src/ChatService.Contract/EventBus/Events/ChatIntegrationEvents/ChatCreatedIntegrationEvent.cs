namespace ChatService.Contract.EventBus.Events.ChatIntegrationEvents;

public record ChatCreatedIntegrationEvent : IntegrationEvent
{
    public SenderInfo? Sender { get; init; }
    public string? GroupId { get; init; }
    public string? MessageId { get; init; }
    public string? MessageContent { get; init; }
    public int Type { get; init; }
    public DateTime? CreatedDate { get; init; }
    public List<string> ReadBy { get; init; } = [];
    
}

public record SenderInfo
{
    public string? SenderId { get; init; }
    public string? FullName { get; init; }
    public string? Avatar { get; init; }
}