using Ardalis.GuardClauses;
using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using HeadStart.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace HeadStart.IntegrationTests.WebApiTests.Me;

[ClassDataSource<ApiTestDataClass>]
public class UpdateLastSelectedTenantEndpointTests(ApiTestDataClass apiTestDataClass) : BaseApiTest
{
    [Test]
    [Category(TestConfiguration.Categories.Standard)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task UpdateLastSelectedTenant_WithValidTenantPath_UpdatesDatabaseAsync(CancellationToken cancellationToken)
    {
        // Arrange
        const string testTenantPath = "HeadStart.Lausanne";
        await using var dbContext = await GetDbContextAsync();
        var apiClient = apiTestDataClass.Admin1ApiClient.Api.V1.Me.Tenant;

        // Act
        var response = await apiClient.PatchAsync(
            new()
            {
                LastSelectedTenantPath = testTenantPath
            },
            cancellationToken: cancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.LastSelectedTenantPath.ShouldBe(testTenantPath);

        // Verify in database
        var user = await dbContext.Users.FirstAsync(
            u => u.Id == Users.AdminApiTest1.Id,
            cancellationToken: cancellationToken);
        user.DernierTenantSelectionneId.ShouldNotBeNull();
        user.DernierTenantSelectionneId.ToString().ShouldBe(testTenantPath);
    }

    [Test]
    [Category(TestConfiguration.Categories.Standard)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    [Arguments("")]
    [Arguments("   ")]
    [Arguments(null)]
    public async Task UpdateLastSelectedTenant_WithNullOrWhiteSpaceValue_ClearsSelectionAsync(string? tenant, CancellationToken cancellationToken)
    {
        // Arrange
        await using var dbContext = await GetDbContextAsync();
        var apiClient = apiTestDataClass.Admin2ApiClient.Api.V1.Me.Tenant;

        // Ensure user exists and has a selected tenant
        await apiClient.PatchAsync(
            new() { LastSelectedTenantPath = "HeadStart" },
            cancellationToken: cancellationToken);

        // Act - Clear the selection
        var response = await apiClient.PatchAsync(
            new()
            {
                LastSelectedTenantPath = tenant
            },
            cancellationToken: cancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.LastSelectedTenantPath.ShouldBeNull();

        // Verify in database
        var user = await dbContext.Users.FirstAsync(
            u => u.Id == Users.AdminApiTest2.Id,
            cancellationToken: cancellationToken);
        user.DernierTenantSelectionneId.ShouldBeNull();
    }

    [Test]
    [Category(TestConfiguration.Categories.Security)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task UpdateLastSelectedTenant_WithoutAuth_ReturnsUnauthorizedAsync(CancellationToken cancellationToken)
    {
        // Act & Assert
        await AssertUnauthorizedAsync(async () =>
            await apiTestDataClass.AnonymousApiClient.Api.V1.Me.Tenant.PatchAsync(
                new() { LastSelectedTenantPath = "root.test" },
                cancellationToken: cancellationToken));
    }
}
