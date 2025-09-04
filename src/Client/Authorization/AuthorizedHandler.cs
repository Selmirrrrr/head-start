using System.Net;

namespace HeadStart.Client.Authorization;

public class AuthorizedHandler(HostAuthenticationStateProvider authenticationStateProvider) : DelegatingHandler
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
