using TUnit.Core.Interfaces;

namespace HeadStart.IntegrationTests.Data;

/// <summary>
/// Base class for Playwright tests, providing common functionality and setup for Playwright testing with ASP.NET Core.
/// </summary>
public class PlaywrightDataClass() : IAsyncInitializer, IAsyncDisposable
{
    public Uri BaseUrl { get; private set; } = new("/");
    public Uri KeycloakUrl { get; private set; } = new("/");

    public async Task InitializeAsync()
    {
        if (GlobalSetup.NotificationService != null)
        {
            await GlobalSetup.NotificationService.WaitForResourceAsync("bff", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            BaseUrl = (GlobalSetup.App ?? throw new InvalidOperationException()).GetEndpoint("bff");
            KeycloakUrl = GlobalSetup.App.GetEndpoint("keycloak");
        }
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
