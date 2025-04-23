namespace ChatService.Contract.Services.User;

public record CreateUserCommand(Guid UserId, string FullName, string Avatar) : ICommand;
public record UpdateUserCommand(Guid UserId, string? FullName, string? Avatar) : ICommand;