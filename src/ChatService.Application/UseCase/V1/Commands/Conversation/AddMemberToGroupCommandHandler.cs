namespace ChatService.Application.UseCase.V1.Commands.Conversation;

public sealed class AddMemberToGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    IParticipantRepository participantRepository,
    IConversationRepository conversationRepository,
    IUserRepository userRepository,
    IOutboxEventRepository outboxEventRepository)
    : ICommandHandler<AddMemberToGroupCommand, Response>
{
    public async Task<Result<Response>> Handle(AddMemberToGroupCommand request, CancellationToken cancellationToken)
    {
        var permissionResult = await GetParticipantWithPermissionAsync(request.ConversationId, request.AdminId, cancellationToken);
        if (permissionResult.IsFailure)
        {
            return Result.Failure<Response>(permissionResult.Error);
        }
        
        var usersExistsResult = await GetUsersExistsAsync(request.UserIds, cancellationToken);
        if (usersExistsResult.IsFailure)
        {
            return Result.Failure<Response>(usersExistsResult.Error);
        }
        
        var dupUsers = await participantRepository.CheckDuplicatedParticipantAsync(request.ConversationId, request.UserIds, cancellationToken);
        if (dupUsers.Count != 0)
        {
            var dupResult = new AddMemberToGroupResponse
            {
                MatchCount = dupUsers.Count, 
                DuplicatedUser = dupUsers.Select(dupUser => new UserDto
                {
                    Id = dupUser.UserId.Id, 
                    FullName = dupUser.FullName,
                    Avatar = dupUser.Avatar.PublicUrl
                }).ToList()
            };
            return Result.Failure<Response>(ConversationErrors.GroupMembersAlreadyExist(dupResult));
        }
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            var userIds = usersExistsResult.Value.Select(u => u.UserId).ToList();
            await conversationRepository.AddMemberToConversationAsync(unitOfWork.ClientSession, request.ConversationId, userIds, cancellationToken);
            var domainEvent = MapToDomainEvent(request.ConversationId, permissionResult.Value.UserId, usersExistsResult.Value);
            await publisher.Publish(domainEvent, cancellationToken);
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
            ConversationMessage.AddMemberToGroupSuccessfully.GetMessage().Code,
            ConversationMessage.AddMemberToGroupSuccessfully.GetMessage().Message));
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
        
        var projection = Builders<Participant>.Projection.Include(p => p.UserId).Include(p => p.Role);
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
    
    private async Task<Result<List<Domain.Models.User>>> GetUsersExistsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken)
    {
        var projection = Builders<Domain.Models.User>.Projection
            .Include(user => user.UserId)
            .Include(user => user.FullName)
            .Include(user => user.Avatar);
        var users = await userRepository.FindListAsync(user => userIds.Contains(user.UserId.Id), projection, cancellationToken);
        return users.Count == userIds.Count() ? Result.Success(users) : Result.Failure<List<Domain.Models.User>>(UserErrors.NotFound);
    }
    
    private GroupMembersAddedEvent MapToDomainEvent(ObjectId conversationId, UserId invitedBy, List<Domain.Models.User> users)
    {
        return new GroupMembersAddedEvent(conversationId, invitedBy, users);
    }
    
    private GroupMembersAddedIntegrationEvent MapToIntegrationEvent(AddMemberToGroupCommand command)
    {
        return new GroupMembersAddedIntegrationEvent()
        {
            ConversationId = command.ConversationId.ToString(),
            Members = command.UserIds
        };
    }
}