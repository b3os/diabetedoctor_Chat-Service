namespace ChatService.Contract.Services.Group.DomainEvents;

public record GroupUpdatedEvent : IDomainEvent
{
    public string GroupId { get; init; } = null!;
    public string? Name { get; init; }
    public string? Avatar { get; init; }   
}