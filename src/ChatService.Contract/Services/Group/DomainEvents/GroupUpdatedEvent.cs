namespace ChatService.Contract.Services.Group.DomainEvents;

public record GroupUpdatedEvent : IDomainEvent
{
    public string GroupId { get; init; } = default!;
    public string? Name { get; init; }
    public string? Avatar { get; init; }   
}