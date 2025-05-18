using System;
using ChatService.Infrastructure.EventBus.Kafka.EventSubscribers;
using ChatService.Contract.Settings;

namespace ChatService.Infrastructure.EventBus.Kafka;

public static class KafkaEventBusExtensions
{
    
    public static void AddKafkaProducer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IProducer<string, EventEnvelope>>(sp =>
        {
            var kafkaSetting = sp.GetRequiredService<IOptions<KafkaSetting>>();
            
            if (kafkaSetting?.Value is null)
            {
                throw new InvalidOperationException("Kafka configuration is missing.");
            }

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaSetting.Value.BootstrapServer,
                Acks = Acks.All,
                MessageSendMaxRetries = 3,
                CompressionType = CompressionType.Gzip,
                // LingerMs = 0,
                MessageTimeoutMs = 10000, // Maximum time may use to deliver a message (including retries)
                RequestTimeoutMs = 10000, // This value is only enforced by the broker and relies on
                RetryBackoffMs = 1000 // The backoff time in milliseconds before retrying a protocol request
                // Nếu cần SASL:
                // SaslUsername = kafkaSetting.Value.SaslUsername,
                // SaslPassword = kafkaSetting.Value.SaslPassword,
                // SecurityProtocol = SecurityProtocol.SaslPlaintext,
                // SaslMechanism = SaslMechanism.Plain
            };

            return new ProducerBuilder<string, EventEnvelope>(producerConfig)
                .SetValueSerializer(new EventEnvelopeSerializer())
                .Build();
        });
    }
    
    public static void AddKafkaEventPublisher(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IEventPublisher>(services => new KafkaEventPublisher(
            services.GetRequiredService<IProducer<string, EventEnvelope>>(),
            services.GetRequiredService<ILoggerFactory>().CreateLogger("KafkaEventPublisher")
        ));
    }
    
    public static void AddKafkaConsumer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<UserSubscriber>();
    }
    
    private class EventEnvelopeSerializer : ISerializer<EventEnvelope>
    {
        public byte[] Serialize(EventEnvelope data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}