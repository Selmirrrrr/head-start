using System.Net;
using HeadStart.Client.Services.Users;
using HeadStart.SharedKernel.Models.Constants;

namespace HeadStart.Client.Authorization;

public class AuthorizedHandler(
    HostAuthenticationStateProvider authenticationStateProvider,
    UserStateService userStateService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();

        HttpResponseMessage responseMessage;
        if (authState.User.Identity is { IsAuthenticated: false })
        {
            // if user is not authenticated, immediately set response status to 401 Unauthorized
            responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }
        else
        {
            // Add X-Tenant-Path header if user has selected a tenant
            var selectedTenant = userStateService.CurrentState.DernierTenantSelectionne;
            if (!string.IsNullOrEmpty(selectedTenant))
            {
                request.Headers.TryAddWithoutValidation(AppHttpHeaders.TenantHeader, selectedTenant);
            }

            responseMessage = await base.SendAsync(request, cancellationToken);
        }

        if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
        {
            // if server returned 401 Unauthorized, redirect to login page
            authenticationStateProvider.SignIn();
        }

        return responseMessage;
    }
}
