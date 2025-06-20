namespace ChatService.Application.UseCase.V1.Commands.Conversation;

public sealed class CreateConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IConversationRepository conversationRepository,
    IUserRepository userRepository,
    IOutboxEventRepository outboxEventRepository,
    IPublisher publisher) 
    : ICommandHandler<CreateConversationCommand, Response<CreateConversationResponse>>
{
    public async Task<Result<Response<CreateConversationResponse>>> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var usersExistsResult = await GetUsersExistsAsync(request.Members, cancellationToken);
        
        if (usersExistsResult.IsFailure)
        {
            return Result.Failure<Response<CreateConversationResponse>>(usersExistsResult.Error);
        }
        
        var conversation = MapToConversation(request, usersExistsResult.Value.Select(u => u.UserId).ToList());
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await conversationRepository.CreateAsync(unitOfWork.ClientSession, conversation, cancellationToken);
            var domainEvent = MapToDomainEvent(conversation.Id, request.OwnerId!, usersExistsResult.Value);
            await publisher.Publish(domainEvent, cancellationToken);
            var @event = OutboxEventExtension.ToOutboxEvent(KafkaTopicConstraints.ConversationTopic, MapToIntegrationEvent(conversation, request.Members));
            await outboxEventRepository.CreateAsync(unitOfWork.ClientSession, @event, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }
        
        return Result.Success(new Response<CreateConversationResponse>(
            ConversationMessage.CreatedGroupSuccessfully.GetMessage().Code,
            ConversationMessage.CreatedGroupSuccessfully.GetMessage().Message, 
            new CreateConversationResponse(conversation.Id.ToString())));
    }
    
    private async Task<Result<List<Domain.Models.User>>> GetUsersExistsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken)
    {
        var projection = Builders<Domain.Models.User>.Projection
            .Include(user => user.UserId)
            .Include(user => user.FullName)
            .Include(user => user.Avatar);
        var users = await userRepository.FindListAsync(user => userIds.Contains(user.UserId.Id),
            projection, 
            cancellationToken);
        
        return users.Count == userIds.Count() ? Result.Success(users) : Result.Failure<List<Domain.Models.User>>(UserErrors.NotFound);
    }
    
    private Domain.Models.Conversation MapToConversation(CreateConversationCommand command, List<UserId> userIds)
    {
        var id = ObjectId.GenerateNewId();
        var avatar = Image.Of(command.Avatar);
        return Domain.Models.Conversation.CreateGroup(id, command.Name!, avatar, userIds);
    }
    
    private ConversationCreatedEvent MapToDomainEvent(ObjectId conversationId, string ownerId, List<Domain.Models.User> users)
    {
        var ownerUserId = users.Where(u => u.UserId.Id == ownerId).Select(u => u.UserId).FirstOrDefault();
        return new ConversationCreatedEvent(
            ConversationId: conversationId,
            OwnerId: ownerUserId!,
            Users: users
        );
    }
    
    private ConversationCreatedIntegrationEvent MapToIntegrationEvent(Domain.Models.Conversation conversation, IEnumerable<string> memberIds)
    {
        return new ConversationCreatedIntegrationEvent
        {
            ConversationId = conversation.Id.ToString(),
            ConversationName = conversation.Name,
            Members = memberIds
        };
    }
}