using ChatService.Infrastructure.EventBus.Ably.EventSubscribers;

namespace ChatService.Infrastructure.EventBus.Ably;

public static class AblyEventBusExtensions
{
    public static void AddAblySubscriber(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<MessageReadSubscriber>();
    }
}