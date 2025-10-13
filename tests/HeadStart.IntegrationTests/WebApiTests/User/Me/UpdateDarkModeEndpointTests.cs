using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using HeadStart.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace HeadStart.IntegrationTests.WebApiTests.User.Me;

[ClassDataSource<ApiTestDataClass>]
public class UpdateDarkModeEndpointTests(ApiTestDataClass apiTestDataClass) : BaseApiTest
{
    [Test]
    [Category(TestConfiguration.Categories.Standard)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task UpdateDarkMode_WithTrue_UpdatesDatabaseAsync(CancellationToken cancellationToken)
    {
        // Arrange
        await using var dbContext = await GetDbContextAsync();
        var apiClient = apiTestDataClass.User1ApiClient.Api.V1.Me;

        // Act
        var response = await apiClient.DarkMode.PatchAsync(
            new()
            {
                IsDarkMode = true
            },
            cancellationToken: cancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.IsDarkMode.ShouldNotBeNull();
        response.IsDarkMode.Value.ShouldBeTrue();

        // Verify in database
        var user = await dbContext.Users.FirstAsync(
            u => u.Id == Users.UserApiTest1.Id,
            cancellationToken: cancellationToken);
        user.DarkMode.ShouldBeTrue();
    }

    [Test]
    [Category(TestConfiguration.Categories.Standard)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task UpdateDarkMode_WithFalse_UpdatesDatabaseAsync(CancellationToken cancellationToken)
    {
        // Arrange
        await using var dbContext = await GetDbContextAsync();
        var apiClient = apiTestDataClass.User2ApiClient.Api.V1.Me;

        // Ensure user exists and set dark mode to true first
        await apiClient.DarkMode.PatchAsync(
            new() { IsDarkMode = true },
            cancellationToken: cancellationToken);

        // Act
        var response = await apiClient.DarkMode.PatchAsync(
            new()
            {
                IsDarkMode = false
            },
            cancellationToken: cancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.IsDarkMode.ShouldNotBeNull();
        response.IsDarkMode.Value.ShouldBeFalse();

        // Verify in database
        var user = await dbContext.Users.FirstAsync(
            u => u.Id == Users.UserApiTest2.Id,
            cancellationToken: cancellationToken);
        user.DarkMode.ShouldBeFalse();
    }

    [Test]
    [Category(TestConfiguration.Categories.Security)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task UpdateDarkMode_WithoutAuth_ReturnsUnauthorizedAsync(CancellationToken cancellationToken)
    {
        // Act & Assert
        await AssertUnauthorizedAsync(async () =>
            await apiTestDataClass.AnonymousApiClient.Api.V1.Me.DarkMode.PatchAsync(
                new() { IsDarkMode = true },
                cancellationToken: cancellationToken));
    }
}
