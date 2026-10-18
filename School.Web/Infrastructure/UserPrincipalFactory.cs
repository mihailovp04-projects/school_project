using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using School.Domain;

namespace School.Web.Infrastructure;

public static class UserPrincipalFactory
{
    public static ClaimsPrincipal Create(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
