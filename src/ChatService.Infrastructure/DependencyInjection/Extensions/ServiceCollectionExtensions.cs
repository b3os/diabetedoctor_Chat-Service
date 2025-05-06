using ChatService.Contract.Infrastructure.Services;
using ChatService.Infrastructure.EventBus;
using ChatService.Infrastructure.EventBus.Ably;
using ChatService.Infrastructure.EventBus.Kafka;
using ChatService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace ChatService.Infrastructure.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructureService(this IHostApplicationBuilder builder)
    {
        builder.AddKafka();
        builder.AddAbly();
        
        builder.Services.AddSingleton(typeof(IntegrationEventFactory));
        builder.Services.AddScoped<IClaimsService, ClaimsService>();
    }

    private static void AddKafka(this IHostApplicationBuilder builder)
    {
        builder.AddKafkaProducer();
        builder.AddKafkaEventPublisher();
        builder.AddKafkaConsumer();
    }

    private static void AddAbly(this IHostApplicationBuilder builder)
    {
        builder.AddAblySubscriber();
    }
    
}