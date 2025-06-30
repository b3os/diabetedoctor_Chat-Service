using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Events.ConversationIntegrationEvents;
using ChatService.Contract.EventBus.Events.MessageIntegrationEvents;
using ChatService.Contract.EventBus.Events.UserIntegrationEvents;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Infrastructure.EventBus.Ably;
using ChatService.Infrastructure.EventBus.Kafka.EventHandlers;
using ChatService.Infrastructure.EventBus.Kafka.EventSubscribers;
using ChatService.Infrastructure.Idempotence;
using ChatService.Infrastructure.Outbox;
using ChatService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace ChatService.Infrastructure.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructureService(this IHostApplicationBuilder builder)
    {
        builder.AddKafka();
        builder.AddAbly();
        // builder.AddOutboxService();
        // builder.Services.AddSingleton(typeof(IntegrationEventFactory));
        builder.Services.AddSingleton<IIntegrationEventFactory, IntegrationEventFactory>();
        builder.Services
            .AddScoped<IClaimsService, ClaimsService>()
            .AddScoped<ICloudinaryService, CloudinaryService>();
        
        builder.Services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        builder.Services.Decorate(typeof(IIntegrationEventHandler<>), typeof(IdempotenceIntegrationEventHandler<>));
    }

    private static void AddKafka(this IHostApplicationBuilder builder)
    {
        var kafkaSettings = builder.Configuration
            .GetSection(KafkaSettings.SectionName)
            .Get<KafkaSettings>() ?? throw new InvalidOperationException("Kafka config missing");
        
        builder.AddKafkaProducer(kafkaSettings);
        builder.AddKafkaEventPublisher();
        
        builder.AddKafkaEventConsumer<UserSubscriber>(kafkaSettings, options =>
        {
            options.KafkaGroupId = kafkaSettings.UserTopicConsumerGroup;
            options.Topic = kafkaSettings.UserTopic;
            options.IntegrationEventFactory = IntegrationEventFactory.Instance;
            options.ServiceName = nameof(ChatService);
            options.AcceptEvent = e => e.IsEvent<UserCreatedIntegrationEvent>();
        }, kafkaSettings.UserConnectionName);
        
        builder.AddKafkaEventConsumer<ChatSubscriber>(kafkaSettings, options =>
        {
            options.KafkaGroupId = kafkaSettings.ChatTopicConsumerGroup;
            options.Topic = kafkaSettings.ChatTopic;
            options.IntegrationEventFactory = IntegrationEventFactory.Instance;
            options.ServiceName = nameof(ChatService);
            options.AcceptEvent = e => e.IsEvent<MessageCreatedIntegrationEvent>();
        }, kafkaSettings.ChatConnectionName);
        
        builder.AddKafkaEventConsumer<RetrySubscriber>(kafkaSettings, options =>
        {
            options.KafkaGroupId = kafkaSettings.RetryTopicConsumerGroup;
            options.Topic = kafkaSettings.RetryTopic;
            options.IntegrationEventFactory = IntegrationEventFactory.Instance;
            options.ServiceName = nameof(ChatService);
        }, kafkaSettings.RetryConnectionName);
    }

    private static void AddAbly(this IHostApplicationBuilder builder)
    {
        builder.AddAblyRealtime();
        builder.AddAblyEventPublisher();
        // builder.AddAblySubscriber();
    }
    
    private static void AddOutboxService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<OutboxProcessor>();
        builder.AddOutboxBackgroundService();
        builder.AddOutboxRetryBackgroundService(opt => opt.RetryCount = 1);
        // builder.AddOutboxRetryBackgroundService(opt => opt.RetryCount = 2);
    }
    

}