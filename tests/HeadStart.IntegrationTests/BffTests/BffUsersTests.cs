using HeadStart.IntegrationTests.Data;

namespace HeadStart.IntegrationTests.BffTests;

[ClassDataSource<AspireDataClass>]
public class BffUsersTests(AspireDataClass aspire)
{
    [Test]
    public async Task UsersMe_IsNotNullAsync()
    {
        // Arrange
        var httpClient = aspire.BffHttpClient;

        // Act
        var response = await httpClient.GetStringAsync("api/users/me");

        // Assert
        await Assert.That(response).IsNotNull();
    }
}
