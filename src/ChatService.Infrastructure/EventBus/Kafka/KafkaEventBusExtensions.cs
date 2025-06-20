using ChatService.Infrastructure.EventBus.Kafka.EventSubscribers;

namespace ChatService.Infrastructure.EventBus.Kafka;

public static class KafkaEventBusExtensions
{
    
    public static void AddKafkaProducer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IProducer<string, EventEnvelope>>(sp =>
        {
            var kafkaSettings = sp.GetRequiredService<IOptions<KafkaSettings>>();
            
            if (kafkaSettings?.Value is null)
            {
                throw new InvalidOperationException("Kafka configuration is missing.");
            }

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServer,
                Acks = Acks.All,
                MessageSendMaxRetries = 3,
                CompressionType = CompressionType.Gzip,
                // LingerMs = 0,
                MessageTimeoutMs = 10000, // Maximum time may use to deliver a message (including retries)
                RequestTimeoutMs = 10000, // This value is only enforced by the broker and relies on
                RetryBackoffMs = 1000, // The backoff time in milliseconds before retrying a protocol request
                // Nếu cần SASL:
                SaslUsername = kafkaSettings.Value.SaslUsername,
                SaslPassword = kafkaSettings.Value.SaslPassword,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.Plain
            };

            return new ProducerBuilder<string, EventEnvelope>(producerConfig)
                .SetValueSerializer(new EventEnvelopeSerializer())
                .Build();
        });
    }
    
    public static void AddKafkaEventPublisher(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
    }
    
    public static void AddKafkaConsumer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<UserSubscriber>();
        builder.Services.AddHostedService<ChatSubscriber>();
    }
    
    private class EventEnvelopeSerializer : ISerializer<EventEnvelope>
    {
        public byte[] Serialize(EventEnvelope data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}