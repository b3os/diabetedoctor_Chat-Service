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
        var permissionResult = await GetParticipantWithPermissionAsync(request.ConversationId, request.AdminId!, cancellationToken);
        if (permissionResult.IsFailure)
        {
            return Result.Failure<Response>(permissionResult.Error);
        }
        
        var usersExistsResult = await GetUsersExistsAsync(request.UserIds, cancellationToken);
        if (usersExistsResult.IsFailure)
        {
            return Result.Failure<Response>(usersExistsResult.Error);
        }
        
        var documents = await participantRepository.CheckDuplicatedParticipantsAsync(request.ConversationId, request.UserIds, cancellationToken);
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
                    var userId = Mapper.MapUserId(participant.User.UserId);
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

        if (dupParticipants.Count != 0 || banParticipants.Count != 0)
        {
            var dupResult = new AddMemberToGroupResponse
            {
                MatchCount = participants.Count,
                DuplicatedUser = dupParticipants.Select(dupUser => new UserResponseDto
                {
                    Id = dupUser.User.UserId.Id,
                    FullName = dupUser.User.FullName,
                    Avatar = dupUser.User.Avatar.PublicUrl
                }).ToList(),
                BannedUser = banParticipants.Select(banUser => new UserResponseDto
                {
                    Id = banUser.User.UserId.Id,
                    FullName = banUser.User.FullName,
                    Avatar = banUser.User.Avatar.PublicUrl
                }).ToList()
            };
            return Result.Failure<Response>(ConversationErrors.GroupMembersAlreadyExistedOrBanned(dupResult));
        }
        
        var allUserIds = usersExistsResult.Value.Select(u => u.UserId).ToList();
        // var userToAdd = usersExistsResult.Value.Where(u => !rejoiners.Contains(u.UserId.Id)).ToList();

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            var userToAdd = allUserIds.Where(userId => !rejoiners.Contains(userId));
            await conversationRepository.AddMemberToConversationAsync(unitOfWork.ClientSession, request.ConversationId, allUserIds, cancellationToken);
            var addParticipants = MapToListParticipant(request.ConversationId, permissionResult.Value.UserId, userToAdd);
            await participantRepository.CreateManyAsync(unitOfWork.ClientSession, addParticipants, cancellationToken);
            await participantRepository.RejoinToConversationAsync(unitOfWork.ClientSession, request.ConversationId, rejoiners, cancellationToken);
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
    
    private async Task<Result<Participant>> GetParticipantWithPermissionAsync(ObjectId? conversationId, string adminId,
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
        
        var projection = Builders<Participant>.Projection.Include(p => p.UserId).Include(p => p.Role);
        var participant = await participantRepository.FindSingleAsync(
            p => p.UserId.Id == adminId
                           && p.ConversationId == conversationId
                           && (p.Role == MemberRole.Owner || p.Role == MemberRole.Admin),
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
        
        var users = await userRepository.FindListAsync(
            user => userIds.Contains(user.UserId.Id)
                    && user.Role != Role.Doctor
                    && user.IsDeleted == false,
            projection,
            cancellationToken);
        return users.Count == userIds.Count() ? Result.Success(users) : Result.Failure<List<Domain.Models.User>>(UserErrors.NotFound);
    }
    
    private IEnumerable<Participant> MapToListParticipant(ObjectId? conversationId, UserId invitedBy, IEnumerable<UserId> userIds)
    {
        var participants = userIds.Select(id =>
            Participant.CreateMember(
                id: ObjectId.GenerateNewId(),
                userId: id,
                conversationId: (ObjectId) conversationId!,
                invitedBy: invitedBy)
        );
        return participants;
    }
    
    private GroupMembersAddedEvent MapToDomainEvent(AddMembersToGroupCommand command)
    {
        return new GroupMembersAddedEvent(command.ConversationId.ToString()!, command.UserIds);
    }
}