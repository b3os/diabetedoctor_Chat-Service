namespace ChatService.Contract.Services.User.Commands;

public record CreateUserCommand : ICommand
{
    public string Id { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string? Avatar { get; init; }
}