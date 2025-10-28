using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using HeadStart.IntegrationTests.Helpers;
using HeadStart.SharedKernel.Models.Constants;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace HeadStart.IntegrationTests.WebApiTests.User.Me;

[ClassDataSource<ApiTestDataClass>]
public class UpdateLanguageEndpointTests(ApiTestDataClass apiTestDataClass) : BaseApiTest
{
    [Test]
    [Category(TestConfiguration.Categories.Standard)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task UpdateLanguage_WithValidLanguageCode_UpdatesDatabaseAsync(CancellationToken cancellationToken)
    {
        // Arrange
        await using var dbContext = await GetDbContextAsync();
        var apiClient = apiTestDataClass.User1ApiClient.Api.V1.Me;

        // Act
        var response = await apiClient.Language.PatchAsync(
            new()
            {
                LanguageCode = LanguesCodes.Anglais
            },
            cancellationToken: cancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.LanguageCode.ShouldBe(LanguesCodes.Anglais);

        // Verify in database
        var user = await dbContext.Users.FirstAsync(
            u => u.Id == Users.UserApiTest1.Id,
            cancellationToken: cancellationToken);
        user.LanguageCode.ShouldBe(LanguesCodes.Anglais);
    }

    [Test]
    [Category(TestConfiguration.Categories.Standard)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task UpdateLanguage_WithDifferentLanguageCodes_UpdatesCorrectlyAsync(CancellationToken cancellationToken)
    {
        // Arrange
        await using var dbContext = await GetDbContextAsync();
        var apiClient = apiTestDataClass.User2ApiClient.Api.V1.Me;

        var languageCodes = new[]
        {
            LanguesCodes.Français,
            LanguesCodes.Anglais,
            LanguesCodes.Allemand,
            LanguesCodes.Italien,
            LanguesCodes.Français
        };

        var user = await dbContext.Users.FirstAsync(
            u => u.Id == Users.UserApiTest2.Id,
            cancellationToken: cancellationToken);
        foreach (var languageCode in languageCodes)
        {
            // Act
            var response = await apiClient.Language.PatchAsync(
                new()
                {
                    LanguageCode = languageCode
                },
                cancellationToken: cancellationToken);

            // Assert
            response.ShouldNotBeNull();
            response.LanguageCode.ShouldBe(languageCode);

            // Verify in database
            await dbContext.Entry(user).ReloadAsync(cancellationToken);
            user.LanguageCode.ShouldBe(languageCode);
        }
    }

    [Test]
    [Category(TestConfiguration.Categories.Security)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task UpdateLanguage_WithoutAuth_ReturnsUnauthorizedAsync(CancellationToken cancellationToken)
    {
        // Act & Assert
        await AssertUnauthorizedAsync(async () =>
            await apiTestDataClass.AnonymousApiClient.Api.V1.Me.Language.PatchAsync(
                new() { LanguageCode = LanguesCodes.Anglais },
                cancellationToken: cancellationToken));
    }
}
