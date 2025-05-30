﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ChatService.Contract.Settings;

namespace ChatService.Infrastructure.EventBus.Kafka;

public abstract class KafkaSubscriberBase : BackgroundService
{
    private readonly IConsumer<string, EventEnvelope> _consumer;
    protected readonly ILogger<KafkaSubscriberBase> Logger;
    private readonly string _topicName;
    
    protected KafkaSubscriberBase(ILogger<KafkaSubscriberBase> logger, IOptions<KafkaSetting> kafkaSettings, string topicName, string groupId)
    {
        Logger = logger;
        _topicName = topicName;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.Value.BootstrapServer,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            SaslUsername = kafkaSettings.Value.SaslUsername,
            SaslPassword = kafkaSettings.Value.SaslPassword,
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.Plain,
        };
        _consumer = new ConsumerBuilder<string, EventEnvelope>(consumerConfig)
            .SetValueDeserializer(new EventEnvelopeDeserializer()).Build();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Subscribing to topics [{topic}]...", _topicName);
        
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
                            Logger.LogInformation("No message found in topic [{topic}].", _topicName);
                            await Task.Delay(1000, stoppingToken);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        Logger.LogError(ex, "Consumer error: {ErrorMessage}", ex.Error.Reason);
                    }
                    catch (TaskCanceledException)
                    {
                        Logger.LogInformation("Kafka Background Service Topic [{topic}] has stopped.", _topicName);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error consuming message");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error subscribing to topics");
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