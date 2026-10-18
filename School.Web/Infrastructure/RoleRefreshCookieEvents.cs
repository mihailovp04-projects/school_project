using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using School.Services.Services;

namespace School.Web.Infrastructure;

public class RoleRefreshCookieEvents : CookieAuthenticationEvents
{
    private readonly IUserService _userService;

    public RoleRefreshCookieEvents(IUserService userService)
    {
        _userService = userService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var login = context.Principal?.Identity?.Name;
        var user = string.IsNullOrEmpty(login) ? null : await _userService.GetByLoginAsync(login);

        if (user == null)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        if (!context.Principal!.IsInRole(user.Role))
        {
            context.ReplacePrincipal(UserPrincipalFactory.Create(user));
            context.ShouldRenew = true;
        }
    }
}
