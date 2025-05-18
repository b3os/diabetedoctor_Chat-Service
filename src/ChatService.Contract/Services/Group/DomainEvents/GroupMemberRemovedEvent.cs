namespace ChatService.Contract.Services.Group.DomainEvents;

public record GroupMemberRemovedEvent : IDomainEvent
{
    public string GroupId { get; init; } = null!;
    public string MemberId { get; init; } = null!;
};