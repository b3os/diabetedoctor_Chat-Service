using ChatService.Application.Infrastructure.Abstractions;
using ChatService.Contract.Common.Constraint;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.ChatIntegrationEvent;
using ChatService.Contract.Services.Message;
using ChatService.Domain.Abstractions;
using MediatR;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Message;

public class CreateMessageCommandHandler(IEventPublisher eventPublisher, IClaimsService claimsService, IMessageRepository messageRepository, IGroupRepository groupRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMessageCommand>
{
    public async Task<Result> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = claimsService.GetCurrentUserId;
        
        var userAndGroup = await FindUserAndGround(userId, request.GroupId, cancellationToken);

        var message = MapToMessage(userId, request);

        await unitOfWork.StartTransactionAsync();
        try
        {
            await messageRepository.CreateAsync(unitOfWork.ClientSession, message, cancellationToken);
            
            var integrationEvent = MapToIntegrationEvent(message, userAndGroup.Group, userAndGroup.User);

            await eventPublisher.PublishAsync(TopicConstraints.ChatTopic, integrationEvent);
            
            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync();
            throw;
        }
        
        return Result.Success(new Response(GroupMessage.CreatedGroupSuccessfully.GetMessage().Code,
            GroupMessage.CreatedGroupSuccessfully.GetMessage().Message));
    }

    private async Task<(Domain.Models.User User, Domain.Models.Group Group)> FindUserAndGround(string userId, string groupId, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindSingleAsync(x => x.UserId.Equals(userId),
            Builders<Domain.Models.User>.Projection
                .Include(user => user.Fullname),
            cancellationToken: cancellationToken);

        if (user is null)
        {
            throw new UserExceptions.UserNotFoundException();
        }
        
        var group = await groupRepository.FindSingleAsync(x => x.Id == ObjectId.Parse(groupId)
                                                               && x.Members.Any(a => a.Equals(userId)),
            Builders<Domain.Models.Group>.Projection
                .Include(group => group.Id)
                .Include(group => group.Name),
            cancellationToken: cancellationToken);

        if (group is null)
        {
            throw new GroupExceptions.GroupNotFoundException();
        }
        
        return (user, group);
    }

    private Domain.Models.Message MapToMessage(string userId, CreateMessageCommand command)
    {
        var id = ObjectId.GenerateNewId();
        return Domain.Models.Message.Create(id, userId, command.Content);
    }

    private ChatCreatedIntegrationEvent MapToIntegrationEvent(Domain.Models.Message message, Domain.Models.Group group, Domain.Models.User user)
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