using ChatService.Contract.Infrastructure.Services;
using ChatService.Infrastructure.EventBus.Ably;
using ChatService.Infrastructure.Idempotence;
using ChatService.Infrastructure.Outbox;
using ChatService.Infrastructure.Services;

namespace ChatService.Infrastructure.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructureService(this IHostApplicationBuilder builder)
    {
        builder.AddKafka();
        builder.AddAbly();
        builder.AddOutboxBackgroundService();
        builder.Services.AddSingleton(typeof(IntegrationEventFactory));
        builder.Services.AddScoped<IClaimsService, ClaimsService>();
        
        builder.Services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        builder.Services.Decorate(typeof(IIntegrationEventHandler<>), typeof(IdempotenceIntegrationEventHandler<>));
    }

    private static void AddKafka(this IHostApplicationBuilder builder)
    {
        builder.AddKafkaProducer();
        builder.AddKafkaEventPublisher();
        builder.AddKafkaConsumer();
    }

    private static void AddAbly(this IHostApplicationBuilder builder)
    {
        builder.AddAblyRealtime();
        builder.AddAblyEventPublisher();
        // builder.AddAblySubscriber();
    }
    
    private static void AddOutboxBackgroundService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<OutboxProcessor>();
        builder.Services.AddHostedService<OutboxBackgroundService>();
    }
    

}