using ChatService.Contract.DTOs.ValueObjectDtos;

namespace ChatService.Contract.EventBus.Events.UserIntegrationEvents;

public record UserInfoCreatedProfileIntegrationEvent : IntegrationEvent
{
    public string UserId { get; init; } = null!;
    public string? HospitalId { get; init; }
    public FullNameDto FullName { get; init; } = null!;
    public string Avatar { get; init; } = null!;
    public string? PhoneNumber { get; init; } = null!;
    public int Role { get; init; }
}
