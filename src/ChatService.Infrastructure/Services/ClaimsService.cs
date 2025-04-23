using System.Security.Claims;
using ChatService.Application.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Http;

namespace ChatService.Infrastructure.Services;

public class ClaimsService : IClaimsService
{
    public string GetCurrentUserId { get; }
    public string GetCurrentRole { get; }

    public ClaimsService(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
        GetCurrentUserId = GetUserId(identity);
        GetCurrentRole = GetRole(identity);
    }

    private static string GetUserId(ClaimsIdentity? identity)
    {
        if (identity == null)
        {
            return string.Empty;
        }
        return identity.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value ?? string.Empty;
    }

    private static string GetRole(ClaimsIdentity? identity)
    {
        if (identity == null)
        {
            return string.Empty;
        }
        return identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value ?? string.Empty;
    }
}