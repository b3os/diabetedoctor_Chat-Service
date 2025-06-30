using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Application.UseCase.V1.Commands.Conversation.GroupConversation;

public sealed class UpdateGroupConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    IMediaRepository mediaRepository,
    IParticipantRepository participantRepository,
    IConversationRepository conversationRepository,
    IOutboxEventRepository outboxEventRepository)
    : ICommandHandler<UpdateGroupConversationCommand, Response>
{
    public async Task<Result<Response>> Handle(UpdateGroupConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await GetConversationWithPermissionAsync(request.ConversationId, request.AdminId, cancellationToken);

        if (conversation.IsFailure)
        {
            return Result.Failure<Response>(conversation.Error);
        }
        
        var modify = conversation.Value.Modify(request.Name);

        if (modify.IsFailure && modify is IValidationResult validationResult)
        {
            return ValidationResult<Response>.WithErrors(validationResult.Errors);
        }
        
        Domain.Models.Media? media = null;
        Image? avatar = null;
        if (request.AvatarId is not null)
        {
            var mediaId = ObjectId.Parse(request.AvatarId);
            media = await mediaRepository.FindByIdAsync(mediaId, cancellationToken);
            if (media is null)
            {
                return Result.Failure<Response>(MediaErrors.MediaNotFound);
            }
            avatar = Image.Of(media.PublicId, media.PublicUrl);
            media.Use();
        }
        
        try
        {
            await unitOfWork.StartTransactionAsync(cancellationToken);
            await conversationRepository.UpdateConversationAsync(unitOfWork.ClientSession, request.ConversationId, request.Name, avatar, cancellationToken);
            if (media is not null)
            {
                await mediaRepository.ReplaceOneAsync(unitOfWork.ClientSession, media, cancellationToken);
            }
            var domainEvent = MapToDomainEvent(request.ConversationId, request.Name, avatar);
            await publisher.Publish(domainEvent, cancellationToken);
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
                     && group.ConversationType == ConversationType.Group,
            conversationProjection,
            cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<Domain.Models.Conversation>(ConversationErrors.NotFound);
        }

        var isParticipantOwner = await participantRepository.ExistsAsync(
            participant => participant.UserId.Id == ownerId
                           && participant.ConversationId == conversationId
                           && participant.Role == MemberRole.Owner,
            cancellationToken);

        return isParticipantOwner is false
            ? Result.Failure<Domain.Models.Conversation>(ConversationErrors.Forbidden)
            : Result.Success(conversation);
    }  
    
    private ConversationUpdatedEvent MapToDomainEvent(ObjectId conversationId, string? name, Image? oldAvatar)
    {
        return new ConversationUpdatedEvent(conversationId.ToString(), name, oldAvatar?.ToString());
    }
}