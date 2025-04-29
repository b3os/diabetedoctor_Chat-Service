using ChatService.Application.Infrastructure.Abstractions;
using ChatService.Infrastructure.EventBus;
using ChatService.Infrastructure.EventBus.Kafka;
using ChatService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace ChatService.Infrastructure.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructureService(this IHostApplicationBuilder builder)
    {
        // builder.AddKafkaConfiguration();
        // builder.AddKafkaProducer();
        // builder.AddKafkaEventPublisher();
        // builder.AddKafkaConsumer();
        
        builder.Services.AddSingleton(typeof(IntegrationEventFactory));
        builder.Services.AddScoped<IClaimsService, ClaimsService>();
    }
    
}