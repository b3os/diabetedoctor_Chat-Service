using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Application.UseCase.V1.Commands.Conversation.GroupConversation;

public sealed class AddDoctorToGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    IUserRepository userRepository,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository)
    : ICommandHandler<AddDoctorToGroupCommand, Response>
{
    public async Task<Result<Response>> Handle(AddDoctorToGroupCommand request, CancellationToken cancellationToken)
    {
        var permissionResult = await GetParticipantWithPermissionAsync(request.ConversationId, request.AdminId, cancellationToken);
        if (permissionResult.IsFailure)
        {
            return Result.Failure<Response>(permissionResult.Error);
        }
        
        var userExistsResult = await GetUserExistsAsync(request.DoctorId, cancellationToken);
        if (userExistsResult.IsFailure)
        {
            return Result.Failure<Response>(userExistsResult.Error);
        }
        var userId = userExistsResult.Value.UserId;

        var dupResult = await CheckDuplicatedParticipantAsync(request.ConversationId, request.DoctorId, cancellationToken);
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
                var participant = MapToParticipant(request.ConversationId, permissionResult.Value.UserId, userId);
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
        
        var projection = Builders<Participant>.Projection
            .Include(p => p.UserId)
            .Include(p => p.Role);
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
    
    private async Task<Result<Domain.Models.User>> GetUserExistsAsync(string userId, CancellationToken cancellationToken)
    {
        var projection = Builders<Domain.Models.User>.Projection
            .Include(user => user.UserId)
            .Include(user => user.Role);
        
        var user = await userRepository.FindSingleAsync(
            user => user.UserId.Id == userId
                    && user.IsDeleted == false,
            projection,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<Domain.Models.User>(UserErrors.NotFound);
        }
        
        return user.Role is not Role.Doctor
            ? Result.Failure<Domain.Models.User>(UserErrors.MustHaveThisRole(nameof(Role.Doctor)))
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
            return Result.Failure<Participant?>(ConversationErrors.MemberIsBanned);
        }

        return participant?.IsDeleted is false 
            ? Result.Failure<Participant?>(ConversationErrors.MemberAlreadyExisted) 
            : Result.Success(participant);
    }
    
    private Participant MapToParticipant(ObjectId conversationId, UserId invitedBy, UserId userId)
    {
        return Participant.CreateDoctor(
            id: ObjectId.GenerateNewId(),
            userId: userId,
            conversationId: conversationId,
            invitedBy: invitedBy);
    }
    
    private GroupMembersAddedEvent MapToDomainEvent(AddDoctorToGroupCommand command)
    {
        return new GroupMembersAddedEvent(command.ConversationId.ToString(), [command.DoctorId]);
    }    
}