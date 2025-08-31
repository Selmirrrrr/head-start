using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CleanAspire.ClientApp.Services.Identity;

public class AuthorizedHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly NavigationManager _navigation;

    public AuthorizedHandler(IServiceProvider serviceProvider, NavigationManager navigation)
    {
        _serviceProvider = serviceProvider;
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authenticationStateProvider = _serviceProvider.GetService<AuthenticationStateProvider>();
        
        HttpResponseMessage responseMessage;
        if (authenticationStateProvider != null)
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity is { IsAuthenticated: false })
            {
                responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            else
            {
                responseMessage = await base.SendAsync(request, cancellationToken);
            }
        }
        else
        {
            responseMessage = await base.SendAsync(request, cancellationToken);
        }

        if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
        {
            var returnUrl = _navigation.Uri;
            var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
            var logInUrl = _navigation.ToAbsoluteUri($"api/Account/Login?returnUrl={encodedReturnUrl}");
            _navigation.NavigateTo(logInUrl.ToString(), true);
        }

        return responseMessage;
    }
}