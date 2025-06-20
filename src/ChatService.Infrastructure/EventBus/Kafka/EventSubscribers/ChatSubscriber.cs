using ChatService.Contract.Common.Constraint;

namespace ChatService.Infrastructure.EventBus.Kafka.EventSubscribers;

public class ChatSubscriber : KafkaSubscriberBase
{
    private readonly IntegrationEventFactory _integrationEventFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public ChatSubscriber(ILogger<KafkaSubscriberBase> logger, IOptions<KafkaSettings> kafkaSettings,
        IntegrationEventFactory integrationEventFactory, IServiceScopeFactory serviceScopeFactory) 
        : base(logger, kafkaSettings, KafkaTopicConstraints.ChatTopic, KafkaTopicConstraints.ChatServiceChatConsumerGroup)
    {
        _integrationEventFactory = integrationEventFactory;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ProcessMessageAsync(EventEnvelope messageValue, CancellationToken stoppingToken)
    {
        const int maxRetries = 2;
        var retries = 0;

        var @event = _integrationEventFactory.CreateEvent(messageValue.EventTypeName, messageValue.Message);
        if (@event is null)
        {
            Logger.LogWarning("Event type not found: {t}", messageValue.EventTypeName);
            return;
        }
        
        using var scope = _serviceScopeFactory.CreateScope();
        
        while (retries < maxRetries)
        {
            try
            {
                var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(@event.GetType());
                var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                await ((dynamic)handler).Handle((dynamic)@event, stoppingToken);
                // var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                // await mediator.Publish(@event, stoppingToken);
                return;
            }
            catch (Exception ex)
            {
                retries++;
                Logger.LogError(ex, "An error occurred while processing the message.");

                // if (retries < maxRetries)
                // {
                //     await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retries)), stoppingToken);
                // }
                // else
                // {
                //     Logger.LogError(ex, "Max retries exceeded. Sending to retry topic: {eventType}",
                //         messageValue.EventTypeName);
                //     var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
                //     var outboxEvent = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.RetryTopic, @event);
                //     outboxEvent.IncreaseRetryCount();
                //     await outboxRepo.SaveAsync(outboxEvent, stoppingToken);
                // }
            }
        }
    }
}