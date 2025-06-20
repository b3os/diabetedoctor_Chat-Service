namespace ChatService.Persistence.Repositories;

public class OutboxEventRepository(IMongoDbContext context) : RepositoryBase<OutboxEvent>(context), IOutboxEventRepository
{
    public async Task SaveAsync(OutboxEvent @event, CancellationToken cancellationToken = default)
    {
        await DbSet.InsertOneAsync(@event, cancellationToken: cancellationToken);
    }
}