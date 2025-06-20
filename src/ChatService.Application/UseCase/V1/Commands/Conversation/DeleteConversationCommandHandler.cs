namespace ChatService.Application.UseCase.V1.Commands.Conversation;

public sealed class DeleteConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository,
    IOutboxEventRepository outboxEventRepository,
    IPublisher publisher)
    : ICommandHandler<DeleteConversationCommand, Response>
{
    public async Task<Result<Response>> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var checkResult = await CheckConversationPermissionAsync(request.ConversationId, request.OwnerId, cancellationToken);

        if (checkResult.IsFailure)
        {
            return Result.Failure<Response>(checkResult.Error);
        }
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await conversationRepository.DeleteOneAsync(unitOfWork.ClientSession, request.ConversationId, cancellationToken);
            await publisher.Publish(MapToEvent(request.ConversationId), cancellationToken);
            var @event = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.ConversationTopic, MapToIntegrationEvent(request.ConversationId));
            await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }
        
        return Result.Success(new Response(
            ConversationMessage.DeletedGroupSuccessfully.GetMessage().Code, 
            ConversationMessage.DeletedGroupSuccessfully.GetMessage().Message));
    }
    
    private async Task<Result> CheckConversationPermissionAsync(ObjectId conversationId, string ownerId,
        CancellationToken cancellationToken)
    {
        var isConversationExisted = await conversationRepository.ExistsAsync(
            c => c.Id == conversationId
                            && c.ConversationType == ConversationTypeEnum.Group,
            cancellationToken);

        if (!isConversationExisted)
        {
            return Result.Failure(ConversationErrors.NotFound);
        }
        
        var participant = await participantRepository.ExistsAsync(
            p => p.UserId.Id == ownerId 
                           && p.ConversationId == conversationId
                           && p.Role == MemberRoleEnum.Owner,
            cancellationToken);
        
        return participant is false
            ? Result.Failure(ConversationErrors.Forbidden)
            : Result.Success();
    }
    
    private ConversationDeletedEvent MapToEvent(ObjectId conversationId)
    {
        return new ConversationDeletedEvent(ConversationId: conversationId);
    }
    
    private ConversationDeletedIntegrationEvent MapToIntegrationEvent(ObjectId conversationId)
    {
        return new ConversationDeletedIntegrationEvent{ConversationId = conversationId.ToString()};
    }
}