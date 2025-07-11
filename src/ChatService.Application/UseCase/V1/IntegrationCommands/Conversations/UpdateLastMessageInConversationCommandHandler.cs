using ChatService.Application.Mapping;
using ChatService.Contract.Services.Conversation.Commands.IntegrationCommand;

namespace ChatService.Application.UseCase.V1.IntegrationCommands.Conversations;

public sealed class UpdateLastMessageInConversationCommandHandler(
    IUnitOfWork unitOfWork,
    IConversationRepository conversationRepository)
: ICommandHandler<UpdateLastMessageInConversationCommand>
{
    public async Task<Result> Handle(UpdateLastMessageInConversationCommand request, CancellationToken cancellationToken)
    {
        var lastMessage = MapToLastMessage(request);
        await conversationRepository.UpdateLastMessageInConversationAsync(unitOfWork.ClientSession, request.ConversationId, lastMessage, cancellationToken);
        return Result.Success(); 
    }

    private Message MapToLastMessage(UpdateLastMessageInConversationCommand command)
    {
        return Message.CreateFromEvent(
            id: command.MessageId,
            conversationId: command.ConversationId,
            senderId: command.SenderId is null ? null : UserId.Of(command.SenderId),
            content: command.MessageContent,
            file: Mapper.MapFileAttachment(command.FileAttachmentDto),
            createdDate: command.CreatedDate,
            type: (MessageType)command.MessageType);
    }
}