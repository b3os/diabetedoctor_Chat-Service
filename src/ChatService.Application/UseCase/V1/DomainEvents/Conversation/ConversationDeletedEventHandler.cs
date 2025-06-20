
namespace ChatService.Application.UseCase.V1.DomainEvents.Conversation;

public class ConversationDeletedEventHandler(
    IUnitOfWork unitOfWork,
    IMessageRepository messageRepository,
    IParticipantRepository participantRepository)
    : IDomainEventHandler<ConversationDeletedEvent>
{
    public async Task Handle(ConversationDeletedEvent notification, CancellationToken cancellationToken)
    {
        var messageFilter = Builders<Message>.Filter.Eq(message => message.ConversationId, notification.ConversationId);
        var participantFilter = Builders<Participant>.Filter.Eq(participant => participant.ConversationId, notification.ConversationId);

        await messageRepository.DeleteManyAsync(unitOfWork.ClientSession, messageFilter, cancellationToken);
        await participantRepository.DeleteManyAsync(unitOfWork.ClientSession, participantFilter, cancellationToken);
    }
}