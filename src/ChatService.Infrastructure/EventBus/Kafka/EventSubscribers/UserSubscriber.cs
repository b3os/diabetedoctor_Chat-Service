using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Events.UserIntegrationEvents;

namespace ChatService.Infrastructure.EventBus.Kafka.EventSubscribers;

public class UserSubscriber : KafkaSubscriberBase<UserCreatedIntegrationEvent>
{
    private readonly IntegrationEventFactory _integrationEventFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public UserSubscriber(ILogger<KafkaSubscriberBase<UserCreatedIntegrationEvent>> logger, IOptions<KafkaSetting> kafkaSetting, IntegrationEventFactory integrationEventFactory, IServiceScopeFactory serviceScopeFactory) : base(logger, kafkaSetting, TopicConstraints.UserTopic, TopicConstraints.ChatServiceUserConsumerGroup)
    {
        _integrationEventFactory = integrationEventFactory;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ProcessMessageAsync(EventEnvelope messageValue, CancellationToken stoppingToken)
    {
        try
        {
            var @event = _integrationEventFactory.CreateEvent(messageValue.EventTypeName, messageValue.Message);
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
            Console.WriteLine(e.Message);
        }
    }
}