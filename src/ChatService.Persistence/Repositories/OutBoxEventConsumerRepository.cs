namespace ChatService.Persistence.Repositories;

public class OutBoxEventConsumerRepository(IMongoDbContext context) : IOutBoxEventConsumerRepository
{
    public async Task<bool> HasProcessedEventAsync(string eventId, string name, CancellationToken cancellationToken = default)
    {
        var builder = Builders<OutboxEventConsumer>.Filter;
        var filter = builder.And(
            builder.Eq(x => x.EventId, eventId),
            builder.Eq(x => x.Name, name)
            );
        return await context.OutboxEventConsumers.Find(filter).AnyAsync(cancellationToken); 
    }

    public async Task CreateEventAsync(OutboxEventConsumer eventConsumer, CancellationToken cancellationToken = default)
    {
        await context.OutboxEventConsumers.InsertOneAsync(eventConsumer, cancellationToken: cancellationToken);
    }
}