namespace ChatService.Contract.Services.Group.DomainEvents;

public record GroupCreatedEvent : IDomainEvent
{
    public string GroupId { get; init; } = null!;
    public string Name { get; init; } = default!;
    public IEnumerable<string> Members {get; init;} = default!;
}