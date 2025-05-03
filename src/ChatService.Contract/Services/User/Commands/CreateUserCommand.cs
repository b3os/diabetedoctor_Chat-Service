namespace ChatService.Contract.Services.User.Commands;

public record CreateUserCommand : ICommand
{
    public required string Id { get; init; }
    public required string FullName { get; init; }
    public string? Avatar { get; init; }
}