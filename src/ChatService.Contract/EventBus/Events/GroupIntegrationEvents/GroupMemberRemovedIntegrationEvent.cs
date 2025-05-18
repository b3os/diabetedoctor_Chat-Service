namespace ChatService.Contract.EventBus.Events.GroupIntegrationEvents;

public record GroupMemberRemovedIntegrationEvent : IntegrationEvent
{
    public string GroupId { get; init; } = null!;
    public string MemberId { get; init; } = null!;
};