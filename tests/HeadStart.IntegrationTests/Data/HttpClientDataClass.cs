using HeadStart.Client.Generated;
using TUnit.Core.Interfaces;
using TUnit.Core.Logging;

namespace HeadStart.IntegrationTests.Data
{
    public class HttpClientDataClass : IAsyncInitializer, IAsyncDisposable
    {
        public HttpClient HttpClient { get; private set; } = new();
        public async Task InitializeAsync()
        {
            HttpClient = (GlobalSetup.App ?? throw new NullReferenceException()).CreateHttpClient("bff");
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
