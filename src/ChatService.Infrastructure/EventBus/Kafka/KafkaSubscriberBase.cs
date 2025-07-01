using ChatService.Application.Helpers;
using ChatService.Contract.Common.Constraint;
using ChatService.Contract.Exceptions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Infrastructure.Options;

namespace ChatService.Infrastructure.EventBus.Kafka;

public class KafkaSubscriberBase<T> : BackgroundService
{
    private readonly IConsumer<string, EventEnvelope> _consumer;
    private readonly ConsumerWorkerOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IIntegrationEventFactory _integrationEventFactory;
    private readonly ILogger<KafkaSubscriberBase<T>> _logger;
    
    public KafkaSubscriberBase(
        IConsumer<string, EventEnvelope> consumer,
        ConsumerWorkerOptions options,
        ILogger<KafkaSubscriberBase<T>> logger, 
        IServiceScopeFactory serviceScopeFactory, 
        IIntegrationEventFactory integrationEventFactory)
    {
        _consumer = consumer;
        _options = options;
        _serviceScopeFactory = serviceScopeFactory;
        _integrationEventFactory = integrationEventFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscribing to topics [{topic}]...", _options.Topic);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _consumer.Subscribe(_options.Topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(100);

                        if (consumeResult is null)
                        {
                            _logger.LogInformation("No message found in topic [{topic}].", _options.Topic);
                            await Task.Delay(1000, stoppingToken);
                            continue;
                        }
                        await ProcessMessageAsync(consumeResult.Message.Value, stoppingToken);
                        _consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Consumer error: {ErrorMessage}", ex.Error.Reason);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogInformation("Kafka Background Service Topic [{topic}] has stopped.", _options.Topic);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error consuming message");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to topics");
            }
            
            await Task.Delay(3000, stoppingToken);
        }
        _consumer.Unsubscribe();
        _consumer.Close();
    }

    private async Task ProcessMessageAsync(EventEnvelope messageValue, CancellationToken stoppingToken)
    {
        var retries = 0;
        var @event = _integrationEventFactory.CreateEvent(messageValue.EventTypeName, messageValue.Message);
        if (@event is not null)
        {
            if (_options.AcceptEvent(@event))
            {
                while (retries <= _options.ServiceRetries)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    try
                    {
                        _logger.LogInformation("Processing message {t}: {message}", messageValue.EventTypeName, messageValue.Message);
                        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(@event.GetType());
                        var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                        await ((dynamic)handler).Handle((dynamic)@event, stoppingToken);
                        return;
                    }
                    catch (DomainException ex)
                    {
                        var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
                        var dlqEvent = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.DeadTopic, @event);
                        await outboxRepo.SaveAsync(dlqEvent, stoppingToken);
                        _logger.LogWarning("Moved to DLQ: {msg}", ex.Message);
                        return;
                    }
                    catch (Exception ex)
                    {
                        retries++;

                        if (retries > _options.ServiceRetries)
                        {
                            var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();

                            if (messageValue.RetryCount > _options.MaxRetries)
                            {
                                var dlqEvent = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.DeadTopic, @event, messageValue.RetryCount);
                                await outboxRepo.SaveAsync(dlqEvent, stoppingToken);
                                _logger.LogWarning("Moved to DLQ: {msg}", ex.Message);
                                return;
                            }
                            var retryEvent = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.RetryTopic, @event, messageValue.RetryCount + 1);
                            await outboxRepo.SaveAsync(retryEvent, stoppingToken);
                            _logger.LogWarning("Moved to Retry topic");
                            return;
                        }
                        _logger.LogError(ex, "Error retry {retries}/{max}", retries, _options.MaxRetries);
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retries)), stoppingToken);
                    }
                }
            }
            else
            {
                _logger.LogDebug("Event skipped: {t}", messageValue.EventTypeName);
            }
        }
    }
}