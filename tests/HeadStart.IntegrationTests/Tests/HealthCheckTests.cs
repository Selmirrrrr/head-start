using HeadStart.IntegrationTests.Data;
using Shouldly;

namespace HeadStart.IntegrationTests.Tests;

/// <summary>
/// Simple health check tests to verify the test infrastructure is working
/// </summary>
[ClassDataSource<ApiTestDataClass>]
public class HealthCheckTests(ApiTestDataClass api)
{
    [Test]
    [Category(TestConfiguration.Categories.HealthCheck)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task WebApi_HealthCheck_ReturnsHealthyAsync(CancellationToken cancellationToken)
    {
        // Arrange
        using var httpClient = new HttpClient
        {
            BaseAddress = api.WebApiUrl
        };

        // Act
        var response = await httpClient.GetAsync("/health", cancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        content.ShouldContain("Healthy");
    }

    [Test]
    [Category(TestConfiguration.Categories.HealthCheck)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task Bff_HealthCheck_ReturnsHealthyAsync(CancellationToken cancellationToken)
    {
        // Arrange
        using var httpClient = new HttpClient
        {
            BaseAddress = api.BffUrl
        };

        // Act
        var response = await httpClient.GetAsync("/health", cancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        content.ShouldContain("Healthy");
    }
}
