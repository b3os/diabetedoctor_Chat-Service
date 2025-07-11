using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Application.UseCase.V1.Commands.Conversations.GroupConversation;

public sealed class AddAdminToGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    IUserRepository userRepository,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository)
    : ICommandHandler<AddAdminToGroupCommand, Response>
{
    public async Task<Result<Response>> Handle(AddAdminToGroupCommand request, CancellationToken cancellationToken)
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
        
        var userExistsResult = await GetUserExistsAsync(request.UserId, user.Value, cancellationToken);
        if (userExistsResult.IsFailure)
        {
            return Result.Failure<Response>(userExistsResult.Error);
        }
        var userId = userExistsResult.Value.UserId;
        
        var dupResult = await CheckDuplicatedParticipantAsync(request.ConversationId, userId, cancellationToken);
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
                var participant = MapToParticipant(request.ConversationId, userId, user.Value.UserId);
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
    
    private async Task<Result<User>> GetUserExistsAsync(string doctorId, User staff, CancellationToken cancellationToken)
    {
        var userId = UserId.Of(doctorId);
        var projection = Builders<User>.Projection
            .Include(user => user.UserId)
            .Include(user => user.Role);
        
        var user = await userRepository.FindSingleAsync(
            user => user.UserId == userId
                    && user.IsDeleted == false,
            projection,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<User>(UserErrors.NotFound);
        }
        
        if (user.HospitalId is null || !user.HospitalId.Equals(staff.HospitalId))
        {
            return Result.Failure<User>(UserErrors.StaffNotBelongToHospital);
        }
        
        return user.Role is not Role.HospitalStaff
            ? Result.Failure<User>(UserErrors.MustHaveThisRole(nameof(Role.HospitalStaff)))
            : Result.Success(user);
    }
    
    private async Task<Result<Participant?>> CheckDuplicatedParticipantAsync(ObjectId conversationId, UserId userId, CancellationToken cancellationToken)
    {
        var projection = Builders<Participant>.Projection
            .Include(p => p.UserId)
            .Include(p => p.Status);
        
        var participant = await participantRepository.FindSingleAsync(
            p => p.UserId == userId && p.ConversationId == conversationId,
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
    
    private Participant MapToParticipant(ObjectId conversationId, UserId userId, UserId invitedBy)
    {
        return Participant.CreateAdmin(
            id: ObjectId.GenerateNewId(),
            userId: userId,
            conversationId: conversationId,
            invitedBy: invitedBy);
    }
    
    private GroupMembersAddedEvent MapToDomainEvent(AddAdminToGroupCommand command)
    {
        return new GroupMembersAddedEvent(command.ConversationId.ToString(), [command.UserId]);
    }    
}