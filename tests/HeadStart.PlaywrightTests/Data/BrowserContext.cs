using Microsoft.Playwright;
using TUnit.Core.Interfaces;

namespace HeadStart.PlaywrightTests.Data;

public class BrowserContext : IAsyncInitializer, IAsyncDisposable
{
    public IBrowser Browser { get; private set; } = null!;
    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;
    public string BffUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        if (GlobalSetup.PlaywrightInstance == null)
            throw new InvalidOperationException("Playwright not initialized");
        
        if (GlobalSetup.App == null)
            throw new InvalidOperationException("Application not started");

        // Wait for BFF service to be running
        if (GlobalSetup.NotificationService != null)
        {
            await GlobalSetup.NotificationService.WaitForResourceAsync("bff", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(30));
        }

        // Get BFF URL
        BffUrl = GlobalSetup.App.GetEndpoint("bff").ToString();
        
        // Launch browser
        Browser = await GlobalSetup.PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--ignore-certificate-errors", "--ignore-ssl-errors" }
        });

        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        Page = await Context.NewPageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Page != null) await Page.CloseAsync();
        if (Context != null) await Context.CloseAsync();
        if (Browser != null) await Browser.CloseAsync();
    }
}