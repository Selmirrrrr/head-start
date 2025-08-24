// Here you could define global logic that would affect all tests

// You can use attributes at the assembly level to apply to all tests in the assembly

using Aspire.Hosting;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace HeadStart.IntegrationTests;

public static class GlobalSetup
{
    public static DistributedApplication? App { get; private set; }
    public static ResourceNotificationService? NotificationService { get; private set; }

    [Before(TestSession)]
    [Timeout(300_000)] // 5 minutes in milliseconds
    public static async Task SetUpAsync(CancellationToken cancellationToken)
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.HeadStart_Aspire_AppHost>(
            [
                "DcpPublisher:RandomizePorts=false"
            ],
            configureBuilder: static (options, _) =>
            {
                options.DisableDashboard = true; // Disable dashboard to speed up startup
            }, cancellationToken);

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();

            // Configure HttpClient to accept self-signed certificates in test environment
            clientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                // Only bypass SSL validation in CI/test environments
                if (Environment.GetEnvironmentVariable("CI") == "true" ||
                    Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
                {
#pragma warning disable S4830 // Server certificate validation is intentionally disabled for CI testing
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#pragma warning restore S4830
                }

                return handler;
            });
        });

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
