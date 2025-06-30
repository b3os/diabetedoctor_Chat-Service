using ChatService.Infrastructure.Options;

namespace ChatService.Infrastructure.Outbox;

public static class OutboxExtension
{
    public static void AddOutboxBackgroundService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<OutboxBackgroundService>();
    }
    
    public static void AddOutboxRetryBackgroundService(this IHostApplicationBuilder builder, Action<OutboxOptions> configure)
    {
        var config = new OutboxOptions();
        configure(config);
        builder.Services.AddHostedService(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<OutboxRetryBackgroundService>>();
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            return new OutboxRetryBackgroundService(logger, scopeFactory, config);
        });
    }
}