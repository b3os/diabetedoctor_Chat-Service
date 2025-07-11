using ChatService.Contract.EventBus.Events.ConversationIntegrationEvents;
using ChatService.Contract.Services.Conversation.Commands.IntegrationCommand;

namespace ChatService.Infrastructure.EventBus.Kafka.EventHandlers;

public sealed class ConversationIntegrationEventHandler(ISender sender, ILogger<ConversationIntegrationEventHandler> logger)
: IIntegrationEventHandler<ConversationUpdatedIntegrationEvent>
{
    public async Task Handle(ConversationUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling conversation updated event: {conversationId}", notification.ConversationId);
        if (notification.OldAvatar is null)
        {
            return;
        }

        await sender.Send(new DeleteOldGroupAvatarCommand
        {
            ImagePublicId = notification.OldAvatar
        }, cancellationToken);
    }
}