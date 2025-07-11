using ChatService.Application.Mapping;
using ChatService.Contract.DTOs.ValueObjectDtos;
using ChatService.Contract.Services.Conversation.Commands.GroupConversation;
using MongoDB.Bson.Serialization;

namespace ChatService.Application.UseCase.V1.Commands.Conversation.GroupConversation;

public sealed class CreateGroupConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IConversationRepository conversationRepository,
    IParticipantRepository participantRepository,
    IUserRepository userRepository,
    IPublisher publisher,
    IOptions<AppDefaultSettings> settings) 
    : ICommandHandler<CreateGroupConversationCommand, Response<CreateGroupConversationResponse>>
{
    public async Task<Result<Response<CreateGroupConversationResponse>>> Handle(CreateGroupConversationCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUserWithHospitalAsync(request.OwnerId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure<Response<CreateGroupConversationResponse>>(user.Error);
        }
        
        var usersExistsResult = await GetUsersExistsAsync(request.Members, cancellationToken);
        
        if (usersExistsResult.IsFailure)
        {
            return Result.Failure<Response<CreateGroupConversationResponse>>(usersExistsResult.Error);
        }
        
        var conversation = MapToConversation(request, usersExistsResult.Value.Select(u => u.UserId).ToList(), user.Value.HospitalId!);
        var ownerUserId = usersExistsResult.Value.Where(u => u.UserId.Id == request.OwnerId).Select(u => u.UserId).FirstOrDefault();
        var participants = MapToConversationParticipants(conversation.Id, ownerUserId!, usersExistsResult.Value);

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await conversationRepository.CreateAsync(unitOfWork.ClientSession, conversation, cancellationToken);
            await participantRepository.CreateManyAsync(unitOfWork.ClientSession, participants, cancellationToken);
            var domainEvent = MapToDomainEvent(conversation.Id, conversation.Name, request.Members);
            await publisher.Publish(domainEvent, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }
        
        return Result.Success(new Response<CreateGroupConversationResponse>(
            ConversationMessage.CreatedGroupSuccessfully.GetMessage().Code,
            ConversationMessage.CreatedGroupSuccessfully.GetMessage().Message, 
            new CreateGroupConversationResponse(conversation.Id.ToString())));
    }

    private async Task<Result<UserWithHospitalDto>> GetUserWithHospitalAsync(string ownerId, CancellationToken cancellationToken)
    {
        var userId = UserId.Of(ownerId);
        var document = await userRepository.GetUserWithHospital(userId, cancellationToken);

        if (document is null)
        {
            return Result.Failure<UserWithHospitalDto>(UserErrors.NotFound);
        }
        
        var user = BsonSerializer.Deserialize<UserWithHospitalDto>(document);
        
        return user.HospitalId is not null ? Result.Success(user) : Result.Failure<UserWithHospitalDto>(HospitalErrors.HospitalNotFound);
    }
    
    private async Task<Result<List<User>>> GetUsersExistsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken)
    {
        var projection = Builders<User>.Projection
            .Include(user => user.UserId);
        
        var users = await userRepository.FindListAsync(user => userIds.Contains(user.UserId.Id),
            projection,
            cancellationToken: cancellationToken);
        
        return users.Count == userIds.Count() ? Result.Success(users) : Result.Failure<List<User>>(UserErrors.NotFound);
    }
    
    private Domain.Models.Conversation MapToConversation(CreateGroupConversationCommand command, List<UserId> userIds, HospitalIdDto hospitalIdDto)
    {
        var id = ObjectId.GenerateNewId();
        var avatar = Image.Of("default-avatar", settings.Value.GroupAvatarDefault);
        var hospitalId = Mapper.MapHospitalId(hospitalIdDto);
        return Domain.Models.Conversation.CreateGroup(id, command.Name, avatar, userIds, hospitalId);
    }
    
    private IEnumerable<Participant> MapToConversationParticipants(ObjectId conversationId, UserId ownerId, List<User> users)
    {
        var participants = users.Select(user =>
            {
                return Equals(user.UserId, ownerId) switch
                {
                    true => Participant.CreateOwner(
                        id: ObjectId.GenerateNewId(),
                        userId: user.UserId,
                        conversationId: conversationId,
                        invitedBy: ownerId),
                    _ => Participant.CreateMember(
                        id: ObjectId.GenerateNewId(),
                        userId: user.UserId,
                        conversationId: conversationId,
                        invitedBy: ownerId),
                };
            }
        );
        return participants;
    }
    
    private ConversationCreatedEvent MapToDomainEvent(ObjectId conversationId, string conversationName, HashSet<string> memberIds)
    {
        return new ConversationCreatedEvent(conversationId.ToString(), conversationName, memberIds);
    }
}