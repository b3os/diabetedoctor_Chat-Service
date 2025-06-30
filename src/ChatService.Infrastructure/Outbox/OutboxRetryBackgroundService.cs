using ChatService.Infrastructure.Options;

namespace ChatService.Infrastructure.Outbox;

internal class OutboxRetryBackgroundService(
    ILogger<OutboxRetryBackgroundService> logger,
    IServiceScopeFactory serviceScopeFactory,
    OutboxOptions outboxOptions)
    : BackgroundService
{
    private const int OutboxProcessorFrequency = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Starting outbox background service");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceScopeFactory.CreateScope();
                var outboxProcessor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                await outboxProcessor.ExecuteRetry(outboxOptions.RetryCount, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(OutboxProcessorFrequency), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("OutboxBackgroundService cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred in OutboxBackgroundService");
        }
        finally
        {
            logger.LogInformation("Outbox background finished ...");
        }
    }
}