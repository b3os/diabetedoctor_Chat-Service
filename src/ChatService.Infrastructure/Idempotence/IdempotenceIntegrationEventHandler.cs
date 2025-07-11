using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.Models;

namespace ChatService.Infrastructure.Idempotence;

public sealed class IdempotenceIntegrationEventHandler<TIntegrationEvent>(
    ILogger<IdempotenceIntegrationEventHandler<TIntegrationEvent>> logger,
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IUnitOfWork unitOfWork,
    IOutBoxEventConsumerRepository consumerRepository)
    : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    public async Task Handle(TIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var consumer = decorated.GetType().Name;
        if (await consumerRepository.HasProcessedEventAsync(notification.EventId.ToString(), consumer, cancellationToken))
        {
            return;
        }

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await decorated.Handle(notification, cancellationToken);
            var eventConsumer = OutboxEventConsumer.Create(notification.EventId.ToString(), consumer);
            await consumerRepository.CreateEventAsync(eventConsumer, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }
    }
}