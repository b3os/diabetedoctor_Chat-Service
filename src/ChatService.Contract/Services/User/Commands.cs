namespace ChatService.Contract.Services.User;

public record CreateUserCommand(string UserId, string FullName, string Avatar) : ICommand;
public record UpdateUserCommand(string UserId, string? FullName, string? Avatar) : ICommand;