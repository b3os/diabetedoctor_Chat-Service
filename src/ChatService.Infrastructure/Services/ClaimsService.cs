using System.Security.Claims;
using ChatService.Contract.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace ChatService.Infrastructure.Services;

public class ClaimsService : IClaimsService
{
    private readonly ClaimsPrincipal _user;
    
    public ClaimsService(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
        // var identity = httpContextAccessor.HttpContext?.User;
        // GetCurrentUserId = GetUserId(identity);
        // GetCurrentRole = GetRole(identity);
    }
    
    public string GetCurrentUserId => _user.FindFirstValue("UserId") ?? string.Empty;
    public string GetCurrentRole => _user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    // private static string GetUserId(ClaimsPrincipal? identity)
    // {
    //     if (identity == null)
    //     {
    //         return string.Empty;
    //     }
    //     return identity.FindFirstValue("UserId") ?? string.Empty;
    // }
    //
    //
    // private static string GetRole(ClaimsPrincipal? identity)
    // {
    //     if (identity == null)
    //     {
    //         return string.Empty;
    //     }
    //     return identity.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    // }
}