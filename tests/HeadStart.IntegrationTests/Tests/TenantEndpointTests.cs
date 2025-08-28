using HeadStart.IntegrationTests.Data;
using Shouldly;

namespace HeadStart.IntegrationTests.Tests;

[ClassDataSource<ApiTestDataClass>]
[Category(TestConfiguration.Categories.Integration)]
public class TenantEndpointTests(ApiTestDataClass apiTestDataClass) : BaseApiTest(apiTestDataClass)
{
    private readonly ApiTestDataClass _apiTestDataClass = apiTestDataClass;

    [Test]
    [Category(TestConfiguration.Categories.Smoke)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task GetTenants_WithValidAuth_ReturnsListOfTenants(CancellationToken cancellationToken)
    {
        // Act
        var response = await _apiTestDataClass.ApiClient.Api.V1.Tenants.GetAsync(cancellationToken: cancellationToken);

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
}
