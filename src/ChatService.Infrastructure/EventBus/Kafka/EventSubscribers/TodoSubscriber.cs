using System;
using System.Threading;
using System.Threading.Tasks;
using ChatService.Contract.EventBus.Events.TodoIntegrationEvents;

namespace ChatService.Infrastructure.EventBus.Kafka.EventSubscribers;

public class TodoSubscriber(
    ILogger<KafkaSubscriberBase<TodoExampleIntegrationEvent>> logger,
    IOptions<KafkaSetting> kafkaSetting,
    IntegrationEventFactory<TodoExampleIntegrationEvent> integrationEventFactory,
    IServiceScopeFactory serviceScopeFactory
)
    : KafkaSubscriberBase<TodoExampleIntegrationEvent>(logger, kafkaSetting, "todo", "todo-group")
{
    protected override async Task ProcessMessageAsync(EventEnvelope messageValue, CancellationToken stoppingToken)
    {
        try
        {
            var @event = integrationEventFactory.CreateEvent(messageValue.EventTypeName, messageValue.Message);
            if (@event is not null)
            {
                using var scope = serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(@event, stoppingToken);
                await Task.CompletedTask;
            }
            else
            {
                logger.LogWarning("Event type not found: {t}", messageValue.EventTypeName);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}