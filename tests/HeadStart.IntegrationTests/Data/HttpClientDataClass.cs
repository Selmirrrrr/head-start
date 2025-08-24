using Ardalis.GuardClauses;
using TUnit.Core.Interfaces;

namespace HeadStart.IntegrationTests.Data
{
    public class HttpClientDataClass : IAsyncInitializer, IAsyncDisposable
    {
        public HttpClient HttpClient { get; private set; } = new();
        public async Task InitializeAsync()
        {
            Guard.Against.Null(GlobalSetup.App);

            HttpClient = (GlobalSetup.App).CreateHttpClient("bff");
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
