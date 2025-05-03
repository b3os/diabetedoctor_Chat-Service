using ChatService.Contract.EventBus.Events.UserIntegrationEvents;
using ChatService.Contract.Services.User;
using ChatService.Contract.Services.User.Commands;

namespace ChatService.Infrastructure.EventBus.Kafka.EventHandlers;

public class UserIntegrationEventHandler(ISender sender, ILogger<UserIntegrationEventHandler> logger) :
    IIntegrationEventHandler<UserCreatedIntegrationEvent>,
    IIntegrationEventHandler<UserUpdatedIntegrationEvent>
{
    public async Task Handle(UserCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling user created event: {userId}", notification.Id);
        await sender.Send(new CreateUserCommand {Id = notification.Id, FullName = notification.FullName, Avatar = notification.Avatar}, cancellationToken);
    }

    public async Task Handle(UserUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling user updated event: {userId}", notification.Id);
        await sender.Send(new UpdateUserCommand {Id = notification.Id, FullName = notification.FullName, Avatar = notification.Avatar}, cancellationToken);
    }
}