namespace ChatService.Contract.Services.Conversation.Commands.IntegrationCommand;

public record DeleteOldGroupAvatarCommand : ICommand
{
    public string ImagePublicId { get; init; } = null!;
}