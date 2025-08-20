using TUnit.Core.Interfaces;

namespace HeadStart.PlaywrightTests.Data
{
    public class PlaywrightDataClass : IAsyncInitializer, IAsyncDisposable
    {
        public IBrowser Browser { get; private set; } = null!;
        public IPage Page { get; private set; } = null!;
        public string BaseUrl { get; private set; } = string.Empty;

        public async Task InitializeAsync()
        {
            // Wait for the BFF service to be ready
            if (GlobalSetup.NotificationService != null)
            {
                await GlobalSetup.NotificationService.WaitForResourceAsync("bff", KnownResourceStates.Running)
                    .WaitAsync(TimeSpan.FromSeconds(30));
            }

            // Get the BFF URL from the Aspire app
            var httpClient = (GlobalSetup.App ?? throw new NullReferenceException()).CreateHttpClient("bff");
            BaseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? throw new InvalidOperationException("Base URL not found");

            // Initialize Playwright
            var playwright = await Playwright.CreateAsync();
            Browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Environment.GetEnvironmentVariable("CI") == "true" || Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true",
                SlowMo = 100 // Slow down for better stability
            });

            var context = await Browser.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true // Accept self-signed certificates in test environment
            });

            Page = await context.NewPageAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (Page != null)
            {
                await Page.CloseAsync();
            }
            if (Browser != null)
            {
                await Browser.CloseAsync();
            }
        }
    }
}