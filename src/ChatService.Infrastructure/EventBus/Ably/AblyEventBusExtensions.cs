using ChatService.Infrastructure.EventBus.Ably.EventSubscribers;
using IO.Ably;

namespace ChatService.Infrastructure.EventBus.Ably;

public static class AblyEventBusExtensions
{
    public static void AddAblyRealtime(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<AblyRealtime>(sp =>
        {
            var ablySetting = sp.GetRequiredService<IOptions<AblySetting>>();
            
            if (ablySetting?.Value is null)
            {
                throw new InvalidOperationException("Kafka configuration is missing.");
            }

            return new AblyRealtime(ablySetting.Value.ApiKey);
        });
    }
    
    public static void AddAblyEventPublisher(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IAblyEventPublisher>(services => new AblyEventPublisher(
            services.GetRequiredService<AblyRealtime>(),
            services.GetRequiredService<ILoggerFactory>().CreateLogger("AblyEventPublisher")
        ));
    }
    
    public static void AddAblySubscriber(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<MessageReadSubscriber>();
    }
}