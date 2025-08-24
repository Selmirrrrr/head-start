using Aspire.Hosting;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace HeadStart.IntegrationTests;

public static class GlobalSetup
{
    public static DistributedApplication? App { get; private set; }
    public static ResourceNotificationService? NotificationService { get; private set; }

    [Before(TestSession)]
    [Timeout(1_800)] // 3 minutes in milliseconds
    public static async Task SetUpAsync(CancellationToken cancellationToken)
    {
        // Set environment variables for test environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
        Environment.SetEnvironmentVariable("CI", "true");

        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.HeadStart_Aspire_AppHost>(
            [
                "DcpPublisher:RandomizePorts=false"
            ],
            configureBuilder: static (options, _) =>
            {
                options.DisableDashboard = true; // Disable dashboard to speed up startup
            }, cancellationToken);

        // The HttpClient configuration is now handled individually in each test
        // to ensure SSL certificate validation bypass is properly applied

        App = await appHost.BuildAsync(cancellationToken);
        NotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
        await App.StartAsync(cancellationToken);
    }

    [After(TestSession)]
    public static void CleanUp()
    {
        Console.WriteLine("...and after!");
    }
}
