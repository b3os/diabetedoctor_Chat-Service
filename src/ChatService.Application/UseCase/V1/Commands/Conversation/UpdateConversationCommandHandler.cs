namespace ChatService.Application.UseCase.V1.Commands.Conversation;

public sealed class UpdateConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IParticipantRepository participantRepository,
    IConversationRepository conversationRepository,
    IOutboxEventRepository outboxEventRepository)
    : ICommandHandler<UpdateConversationCommand, Response>
{
    public async Task<Result<Response>> Handle(UpdateConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await GetConversationWithPermissionAsync(request.GroupId, request.AdminId, cancellationToken);

        if (conversation.IsFailure)
        {
            return Result.Failure<Response>(conversation.Error);
        }

        var avatar = !string.IsNullOrWhiteSpace(request.Avatar) ? Image.Of(request.Avatar) : null;
        var modify = conversation.Value.Modify(request.Name, avatar);

        if (modify.IsFailure && modify is IValidationResult validationResult)
        {
            return ValidationResult<Response>.WithErrors(validationResult.Errors);
        }

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await conversationRepository.UpdateConversationAsync(unitOfWork.ClientSession, request.GroupId,
                request.Name, avatar, cancellationToken);
            var @event = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.ConversationTopic,
                MapToIntegrationEvent(request));
            await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Success(new Response(
            ConversationMessage.UpdatedGroupSuccessfully.GetMessage().Code,
            ConversationMessage.UpdatedGroupSuccessfully.GetMessage().Message));
    }

    private async Task<Result<Domain.Models.Conversation>> GetConversationWithPermissionAsync(ObjectId conversationId,
        string ownerId,
        CancellationToken cancellationToken)
    {
        var conversationProjection = Builders<Domain.Models.Conversation>.Projection
            .Include(conversation => conversation.Avatar)
            .Include(conversation => conversation.Name);
        var conversation = await conversationRepository.FindSingleAsync(
            group => group.Id == conversationId
                     && group.ConversationType == ConversationTypeEnum.Group,
            conversationProjection,
            cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<Domain.Models.Conversation>(ConversationErrors.NotFound);
        }

        var isParticipantOwner = await participantRepository.ExistsAsync(
            participant => participant.UserId.Id == ownerId
                           && participant.ConversationId == conversationId
                           && participant.Role == MemberRoleEnum.Owner,
            cancellationToken);

        return isParticipantOwner is false
            ? Result.Failure<Domain.Models.Conversation>(ConversationErrors.Forbidden)
            : Result.Success(conversation);
    }
    
    private ConversationUpdatedIntegrationEvent MapToIntegrationEvent(UpdateConversationCommand command)
    {
        return new ConversationUpdatedIntegrationEvent()
        {
            ConversationId = command.GroupId.ToString(),
            ConversationName = command.Name,
            Avatar = command.Avatar
        };
    }
}