using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using Shouldly;

namespace HeadStart.IntegrationTests.WebApiTests.Admin.Tenants;

[ClassDataSource<ApiTestDataClass>]
public class TenantEndpointTests(ApiTestDataClass apiTestDataClass) : BaseApiTest
{

    [Test]
    [Category(TestConfiguration.Categories.Standard)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task GetTenants_WithValidAuthAndPlatformAdmin_ReturnsListOfTenantsAsync(CancellationToken cancellationToken)
    {
        // Act
        var response = await apiTestDataClass.PlatformAdmin1ApiClient.Api.V1.PlatformAdmin.Tenants.GetAsync(cancellationToken: cancellationToken);

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
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task GetTenants_WithValidAuthButNotPlatformAdmin_ReturnsForbiddenAsync(CancellationToken cancellationToken)
    {
        // Act
        await AssertForbiddenAsync(async () =>
            await apiTestDataClass.Admin1ApiClient.Api.V1.PlatformAdmin.Tenants.GetAsync(cancellationToken: cancellationToken));
    }

    [Test]
    [Category(TestConfiguration.Categories.Security)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task GetTenants_WithoutAuth_ReturnsUnauthorizedAsync(CancellationToken cancellationToken)
    {
        // Act & Assert
        await AssertUnauthorizedAsync(async () =>
            await apiTestDataClass.AnonymousApiClient.Api.V1.PlatformAdmin.Tenants.GetAsync(cancellationToken: cancellationToken));
    }
}
