using ChatService.Contract.EventBus.Events.UserIntegrationEvents;
using ChatService.Contract.Services.User.Commands;

namespace ChatService.Infrastructure.EventBus.Kafka.EventHandlers;

public sealed class UserIntegrationEventHandler(ISender sender, ILogger<UserIntegrationEventHandler> logger) :
    IIntegrationEventHandler<UserInfoCreatedProfileIntegrationEvent>,
    IIntegrationEventHandler<UserUpdatedIntegrationEvent>
{
    public async Task Handle(UserInfoCreatedProfileIntegrationEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling user created event: {eventId}", notification.EventId);

        if (string.IsNullOrWhiteSpace(notification.UserId))
        {
            logger.LogWarning("UserCreatedIntegrationEvent missing Id. Skipping user creation...");
            return;
        }

        await sender.Send(new CreateUserCommand
        {
            Id = notification.UserId,
            HospitalId = notification.HospitalId,
            FullName = notification.FullName,
            Avatar = notification.Avatar,
            PhoneNumber = notification.PhoneNumber,
            Role = notification.Role
        }, cancellationToken);
    }

    public async Task Handle(UserUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling user updated event: {eventId}", notification.EventId);

        if (string.IsNullOrWhiteSpace(notification.UserId))
        {
            logger.LogWarning("UserUpdatedIntegrationEvent missing Id. Skipping user updating...");
            return;
        }

        await sender.Send(new UpdateUserCommand
        {
            Id = notification.UserId,
            FullName = notification.FullName,
            Avatar = notification.Avatar,
            PhoneNumber = notification.PhoneNumber
        }, cancellationToken);
    }
}