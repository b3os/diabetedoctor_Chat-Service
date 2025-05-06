using ChatService.Contract.EventBus.Events.MessageReadIntegrationEvents;
using ChatService.Contract.Services.MessageReadStatus.Commands;
using MongoDB.Bson;

namespace ChatService.Infrastructure.EventBus.Ably.EventHandlers;

public class MessageReadIntegrationEventHandler(ISender sender, ILogger<MessageReadIntegrationEventHandler> logger) : IIntegrationEventHandler<LastMessageReadIntegrationEvent>
{
    public async Task Handle(LastMessageReadIntegrationEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling last read message event: {messageId}", notification.LastReadMessageId);
        if (string.IsNullOrWhiteSpace(notification.UserId) ||
            !ObjectId.TryParse(notification.LastReadMessageId, out var messageId) ||
            !ObjectId.TryParse(notification.GroupId, out var groupId))
        {
            logger.LogWarning("LastMessageReadIntegrationEvent missing info. Skipping message status updating...");
            return;
        }
        await sender.Send(new UpsertMessageReadStatusCommand(){GroupId = groupId, MessageId = messageId, UserId = notification.UserId}, cancellationToken);
    }
}