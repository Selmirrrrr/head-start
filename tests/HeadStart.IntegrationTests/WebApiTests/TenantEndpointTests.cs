using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using Microsoft.Kiota.Abstractions;
using Shouldly;

namespace HeadStart.IntegrationTests.WebApiTests;

[ClassDataSource<ApiTestDataClass>]
[Category(TestConfiguration.Categories.Integration)]
public class TenantEndpointTests(ApiTestDataClass apiTestDataClass) : BaseApiTest()
{
    [Test]
    [Category(TestConfiguration.Categories.Smoke)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task GetTenants_WithValidAuth_ReturnsListOfTenantsAsync(CancellationToken cancellationToken)
    {
        // Act
        var response = await apiTestDataClass.AuthApiClient.Api.V1.Tenants.GetAsync(cancellationToken: cancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.Tenants.ShouldNotBeNull();

        // Verify structure of tenant data
        if (response.Tenants.Count != 0)
        {
            var firstTenant = response.Tenants[0];
            firstTenant.Id.ShouldNotBeNullOrWhiteSpace();
            firstTenant.Name.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Test]
    [Category(TestConfiguration.Categories.Security)]
    public async Task GetTenants_WithoutAuth_ReturnsUnauthorizedAsync(CancellationToken cancellationToken)
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(async () =>
            await apiTestDataClass.AnonymousApiClient.Api.V1.Tenants.GetAsync(cancellationToken: cancellationToken));

        exception.ResponseStatusCode.ShouldBe(401);
    }


    [Test]
    [MethodDataSource(nameof(GetNumberOfCalls))]
    [Category(TestConfiguration.Categories.Performance)]
    public async Task GetTenants_MultipleCallsInSequence_MaintainsPerformanceAsync(int numberOfCalls, CancellationToken cancellationToken)
    {
        // This tests that performance doesn't degrade with multiple sequential calls
        var executionTimes = new List<double>();

        for (var i = 0; i < numberOfCalls; i++)
        {
            var (_, elapsed) = await MeasureExecutionTimeAsync(
                async () => await apiTestDataClass.AuthApiClient.Api.V1.Tenants.GetAsync(cancellationToken: cancellationToken));

            executionTimes.Add(elapsed.TotalMilliseconds);
        }

        // Assert - Check that performance is consistent
        var averageTime = executionTimes.Average();
        var maxTime = executionTimes.Max();

        averageTime.ShouldBeLessThan(500, $"Average time for {numberOfCalls} calls should be under 500ms");
        maxTime.ShouldBeLessThan(1000, "No individual call should take more than 1 second");

        // Check for performance degradation
        if (executionTimes.Count > 2)
        {
            var firstHalfAvg = executionTimes.Take(executionTimes.Count / 2).Average();
            var secondHalfAvg = executionTimes.Skip(executionTimes.Count / 2).Average();

            // Second half shouldn't be significantly slower than first half
            secondHalfAvg.ShouldBeLessThan(firstHalfAvg * 1.5,
                "Performance shouldn't degrade significantly over multiple calls");
        }
    }

    public static IEnumerable<int> GetNumberOfCalls()
    {
        yield return 1;
        yield return 5;
        yield return 10;
    }
}
