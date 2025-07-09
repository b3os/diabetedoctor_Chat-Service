using ChatService.Contract.DTOs.ValueObjectDtos;

namespace ChatService.Contract.EventBus.Events.UserIntegrationEvents;

public record UserUpdatedIntegrationEvent : IntegrationEvent
{
    public string? UserId { get; init; }
    public FullNameDto? FullName { get; init; }
    public string? Avatar { get; init; }
    public string? PhoneNumber { get; init; }
}