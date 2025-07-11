using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Application.UseCase.V1.Commands.Conversations.GroupConversation;

public sealed class JoinGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    IUserRepository userRepository,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository)
    : ICommandHandler<JoinGroupCommand, Response>
{
    public async Task<Result<Response>> Handle(JoinGroupCommand request, CancellationToken cancellationToken)
    {
        var userExistsResult = await GetUserExistsAsync(request.UserId, cancellationToken);
        if (userExistsResult.IsFailure)
        {
            return Result.Failure<Response>(userExistsResult.Error);
        }
        var userId = userExistsResult.Value.UserId;
        
        var dupResult = await CheckDuplicatedParticipantAsync(request.ConversationId, request.UserId, cancellationToken);
        if (dupResult.IsFailure)
        {
            return Result.Failure<Response>(dupResult.Error);
        }
        
        try
        {
            await unitOfWork.StartTransactionAsync(cancellationToken);
            await conversationRepository.AddMemberToConversationAsync(unitOfWork.ClientSession, request.ConversationId, [userId], cancellationToken);
            if (dupResult.Value is null)
            {
                var participant = MapToParticipant(request.ConversationId, userId, UserId.Of(request.InvitedBy));
                await participantRepository.CreateAsync(unitOfWork.ClientSession, participant, cancellationToken);
            }
            else
            {
                await participantRepository.RejoinToConversationAsync(unitOfWork.ClientSession, request.ConversationId, [userId], cancellationToken);
            }
            
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
            ConversationMessage.AddDoctorToGroupSuccessfully.GetMessage().Code,
            ConversationMessage.AddDoctorToGroupSuccessfully.GetMessage().Message));
    }
    
    private async Task<Result<User>> GetUserExistsAsync(string userId, CancellationToken cancellationToken)
    {
        var projection = Builders<User>.Projection
            .Include(user => user.UserId)
            .Include(user => user.Role);
        
        var user = await userRepository.FindSingleAsync(
            user => user.UserId.Id == userId
                    && user.IsDeleted == false,
            projection,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<User>(UserErrors.NotFound);
        }
        
        return user.Role is not Role.Patient
            ? Result.Failure<User>(UserErrors.MustHaveThisRole(nameof(Role.Patient)))
            : Result.Success(user);
    }
    
    private async Task<Result<Participant?>> CheckDuplicatedParticipantAsync(ObjectId conversationId, string userId, CancellationToken cancellationToken)
    {
        var projection = Builders<Participant>.Projection
            .Include(p => p.UserId)
            .Include(p => p.Status);
        
        var participant = await participantRepository.FindSingleAsync(
            p => p.UserId.Id == userId && p.ConversationId == conversationId,
            projection,
            cancellationToken);

        if (participant?.Status is Status.LocalBan)
        {
            return Result.Failure<Participant?>(ConversationErrors.YouAreBanned);
        }

        return participant?.IsDeleted is false 
            ? Result.Failure<Participant?>(ConversationErrors.YouAlreadyInGroup) 
            : Result.Success(participant);
    }
    
    private Participant MapToParticipant(ObjectId conversationId, UserId userId, UserId invitedBy)
    {
        return Participant.CreateMember(
            id: ObjectId.GenerateNewId(),
            userId: userId,
            conversationId: conversationId,
            invitedBy: invitedBy);
    }
    
    private GroupMembersAddedEvent MapToDomainEvent(JoinGroupCommand command)
    {
        return new GroupMembersAddedEvent(command.ConversationId.ToString(), [command.UserId!]);
    }    
}