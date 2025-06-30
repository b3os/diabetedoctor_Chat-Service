using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Application.UseCase.V1.Commands.Conversation.GroupConversation;

public sealed class PromoteGroupMemberCommandHandler(
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<PromoteGroupMemberCommand, Response>
{
    public async Task<Result<Response>> Handle(PromoteGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var permissionResult =
            await CheckConversationPermissionAsync(request.GroupId, request.MemberId, cancellationToken);
        if (permissionResult.IsFailure)
        {
            return Result.Failure<Response>(permissionResult.Error);
        }

        var member = await participantRepository.FindSingleAsync(
            participant => participant.UserId.Id == request.MemberId && participant.ConversationId == request.GroupId,
            cancellationToken: cancellationToken);
        if (member is null)
        {
            return Result.Failure<Response>(ConversationErrors.GroupMemberNotExists);
        }

        if (member.Role is MemberRole.Owner)
        {
            return Result.Failure<Response>(ConversationErrors.CannotDemoteOwner);
        }

        var isPromoted = member.Role is MemberRole.Member;
        switch (isPromoted)
        {
            case true:
                member.Promote();
                break;
            default:
                member.Demote();
                break;
        }

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await participantRepository.ReplaceOneAsync(unitOfWork.ClientSession, member, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        var message = isPromoted
            ? ConversationMessage.PromoteMemberToAdminSuccessfully.GetMessage()
            : ConversationMessage.DemoteAdminToMemberSuccessfully.GetMessage();

        return Result.Success(new Response(message.Code, message.Message));
    }

    private async Task<Result> CheckConversationPermissionAsync(ObjectId conversationId, string ownerId,
        CancellationToken cancellationToken)
    {
        var isConversationExisted = await conversationRepository.ExistsAsync(
            c => c.Id == conversationId 
                            && c.ConversationType == ConversationType.Group, 
            cancellationToken);

        if (!isConversationExisted)
        {
            return Result.Failure(ConversationErrors.NotFound);
        }

        var projection = Builders<Participant>.Projection.Include(p => p.Role);
        var participant = await participantRepository.FindSingleAsync(
            p => p.UserId.Id == ownerId
                           && p.ConversationId == conversationId
                           && p.Role == MemberRole.Owner,
            projection,
            cancellationToken);
        
        return participant is null
            ? Result.Failure(ConversationErrors.Forbidden)
            : Result.Success();
    }
}