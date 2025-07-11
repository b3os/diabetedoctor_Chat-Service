using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Application.UseCase.V1.Commands.Conversations.GroupConversation;

public sealed class DeleteGroupConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository,
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    IPublisher publisher)
    : ICommandHandler<DeleteGroupConversationCommand, Response>
{
    public async Task<Result<Response>> Handle(DeleteGroupConversationCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUserWithPermissionAsync(request.StaffId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure<Response>(user.Error);
        }
        
        var checkResult = await CheckConversationPermissionAsync(request.ConversationId, user.Value, cancellationToken);
        if (checkResult.IsFailure)
        {
            return Result.Failure<Response>(checkResult.Error);
        }
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            var filter = Builders<Conversation>.Filter.Eq(c => c.Id, request.ConversationId);
            await conversationRepository.DeleteOneAsync(unitOfWork.ClientSession, filter, cancellationToken);
            
            var participantFilter = Builders<Participant>.Filter.Eq(participant => participant.ConversationId, request.ConversationId);
            await participantRepository.DeleteManyAsync(unitOfWork.ClientSession, participantFilter, cancellationToken);
            
            var messageFilter = Builders<Message>.Filter.Eq(message => message.ConversationId, request.ConversationId);
            await messageRepository.DeleteManyAsync(unitOfWork.ClientSession, messageFilter, cancellationToken);
            
            var domainEvent = MapToDomainEvent(request.ConversationId);
            await publisher.Publish(domainEvent, cancellationToken);
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
    
    private async Task<Result> CheckConversationPermissionAsync(ObjectId conversationId, User staff,
        CancellationToken cancellationToken)
    {
        var exists = await conversationRepository.ExistsAsync(
            c => c.Id == conversationId 
                 && c.HospitalId == staff.HospitalId 
                 && c.ConversationType == ConversationType.Group,
            cancellationToken);

        return exists
            ? Result.Success()
            : Result.Failure(ConversationErrors.Forbidden);
    }
    
    private ConversationDeletedEvent MapToDomainEvent(ObjectId conversationId)
    {
        return new ConversationDeletedEvent(conversationId.ToString());
    }
    
    
}