using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Application.UseCase.V1.Commands.Conversations.GroupConversation;

public sealed class RemoveGroupMemberCommandHandler(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository)
    : ICommandHandler<RemoveGroupMemberCommand, Response>
{
    public async Task<Result<Response>> Handle(RemoveGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var permissionResult = await GetParticipantWithPermissionAsync(request.ConversationId, request.AdminId, cancellationToken);
        if (permissionResult.IsFailure)
        {
            return Result.Failure<Response>(permissionResult.Error);
        }
        
        var memberCanBeRemovedResult = await EnsureMemberCanBeRemoved(request.ConversationId, permissionResult.Value, request.MemberId, cancellationToken);
        if (memberCanBeRemovedResult.IsFailure)
        {
            return Result.Failure<Response>(memberCanBeRemovedResult.Error);
        }
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await conversationRepository.RemoveMemberFromConversationAsync(unitOfWork.ClientSession,
                request.ConversationId, memberCanBeRemovedResult.Value.UserId, cancellationToken);
            await participantRepository.SoftDeleteAsync(unitOfWork.ClientSession, memberCanBeRemovedResult.Value.Id, cancellationToken);
            var domainEvent = MapToDomainEvent(request);
            await publisher.Publish(domainEvent, cancellationToken);
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
                 && c.ConversationType == ConversationType.Group,
            cancellationToken);

        if (!isConversationExisted)
        {
            return Result.Failure<Participant>(ConversationErrors.NotFound);
        }
        
        var projection = Builders<Participant>.Projection.Include(p => p.Role);
        var participant = await participantRepository.FindSingleAsync(
            p => p.UserId.Id == adminId
                           && p.ConversationId == conversationId
                           && p.Role != MemberRole.Member,
            projection,
            cancellationToken);
        
        return participant is null
            ? Result.Failure<Participant>(ConversationErrors.Forbidden)
            : Result.Success(participant);
    }
    
    private async Task<Result<Participant>> EnsureMemberCanBeRemoved(ObjectId groupId, Participant executor, string memberId, CancellationToken cancellationToken = default)
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
    
    private GroupMemberRemovedEvent MapToDomainEvent(RemoveGroupMemberCommand command)
    {
        return new GroupMemberRemovedEvent(command.ConversationId.ToString(), command.MemberId);
    }
}