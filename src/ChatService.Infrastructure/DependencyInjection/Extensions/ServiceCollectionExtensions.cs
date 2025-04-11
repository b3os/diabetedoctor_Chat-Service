using ChatService.Infrastructure.EventBus;
using ChatService.Infrastructure.EventBus.Kafka;
using ChatService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace ChatService.Infrastructure.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructureService(this IHostApplicationBuilder builder)
    {
        builder.AddConfigurationService();
        // builder.AddKafkaConfiguration();
        // builder.AddKafkaProducer();
        // builder.AddKafkaEventPublisher();
        // builder.AddKafkaConsumer();
        
        builder.Services.AddSingleton(typeof(IntegrationEventFactory<>));

        builder.Services
          .AddSingleton<ICurrentTimeService, CurrentTimeService>();
    }

    private static void AddConfigurationService(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<KafkaSetting>(builder.Configuration.GetSection(KafkaSetting.SectionName));
    }
}