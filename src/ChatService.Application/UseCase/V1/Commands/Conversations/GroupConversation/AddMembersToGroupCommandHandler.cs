using ChatService.Application.Mapping;
using ChatService.Contract.DTOs.ParticipantDtos;
using ChatService.Contract.Services.Conversation.Commands.GroupConversation;
using MongoDB.Bson.Serialization;

namespace ChatService.Application.UseCase.V1.Commands.Conversation.GroupConversation;

public sealed class AddMembersToGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    IParticipantRepository participantRepository,
    IConversationRepository conversationRepository,
    IUserRepository userRepository)
    : ICommandHandler<AddMembersToGroupCommand, Response>
{
    public async Task<Result<Response>> Handle(AddMembersToGroupCommand request, CancellationToken cancellationToken)
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
        
        var usersExistsResult = await GetUsersExistsAsync(request.UserIds, cancellationToken);
        if (usersExistsResult.IsFailure)
        {
            return Result.Failure<Response>(usersExistsResult.Error);
        }

        var checkDupResult = await CheckRejoinerOrDuplicatedParticipantsAsync(request.ConversationId, request.UserIds, cancellationToken);
        if (checkDupResult.IsFailure)
        {
            return Result.Failure<Response>(checkDupResult.Error);
        }
        
        var rejoiners = checkDupResult.Value;
        var allUserIds = usersExistsResult.Value.Select(u => u.UserId).ToList();
        var userToAdd = rejoiners.Count > 0 
            ? allUserIds.Where(userId => !rejoiners.Contains(userId))
            : allUserIds;
        
        try
        {
            await unitOfWork.StartTransactionAsync(cancellationToken);
            
            // add new participants
            await conversationRepository.AddMemberToConversationAsync(unitOfWork.ClientSession, request.ConversationId, allUserIds, cancellationToken);
            var addParticipants = MapToListParticipant(request.ConversationId, userToAdd, user.Value.UserId);
            await participantRepository.CreateManyAsync(unitOfWork.ClientSession, addParticipants, cancellationToken);
            
            // rejoiners
            if (rejoiners.Count > 0)
            {
                await participantRepository.RejoinToConversationAsync(unitOfWork.ClientSession, request.ConversationId, rejoiners, cancellationToken);
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
            ConversationMessage.AddMemberToGroupSuccessfully.GetMessage().Code,
            ConversationMessage.AddMemberToGroupSuccessfully.GetMessage().Message));
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
    
    private async Task<Result<List<User>>> GetUsersExistsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken)
    {
        var projection = Builders<User>.Projection
            .Include(user => user.UserId)
            .Include(user => user.DisplayName)
            .Include(user => user.Avatar);
        
        var users = await userRepository.FindListAsync(
            user => userIds.Contains(user.UserId.Id)
                    && user.Role == Role.Patient
                    && user.IsDeleted == false,
            projection,
            cancellationToken);
        return users.Count == userIds.Count() ? Result.Success(users) : Result.Failure<List<User>>(UserErrors.NotFound);
    }
    
    private async Task<Result<List<UserId>>> CheckRejoinerOrDuplicatedParticipantsAsync(ObjectId conversationId, HashSet<string> userIds, CancellationToken cancellationToken)
    {
        var documents = await participantRepository.CheckDuplicatedParticipantsAsync(conversationId, userIds, cancellationToken);
        var participants = documents.Select(doc => BsonSerializer.Deserialize<ParticipantWithUserDto>(doc)).ToList();
        
        var rejoiners = new List<UserId>();
        var dupParticipants = new List<ParticipantWithUserDto>();
        var banParticipants = new List<ParticipantWithUserDto>();
        if (participants.Count != 0)
        {
            foreach (var participant in participants)
            {
                if (participant.IsDeleted is true && participant.Status is (int)Status.Active)
                {
                    var userId = Mapper.MapUserId(participant.UserId);
                    rejoiners.Add(userId);
                }
                else
                {
                    switch (participant.Status)
                    {
                        case (int)Status.LocalBan:
                            banParticipants.Add(participant);
                            break;
                        default:
                            dupParticipants.Add(participant);
                            break;
                    }
                }
            }
        }

        if (dupParticipants.Count == 0 && banParticipants.Count == 0) return Result.Success(rejoiners);
        
        var dupResult = new AddMemberToGroupResponse
        {
            MatchCount = participants.Count,
            DuplicatedUser = dupParticipants.Select(dupUser => new UserResponseDto
            {
                Id = dupUser.UserId.Id,
                FullName = dupUser.FullName,
                Avatar = dupUser.Avatar
            }).ToList(),
            BannedUser = banParticipants.Select(banUser => new UserResponseDto
            {
                Id = banUser.UserId.Id,
                FullName = banUser.FullName,
                Avatar = banUser.Avatar
            }).ToList()
        };
        return Result.Failure<List<UserId>>(ConversationErrors.GroupMembersAlreadyExistedOrBanned(dupResult));

    }
    
    private IEnumerable<Participant> MapToListParticipant(ObjectId conversationId, IEnumerable<UserId> userIds, UserId invitedBy)
    {
        var participants = userIds.Select(id =>
            Participant.CreateMember(
                id: ObjectId.GenerateNewId(),
                userId: id,
                conversationId: conversationId,
                invitedBy: invitedBy)
        );
        return participants;
    }
    
    private GroupMembersAddedEvent MapToDomainEvent(AddMembersToGroupCommand command)
    {
        return new GroupMembersAddedEvent(command.ConversationId.ToString(), command.UserIds);
    }
}