namespace ChatService.Contract.Services.Group.Commands;

public record RemoveMemberFromGroupCommand : ICommand
{
    public string? AdminId { get; init; }
    public string? GroupId { get; init; }
    public IEnumerable<string> UserIds { get; init; } = [];
}