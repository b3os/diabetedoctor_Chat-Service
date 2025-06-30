namespace ChatService.Infrastructure.Outbox;

internal class OutboxBackgroundService(
    ILogger<OutboxBackgroundService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private const int OutboxProcessorFrequency = 3;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Starting outbox background service");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceScopeFactory.CreateScope();
                var outboxProcessor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                await outboxProcessor.Execute(stoppingToken);
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