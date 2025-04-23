namespace ChatService.Application.Infrastructure.Abstractions;

public interface IClaimsService
{
    public string GetCurrentUserId { get; }
    public string GetCurrentRole { get; }
}