namespace ChatService.Application.UseCase.V1.DomainEvents.Conversation;

public sealed class GroupMemberAddedEventHandler(
    IParticipantRepository participantRepository,
    IUnitOfWork unitOfWork)
    : IDomainEventHandler<GroupMembersAddedEvent>
{
    public async Task Handle(GroupMembersAddedEvent notification, CancellationToken cancellationToken)
    {
        var participants = MapToListParticipant(notification.ConversationId, notification.InvitedBy, notification.Users);
        await participantRepository.CreateManyAsync(unitOfWork.ClientSession, participants, cancellationToken);
    }
    
    private IEnumerable<Participant> MapToListParticipant(ObjectId conversationId, UserId invitedBy, List<User> users)
    {
        var participants = users.Select(user =>
            Participant.Create(
                id: ObjectId.GenerateNewId(),
                userId: user.UserId,
                conversationId: conversationId,
                fullName: user.FullName,
                avatar: user.Avatar,
                role: MemberRoleEnum.Member,
                invitedBy: invitedBy)
        );
        return participants;
    }
}