using HeadStart.IntegrationTests.Data;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace HeadStart.IntegrationTests.Tests;

[ClassDataSource<AspireDataClass>]
public class ApiTests(AspireDataClass aspire)
{
    [Test]
    public async Task GetWeatherForecastReturnsOkStatusCodeAsync()
    {
        // Arrange
        var httpClient = aspire.BffHttpClient;

        // Act
        var response = await httpClient.GetStringAsync("api/users/me");

        // Assert
        await Assert.That(response).IsNotNull();
    }
}
