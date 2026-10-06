using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace School.Web.Infrastructure;

public class AdminGuard
{
    private readonly AuthenticationStateProvider _authProvider;
    private readonly IJSRuntime _js;

    public AdminGuard(AuthenticationStateProvider authProvider, IJSRuntime js)
    {
        _authProvider = authProvider;
        _js = js;
    }

    public async Task<bool?> ConfirmAdminActionAsync(string question)
    {
        var authState = await _authProvider.GetAuthenticationStateAsync();
        if (!authState.User.IsInRole("Admin"))
        {
            return null;
        }

        return await _js.InvokeAsync<bool>("confirm", question);
    }
}