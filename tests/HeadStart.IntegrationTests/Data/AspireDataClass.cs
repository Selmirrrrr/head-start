using Ardalis.GuardClauses;
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
            Guard.Against.Null(GlobalSetup.App);

            BffHttpClient = (GlobalSetup.App).CreateHttpClient("bff");
            BaseUrl = (GlobalSetup.App ?? throw new InvalidOperationException()).GetEndpoint("bff");
            KeycloakUrl = GlobalSetup.App.GetEndpoint("keycloak");

            if (GlobalSetup.NotificationService != null)
            {
                await GlobalSetup.NotificationService.WaitForResourceAsync("bff", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Console.Out.WriteLineAsync("And when the class is finished with, we can clean up any resources.");
        }
    }
}
