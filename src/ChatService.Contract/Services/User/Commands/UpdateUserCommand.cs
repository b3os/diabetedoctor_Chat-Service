using ChatService.Contract.DTOs.ValueObjectDtos;

namespace ChatService.Contract.Services.User.Commands;

public record UpdateUserCommand : ICommand
{
    public required string Id { get; init; }
    public FullNameDto? FullName { get; init; }
    public string? Avatar { get; init; }
    public string? PhoneNumber { get; init; }
}