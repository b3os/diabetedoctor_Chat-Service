using ChatService.Contract.Helpers;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Models;
using MongoDB.Driver;

namespace ChatService.Infrastructure.Outbox;

internal sealed class OutboxProcessor(
    IUnitOfWork unitOfWork, 
    IMongoDbContext mongoDbContext, 
    IntegrationEventFactory integrationEventFactory,
    IEventPublisher publisher)
{
    private const int BatchSize = 20;
    
    public async Task Execute(CancellationToken cancellationToken = default)
    {
        await unitOfWork.StartTransactionAsync(cancellationToken);
        
        var outboxMessages = await mongoDbContext.OutboxEvents
            .Find(@event => @event.ProcessedAt == null)
            .Sort(Builders<OutboxEvent>.Sort.Ascending(x => x.CreatedDate))
            .Limit(BatchSize)
            .ToListAsync(cancellationToken);

        if (outboxMessages.Count == 0) return;
        
        foreach (var outboxMessage in outboxMessages)
        {
            var @event = integrationEventFactory.CreateEvent(outboxMessage.EventType, outboxMessage.Message);
            if (@event is null) continue;
            var filter = Builders<OutboxEvent>.Filter.Eq(outboxEvent => outboxEvent.Id, outboxMessage.Id);
            var update = Builders<OutboxEvent>.Update.Set(outboxEvent => outboxEvent.ProcessedAt, CurrentTimeService.GetCurrentTime());
            var option = new UpdateOptions { IsUpsert = false };
            
            try
            {
                await publisher.PublishAsync(outboxMessage.Topic, @event, outboxMessage.RetryCount,cancellationToken);
                await mongoDbContext.OutboxEvents.UpdateOneAsync(unitOfWork.ClientSession, filter, update, option, cancellationToken);
            }
            catch (Exception ex)
            {
                var updateError = Builders<OutboxEvent>.Update.Set(outboxEvent => outboxEvent.ErrorMessage, ex.ToString());
                var updateWithError = Builders<OutboxEvent>.Update.Combine(update, updateError);
                await mongoDbContext.OutboxEvents.UpdateOneAsync(unitOfWork.ClientSession, filter, updateWithError, option, cancellationToken);
            }
        }

        await unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}