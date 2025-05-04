using System.Security.Claims;
using ChatService.Contract.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace ChatService.Infrastructure.Services;

public class ClaimsService : IClaimsService
{
    public string GetCurrentUserId { get; }
    public string GetCurrentRole { get; }

    public ClaimsService(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User;
        GetCurrentUserId = GetUserId(identity);
        GetCurrentRole = GetRole(identity);
    }

    private static string GetUserId(ClaimsPrincipal? identity)
    {
        if (identity == null)
        {
            return string.Empty;
        }
        return identity.FindFirstValue("UserId") ?? string.Empty;
    }
    

    private static string GetRole(ClaimsPrincipal? identity)
    {
        if (identity == null)
        {
            return string.Empty;
        }
        return identity.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}