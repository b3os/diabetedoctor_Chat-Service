namespace ChatService.Contract.EventBus.Events.GroupIntegrationEvents;

public record GroupMemberRemovedIntegrationEvent : IntegrationEvent
{
    public string GroupId { get; init; } = default!;
    public string MemberId { get; init; } = default!;
};