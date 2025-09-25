using Aspire.Hosting;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace HeadStart.IntegrationTests;

public static class GlobalSetup
{
    public static DistributedApplication? App { get; private set; }
    public static ResourceNotificationService? NotificationService { get; private set; }

    [Before(TestSession)]
    [Timeout(3000)] // 5 minutes in milliseconds
    public static async Task SetUpAsync()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.HeadStart_Aspire_AppHost>(
            [
                "DcpPublisher:RandomizePorts=false"
            ],
            configureBuilder: static (options, _) =>
            {
                options.DisableDashboard = true; // Disable dashboard to speed up startup
            });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();

            // Configure HttpClient to accept self-signed certificates in test environment
            clientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                return handler;
            });
        });

        App = await appHost.BuildAsync();
        NotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
        await App.StartAsync();
    }

    [After(TestSession)]
    public static void CleanUp()
    {
        Console.WriteLine("...and after!");
    }
}
