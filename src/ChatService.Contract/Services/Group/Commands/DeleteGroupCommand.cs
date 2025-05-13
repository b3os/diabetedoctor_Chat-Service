namespace ChatService.Contract.Services.Group.Commands;

public record DeleteGroupCommand : ICommand
{
    public string? OwnerId { get; set; }
    public ObjectId GroupId { get; set; }
}