namespace ChatService.Domain.Abstractions.Repositories;

public interface IOutBoxEventConsumerRepository
{
    Task<bool> HasProcessedEventAsync(string eventId, string name, CancellationToken cancellationToken = default);
    Task CreateEventAsync(OutboxEventConsumer eventConsumer, CancellationToken cancellationToken = default);
}