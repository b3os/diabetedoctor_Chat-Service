namespace ChatService.Contract.Services.User.Commands;

public record UpdateUserCommand : ICommand
{
    public required string Id { get; init; }
    public string? FullName { get; init; }
    public string? Avatar { get; init; }
}