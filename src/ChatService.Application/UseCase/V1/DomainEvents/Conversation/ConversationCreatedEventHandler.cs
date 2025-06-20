namespace ChatService.Application.UseCase.V1.DomainEvents.Conversation;

public class ConversationCreatedEventHandler(
    IUnitOfWork unitOfWork,
    IParticipantRepository participantRepository)
    : IDomainEventHandler<ConversationCreatedEvent>
{
    public async Task Handle(ConversationCreatedEvent notification, CancellationToken cancellationToken)
    {
        var participants = MapToConversationParticipants(notification.ConversationId, notification.OwnerId, notification.Users);
        await participantRepository.CreateManyAsync(unitOfWork.ClientSession, participants, cancellationToken);
    }

    private IEnumerable<Participant> MapToConversationParticipants(ObjectId conversationId, UserId ownerId, List<User> users)
    {
        var participants = users.Select(user =>
            Participant.Create(
                id: ObjectId.GenerateNewId(),
                userId: user.UserId,
                conversationId: conversationId,
                fullName: user.FullName,
                avatar: user.Avatar,
                role: user.UserId == ownerId ? MemberRoleEnum.Owner : MemberRoleEnum.Member,
                invitedBy: ownerId)
        );
        return participants;
    }
}