using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ChatService.Contract.Settings;

namespace ChatService.Infrastructure.EventBus.Kafka;

public abstract class KafkaSubscriberBase<T> : BackgroundService
{
    private readonly IConsumer<string, EventEnvelope> _consumer;
    private readonly ILogger<KafkaSubscriberBase<T>> _logger;
    private readonly string _topicName;
    
    protected KafkaSubscriberBase(ILogger<KafkaSubscriberBase<T>> logger, IOptions<KafkaSetting> kafkaSetting, string topicName, string groupId)
    {
        _logger = logger;
        _topicName = topicName;
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaSetting.Value.BootstrapServer,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            //SaslUsername = configuration["Kafka:SaslUsername"],
            //SaslPassword = configuration["Kafka:SaslPassword"],
            // SecurityProtocol = SecurityProtocol.SaslSsl,
            //SaslMechanism = SaslMechanism.Plain,
        };
        _consumer = new ConsumerBuilder<string, EventEnvelope>(consumerConfig)
            .SetValueDeserializer(new EventEnvelopeDeserializer()).Build();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscribing to topics [{topic}]...", _topicName);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _consumer.Subscribe(_topicName);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(100);

                        if (consumeResult != null)
                        {
                            _ = Task.Run(
                                () => ProcessMessageAsync(consumeResult.Message.Value, stoppingToken),
                                stoppingToken);
                        }
                        else
                        {
                            _logger.LogInformation("No message found in topic [{topic}].", _topicName);
                            await Task.Delay(1000, stoppingToken);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Consumer error: {ErrorMessage}", ex.Error.Reason);
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
    
    protected abstract Task ProcessMessageAsync(EventEnvelope messageValue, CancellationToken stoppingToken);

    private class EventEnvelopeDeserializer : IDeserializer<EventEnvelope>
    {
        public EventEnvelope Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonSerializer.Deserialize<EventEnvelope>(data) ?? throw new Exception("Error deserialize data");
        }
    }
}