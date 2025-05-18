namespace ChatService.Contract.Services.Group.DomainEvents;

public record GroupMembersAddedEvent : IDomainEvent
{
    public string GroupId { get; init; } = null!;
    public IEnumerable<string> Members { get; init; } = null!;
}