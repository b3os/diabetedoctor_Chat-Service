using ChatService.Contract.DTOs.ValueObjectDtos;

namespace ChatService.Contract.Services.User.Commands;

public record CreateUserCommand : ICommand
{
    public string Id { get; init; } = null!;
    public string? HospitalId { get; init; }
    public FullNameDto FullName { get; init; } = null!;
    public string Avatar { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public int Role { get; init; }
}