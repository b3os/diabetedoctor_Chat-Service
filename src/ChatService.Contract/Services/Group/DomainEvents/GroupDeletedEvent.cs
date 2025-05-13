namespace ChatService.Contract.Services.Group.DomainEvents;

public record GroupDeletedEvent : IDomainEvent
{
    public string GroupId { get; init; } = default!;
};