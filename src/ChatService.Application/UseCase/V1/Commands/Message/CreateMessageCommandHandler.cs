using ChatService.Application.Mapping;
using ChatService.Contract.Services.Message.DomainEvents;
using ChatService.Domain.ValueObjects;

namespace ChatService.Application.UseCase.V1.Commands.Message;

public class CreateMessageCommandHandler(
    IMessageRepository messageRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPublisher publisher)
    : ICommandHandler<CreateMessageCommand>
{
    public async Task<Result> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var user = await EnsureUserExists(request.UserId, cancellationToken);

        await EnsureAccessGroupPermissionAsync(request.GroupId, request.UserId, cancellationToken);

        var message = MapToMessage(request.GroupId, request.UserId, request);

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await messageRepository.CreateAsync(unitOfWork.ClientSession, message, cancellationToken);

            await publisher.Publish(MapToEvent(request.GroupId, user, message), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Success(new Response(MessageMessage.CreateMessageSuccessfully.GetMessage().Code,
            MessageMessage.CreateMessageSuccessfully.GetMessage().Message));
    }

    private async Task<Domain.Models.User> EnsureUserExists(string userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindSingleAsync(user => user.UserId.Id == userId,
            cancellationToken: cancellationToken);

        if (user is null)
            throw new UserExceptions.UserNotFoundException();

        return user;
    }

    private async Task EnsureAccessGroupPermissionAsync(ObjectId groupId, string userId,
        CancellationToken cancellationToken)
    {
        var exists = await groupRepository.ExistsAsync(
            group => group.Id == groupId && group.Members.Any(member => member.UserId.Id == userId),
            cancellationToken);

        if (!exists)
            throw new UserExceptions.UserNotFoundException();
    }

    private Domain.Models.Message MapToMessage(ObjectId groupId, string userId, CreateMessageCommand command)
    {
        var id = ObjectId.GenerateNewId();
        var senderUserId = UserId.Of(userId);
        var type = Mapper.MapMessageType(command.Type);
        return Domain.Models.Message.Create(id, groupId, senderUserId, command.Content!, type);
    }

    private ChatCreatedEvent MapToEvent(ObjectId groupId, Domain.Models.User sender, Domain.Models.Message message)
    {
        return new ChatCreatedEvent
        {
            SenderId = sender.UserId.Id,
            SenderFullName = sender.Fullname,
            SenderAvatar = sender.Avatar.PublicUrl,
            MessageId = message.Id.ToString(),
            MessageContent = message.Content,
            GroupId = groupId.ToString()
        };
    }
}