namespace ChatService.Contract.Services.Group.DomainEvents;

public record GroupMembersAddedEvent : IDomainEvent
{
    public string GroupId { get; init; } = default!;
    public IEnumerable<string> Members { get; init; } = default!;
}