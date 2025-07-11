namespace ChatService.Contract.EventBus.Events.HospitalIntegrationEvents;

public record HospitalCreatedIntegrationEvent : IntegrationEvent
{
    public string Id { get; init; } = null!;
    public string HospitalName { get; init; } = null!;
}