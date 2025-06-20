namespace ChatService.Application.UseCase.V1.Commands.Conversation;

public sealed class RemoveMemberFromGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository,
    IOutboxEventRepository outboxEventRepository)
    : ICommandHandler<RemoveMemberFromGroupCommand, Response>
{
    public async Task<Result<Response>> Handle(RemoveMemberFromGroupCommand request, CancellationToken cancellationToken)
    {
        var permissionResult = await GetParticipantWithPermissionAsync(request.ConversationId, request.AdminId, cancellationToken);
        if (permissionResult.IsFailure)
        {
            return Result.Failure<Response>(permissionResult.Error);
        }
        
        var executorPermissionResult = await EnsureExecutorHasPermission(request.ConversationId, permissionResult.Value, request.MemberId, cancellationToken);
        if (executorPermissionResult.IsFailure)
        {
            return Result.Failure<Response>(executorPermissionResult.Error);
        }
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await conversationRepository.RemoveMemberFromConversationAsync(unitOfWork.ClientSession,
                request.ConversationId, executorPermissionResult.Value.UserId, cancellationToken);
            await participantRepository.DeleteOneAsync(unitOfWork.ClientSession, executorPermissionResult.Value.Id, cancellationToken);
            var @event = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.ConversationTopic, MapToIntegrationEvent(request));
            await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Success(new Response(
            ConversationMessage.RemoveMemberFromGroupSuccessfully.GetMessage().Code,
            ConversationMessage.RemoveMemberFromGroupSuccessfully.GetMessage().Message));
    }
    
    private async Task<Result<Participant>> GetParticipantWithPermissionAsync(ObjectId conversationId, string adminId,
        CancellationToken cancellationToken)
    {
        var isConversationExisted = await conversationRepository.ExistsAsync(
            c => c.Id == conversationId
                 && c.ConversationType == ConversationTypeEnum.Group,
            cancellationToken);

        if (!isConversationExisted)
        {
            return Result.Failure<Participant>(ConversationErrors.NotFound);
        }
        
        var projection = Builders<Participant>.Projection.Include(p => p.Role);
        var participant = await participantRepository.FindSingleAsync(
            p => p.UserId.Id == adminId
                           && p.ConversationId == conversationId
                           && p.Role != MemberRoleEnum.Member,
            projection,
            cancellationToken);
        
        return participant is null
            ? Result.Failure<Participant>(ConversationErrors.Forbidden)
            : Result.Success(participant);
    }
    
    private async Task<Result<Participant>> EnsureExecutorHasPermission(ObjectId groupId, Participant executor, string memberId, CancellationToken cancellationToken = default)
    {
        var member = await participantRepository.FindSingleAsync(
            p => p.UserId.Id == memberId
                            && p.ConversationId == groupId,
            cancellationToken: cancellationToken);

        if (member is null)
        {
            return Result.Failure<Participant>(ConversationErrors.GroupMemberNotExists);
        }
        
        return executor.Role >= member.Role ? Result.Failure<Participant>(ConversationErrors.CannotRemoveMember) : Result.Success(member);
    }
    
    private GroupMemberRemovedIntegrationEvent MapToIntegrationEvent(RemoveMemberFromGroupCommand command)
    {
        return new GroupMemberRemovedIntegrationEvent()
        {
            ConversationId = command.ConversationId.ToString(),
            MemberId = command.MemberId
        };
    }
}