using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Application.UseCase.V1.Commands.Conversation.GroupConversation;

public sealed class DeleteGroupConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository,
    IMessageRepository messageRepository,
    IOutboxEventRepository outboxEventRepository,
    IPublisher publisher)
    : ICommandHandler<DeleteGroupConversationCommand, Response>
{
    public async Task<Result<Response>> Handle(DeleteGroupConversationCommand request, CancellationToken cancellationToken)
    {
        var checkResult = await CheckConversationPermissionAsync(request.ConversationId, request.OwnerId, cancellationToken);

        if (checkResult.IsFailure)
        {
            return Result.Failure<Response>(checkResult.Error);
        }
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            var filter = Builders<Domain.Models.Conversation>.Filter.Eq(c => c.Id, request.ConversationId);
            await conversationRepository.DeleteOneAsync(unitOfWork.ClientSession, filter, cancellationToken);
            
            var messageFilter = Builders<Domain.Models.Message>.Filter.Eq(message => message.ConversationId, request.ConversationId);
            var participantFilter = Builders<Participant>.Filter.Eq(participant => participant.ConversationId, request.ConversationId);
            await messageRepository.DeleteManyAsync(unitOfWork.ClientSession, messageFilter, cancellationToken);
            await participantRepository.DeleteManyAsync(unitOfWork.ClientSession, participantFilter, cancellationToken);
            
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
        
        var participant = await participantRepository.ExistsAsync(
            p => p.UserId.Id == ownerId 
                           && p.ConversationId == conversationId
                           && p.Role == MemberRole.Owner,
            cancellationToken);
        
        return participant is false
            ? Result.Failure(ConversationErrors.Forbidden)
            : Result.Success();
    }
    
    private ConversationDeletedEvent MapToDomainEvent(ObjectId conversationId)
    {
        return new ConversationDeletedEvent(conversationId.ToString());
    }
    
    
}