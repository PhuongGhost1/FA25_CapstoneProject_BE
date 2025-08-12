using System.Security.Claims;
using CusomMapOSM_Application.Interfaces.Services.User;
using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null || !principal.Identity?.IsAuthenticated == true)
            return null;

        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("userId");
        return Guid.TryParse(idClaim?.Value, out var id) ? id : null;
    }

    public string? GetEmail()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null || !principal.Identity?.IsAuthenticated == true)
            return null;

        return principal.FindFirst(ClaimTypes.Email)?.Value
               ?? principal.FindFirst("email")?.Value;
    }
}



