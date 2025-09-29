using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using HeadStart.IntegrationTests.Helpers;
using Shouldly;

namespace HeadStart.IntegrationTests.WebApiTests.Me;

[ClassDataSource<ApiTestDataClass>]
public class GetMeEndpointTests(ApiTestDataClass apiTestDataClass) : BaseApiTest
{
    [Test]
    [Category(TestConfiguration.Categories.Standard)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task GetMe_WithValidAuth_ReturnsUserProfileAsync(CancellationToken cancellationToken)
    {
        // Act
        var response = await apiTestDataClass.User1ApiClient.Api.V1.Me.GetAsync(cancellationToken: cancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.Id.ShouldNotBeEmpty();
        response.Nom.ShouldNotBeNullOrWhiteSpace();
        response.Prenom.ShouldNotBeNullOrWhiteSpace();
        response.Email.ShouldNotBeNullOrWhiteSpace();
        response.Email.ShouldBe(Users.UserApiTest1.UserEmail);
        response.Roles.ShouldNotBeNull();
        response.LangueCode.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    [Category(TestConfiguration.Categories.Security)]
    [Timeout(TestConfiguration.Timeouts.QuickTest)]
    public async Task GetMe_WithoutAuth_ReturnsUnauthorizedAsync(CancellationToken cancellationToken)
    {
        // Act & Assert
        await AssertUnauthorizedAsync(async () =>
            await apiTestDataClass.AnonymousApiClient.Api.V1.Me.GetAsync(cancellationToken: cancellationToken));
    }
}
