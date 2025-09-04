using System.Net.Http.Json;
using System.Security.Claims;
using HeadStart.SharedKernel.Models.Models.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace HeadStart.Client.Authorization;

public class HostAuthenticationStateProvider(NavigationManager navigation, HttpClient client, ILogger<HostAuthenticationStateProvider> logger) : AuthenticationStateProvider
{
    private static readonly TimeSpan _userCacheRefreshInterval = TimeSpan.FromSeconds(60);

    private const string LoginPath = "api/Account/Login";

    private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return new AuthenticationState(await GetUser(useCache: true));
    }

    public void SignIn(string? customReturnUrl = null)
    {
        var returnUrl = customReturnUrl != null ? navigation.ToAbsoluteUri(customReturnUrl).ToString() : null;
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? navigation.Uri);
        var logInUrl = navigation.ToAbsoluteUri($"{LoginPath}?returnUrl={encodedReturnUrl}");
        navigation.NavigateTo(logInUrl.ToString(), true);
    }

    private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = false)
    {
        var now = DateTimeOffset.Now;
        if (useCache && now < _userLastCheck + _userCacheRefreshInterval)
        {
            logger.LogDebug("Taking user from cache");
            return _cachedUser;
        }

        logger.LogDebug("Fetching user");
        _cachedUser = await FetchUser();
        _userLastCheck = now;

        return _cachedUser;
    }

    private async Task<ClaimsPrincipal> FetchUser()
    {
        UserInfo? user = null;

        try
        {
            logger.LogInformation("Attempting to fetch user from: '{BaseAddress}' base url.", client.BaseAddress?.ToString());
            user = await client.GetFromJsonAsync<UserInfo>("api/User");
        }
        catch (Exception exc)
        {
            logger.LogWarning(exc, "Fetching user failed.");
        }

        if (user?.IsAuthenticated != true)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var identity = new ClaimsIdentity(
            nameof(HostAuthenticationStateProvider),
            user.NameClaimType,
            user.RoleClaimType);

        if (user.Claims != null)
        {
            foreach (var claim in user.Claims)
            {
                identity.AddClaim(new Claim(claim.Type, claim.Value));
            }
        }

        return new ClaimsPrincipal(identity);
    }
}
