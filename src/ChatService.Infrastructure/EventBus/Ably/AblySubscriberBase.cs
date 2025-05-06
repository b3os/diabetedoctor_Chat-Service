using IO.Ably;
using IO.Ably.Realtime;

namespace ChatService.Infrastructure.EventBus.Ably;

public abstract class AblySubscriberBase : BackgroundService
{
    private readonly IRealtimeChannel _channel;
    protected readonly ILogger<AblySubscriberBase> Logger;
    private readonly string _eventName;

    protected AblySubscriberBase(ILogger<AblySubscriberBase> logger, IOptions<AblySetting> ablySetting,
        string eventName, string channelName)
    {
        Logger = logger;
        _eventName = eventName;

        var realtime = new AblyRealtime(ablySetting.Value.ApiKey);
        _channel = realtime.Channels.Get(channelName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Subscribing to events [{event}]...", _eventName);

        try
        {
            _channel.Subscribe(_eventName, message =>
            {
                try
                {
                    if (message.Data is string json)
                    {
                        var envelope = JsonSerializer.Deserialize<EventEnvelope<object>>(json);
                        
                        if (envelope?.Message != null)
                        {
                            _ = Task.Run(
                                () => ProcessEventAsync(envelope, stoppingToken),
                                stoppingToken);
                        }
                    }
                }
                catch (AblyException ex)
                {
                    Logger.LogError(ex, "Ably error: {AblyErrorMessage}", ex.ErrorInfo.Message);
                }
                catch (JsonException ex)
                {
                    Logger.LogError(ex, "Failed to deserialize message into EventEnvelope. JSON: {Json}", message.Data);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error: {ErrorMessage}", ex.Message);
                }
            });

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            Logger.LogInformation("Ably Background Service Event [{event}] has stopped.", _eventName);
        }
        finally
        {
            Logger.LogInformation("Unsubscribing to events [{event}]...", _eventName);
            _channel.Unsubscribe();
            await _channel.DetachAsync();
        }
    }

    protected abstract Task ProcessEventAsync(EventEnvelope<object> messageValue, CancellationToken stoppingToken);
}