using TUnit.Core.Interfaces;

namespace HeadStart.IntegrationTests.Data
{
    public class AspireDataClass : IAsyncInitializer, IAsyncDisposable
    {
        public HttpClient BffHttpClient { get; private set; } = new();
        public Uri BaseUrl { get; private set; } = new("/");
        public Uri KeycloakUrl { get; private set; } = new("/");

        public async Task InitializeAsync()
        {
            var app = GlobalSetup.App ?? throw new InvalidOperationException(nameof(GlobalSetup.App));
            BffHttpClient = app.CreateHttpClient("bff");
            BaseUrl = app.GetEndpoint("bff");
            KeycloakUrl = app.GetEndpoint("keycloak");

            if (GlobalSetup.NotificationService != null)
            {
                await GlobalSetup.NotificationService.WaitForResourceAsync("bff", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.CompletedTask;
        }
    }
}
