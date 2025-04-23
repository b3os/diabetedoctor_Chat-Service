namespace ChatService.Contract.Services.Message.Commands;

public record CreateMessageCommand : ICommand
{
    public required string GroupId { get; init; }
    public required string Content {get; init; }
}