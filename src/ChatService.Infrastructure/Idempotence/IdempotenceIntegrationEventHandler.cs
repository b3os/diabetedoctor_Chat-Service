using ChatService.Application.Helpers;
using ChatService.Contract.Common.Constraint;
using ChatService.Contract.Exceptions;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.Models;

namespace ChatService.Infrastructure.Idempotence;

public sealed class IdempotenceIntegrationEventHandler<TIntegrationEvent>(
    ILogger<IdempotenceIntegrationEventHandler<TIntegrationEvent>> logger,
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IUnitOfWork unitOfWork,
    IOutboxEventRepository eventRepository,
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
        catch (DomainException ex)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            var dlqEvent = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.DeadTopic, notification);
            await eventRepository.SaveAsync(dlqEvent, cancellationToken);
            logger.LogWarning(
                "A error occurred while processing event. The event has been moved to the Dead Letter Queue. Reason: {Reason}", 
                ex.Message);
        }
    }
}