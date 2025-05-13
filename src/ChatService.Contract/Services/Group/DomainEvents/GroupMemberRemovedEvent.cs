namespace ChatService.Contract.Services.Group.DomainEvents;

public record GroupMemberRemovedEvent : IDomainEvent
{
    public string GroupId { get; init; } = default!;
    public string MemberId { get; init; } = default!;
};