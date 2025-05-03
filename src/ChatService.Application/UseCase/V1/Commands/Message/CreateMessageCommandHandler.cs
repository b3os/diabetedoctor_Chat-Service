using ChatService.Application.Mapping;
using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.ChatIntegrationEvents;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services.Message;
using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MediatR;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Message;

public class CreateMessageCommandHandler(
    IMessageRepository messageRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMessageCommand>
{
    // IEventPublisher eventPublisher, 
    public async Task<Result> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var groupId = ObjectId.Parse(request.GroupId);
        // var userAndGroup = await FindUserAndGround(userId, groupId, cancellationToken);

        var message = MapToMessage(groupId, request.UserId, request);

        await unitOfWork.StartTransactionAsync();
        try
        {
            await messageRepository.CreateAsync(unitOfWork.ClientSession, message, cancellationToken);

            // var integrationEvent = MapToIntegrationEvent(message, userAndGroup.Group, userAndGroup.User);

            // await eventPublisher.PublishAsync(TopicConstraints.ChatTopic, integrationEvent);

            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync();
            throw;
        }

        return Result.Success(new Response(MessageMessage.CreateMessageSuccessfully.GetMessage().Code,
            MessageMessage.CreateMessageSuccessfully.GetMessage().Message));
    }

    private async Task<(Domain.Models.User User, Domain.Models.Group Group)> FindUserAndGround(string userId,
        ObjectId groupId, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindSingleAsync(x => x.UserId.Id.Equals(userId),
            Builders<Domain.Models.User>.Projection
                .Include(user => user.Fullname),
            cancellationToken: cancellationToken);
        
        if (user is null)
        {
            throw new UserExceptions.UserNotFoundException();
        }

        var group = await groupRepository.FindSingleAsync(x => x.Id == groupId && x.Members.Any(a => a.Equals(userId)),
            
            cancellationToken: cancellationToken);

        if (group is null)
        {
            throw new GroupExceptions.GroupNotFoundException();
        }

        return (user, group);
    }


    private Domain.Models.Message MapToMessage(ObjectId groupId, string userId, CreateMessageCommand command)
    {
        var id = ObjectId.GenerateNewId();
        var senderUserId = UserId.Of(userId);
        var type = ValueObjectMapper.MapMessageType(command.Message.Type);
        return Domain.Models.Message.Create(id, groupId, senderUserId, command.Message.Content, type);
    }

    private ChatCreatedIntegrationEvent MapToIntegrationEvent(Domain.Models.Message message, Domain.Models.Group group,
        Domain.Models.User user)
    {
        return new ChatCreatedIntegrationEvent
        {
            Id = message.Id.ToString(),
            Content = message.Content,
            FullName = user.Fullname,
            GroupName = group.Name,
        };
    }
}