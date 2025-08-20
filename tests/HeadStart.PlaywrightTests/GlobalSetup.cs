using Aspire.Hosting;
using Microsoft.Playwright;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace HeadStart.PlaywrightTests;

public static class GlobalSetup
{
    public static DistributedApplication? App { get; private set; }
    public static ResourceNotificationService? NotificationService { get; private set; }
    public static IPlaywright? PlaywrightInstance { get; private set; }

    [Before(TestSession)]
    public static async Task SetUp()
    {
        // Initialize Playwright
        PlaywrightInstance = await Playwright.CreateAsync();

        // Set up Aspire application
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.HeadStart_Aspire_AppHost>();
        
        App = await appHost.BuildAsync();
        NotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
        await App.StartAsync();

        // Install Playwright browsers if needed
        Microsoft.Playwright.Program.Main(new[] { "install" });
    }

    [After(TestSession)]
    public static async Task CleanUp()
    {
        PlaywrightInstance?.Dispose();
        if (App != null)
        {
            await App.DisposeAsync();
        }
    }
}