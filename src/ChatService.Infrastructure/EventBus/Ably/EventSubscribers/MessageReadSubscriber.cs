using ChatService.Contract.Common.Constraint;

namespace ChatService.Infrastructure.EventBus.Ably.EventSubscribers;

public class MessageReadSubscriber : AblySubscriberBase
{
    private readonly IntegrationEventFactory _integrationEventFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MessageReadSubscriber(ILogger<AblySubscriberBase> logger, IOptions<AblySetting> ablySetting,
        IntegrationEventFactory integrationEventFactory, IServiceScopeFactory serviceScopeFactory) : base(logger,
        ablySetting, AblyTopicConstraints.MessageReadEvent, AblyTopicConstraints.MessageReadChannel)
    {
        _integrationEventFactory = integrationEventFactory;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ProcessEventAsync(EventEnvelope<object> messageValue, CancellationToken stoppingToken)
    {
        try
        {
            var @event = _integrationEventFactory.CreateEvent(messageValue.EventTypeName, messageValue.Message!.ToString()!);
            if (@event is not null)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(@event, stoppingToken);
            }
            else
            {
                Logger.LogWarning("Event type not found: {t}", messageValue.EventTypeName);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occurred while processing the message.");
        }
    }
}