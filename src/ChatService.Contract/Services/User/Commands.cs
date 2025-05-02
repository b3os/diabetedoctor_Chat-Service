namespace ChatService.Contract.Services.User;

public record CreateUserCommand(string Id, string FullName, string Avatar) : ICommand;
public record UpdateUserCommand(string Id, string? FullName, string? Avatar) : ICommand;