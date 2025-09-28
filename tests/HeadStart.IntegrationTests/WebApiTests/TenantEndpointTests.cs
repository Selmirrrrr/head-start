using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using Microsoft.Kiota.Abstractions;
using Shouldly;

namespace HeadStart.IntegrationTests.WebApiTests;

[ClassDataSource<ApiTestDataClass>]
public class TenantEndpointTests(ApiTestDataClass apiTestDataClass) : BaseApiTest
{
    [Test]
    [Category(TestConfiguration.Categories.Standard)]
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
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task GetTenants_WithoutAuth_ReturnsUnauthorizedAsync(CancellationToken cancellationToken)
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<ApiException>(async () =>
            await apiTestDataClass.AnonymousApiClient.Api.V1.Tenants.GetAsync(cancellationToken: cancellationToken));

        exception.ResponseStatusCode.ShouldBe(401);
    }
}
