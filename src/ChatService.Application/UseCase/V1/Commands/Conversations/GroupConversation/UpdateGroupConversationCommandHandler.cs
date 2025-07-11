using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Application.UseCase.V1.Commands.Conversations.GroupConversation;

public sealed class UpdateGroupConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    IMediaRepository mediaRepository,
    IUserRepository userRepository,
    IParticipantRepository participantRepository,
    IConversationRepository conversationRepository)
    : ICommandHandler<UpdateGroupConversationCommand, Response>
{
    public async Task<Result<Response>> Handle(UpdateGroupConversationCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUserWithPermissionAsync(request.StaffId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure<Response>(user.Error);
        }
        
        var conversation = await GetConversationWithPermissionAsync(request.ConversationId, user.Value, cancellationToken);
        if (conversation.IsFailure)
        {
            return Result.Failure<Response>(conversation.Error);
        }
        
        var modify = conversation.Value.Modify(request.Name);

        if (modify.IsFailure && modify is IValidationResult validationResult)
        {
            return ValidationResult<Response>.WithErrors(validationResult.Errors);
        }
        
        Media? media = null;
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
            await conversationRepository.UpdateConversationAsync(unitOfWork.ClientSession, request.ConversationId!, request.Name, avatar, cancellationToken);
            if (media is not null)
            {
                await mediaRepository.ReplaceOneAsync(unitOfWork.ClientSession, media, cancellationToken);
            }
            var domainEvent = MapToDomainEvent(request.ConversationId!, request.Name, avatar);
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

    private async Task<Result<User>> GetUserWithPermissionAsync(string staffId, CancellationToken cancellationToken)
    {
        var userId = UserId.Of(staffId);
        var user = await userRepository.FindSingleAsync(
            u => u.UserId == userId && u.IsDeleted == false,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.Failure<User>(UserErrors.NotFound);
        }

        return user.HospitalId is not null
            ? Result.Success(user)
            : Result.Failure<User>(HospitalErrors.HospitalNotFound);
    }
    
    private async Task<Result<Conversation>> GetConversationWithPermissionAsync(ObjectId conversationId,
        User staff,
        CancellationToken cancellationToken)
    {
        var conversationProjection = Builders<Conversation>.Projection
            .Include(conversation => conversation.Avatar)
            .Include(conversation => conversation.Name);
        var conversation = await conversationRepository.FindSingleAsync(
            c => c.Id == conversationId 
                 && c.HospitalId == staff.HospitalId 
                 && c.ConversationType == ConversationType.Group,
            conversationProjection,
            cancellationToken);

        return conversation is not null
            ? Result.Success(conversation)
            : Result.Failure<Conversation>(ConversationErrors.NotFound);
    }  
    
    private ConversationUpdatedEvent MapToDomainEvent(ObjectId? conversationId, string? name, Image? oldAvatar)
    {
        return new ConversationUpdatedEvent(conversationId.ToString()!, name, oldAvatar?.ToString());
    }
}