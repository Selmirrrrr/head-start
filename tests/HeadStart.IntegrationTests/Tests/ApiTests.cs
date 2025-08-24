using HeadStart.IntegrationTests.Data;

namespace HeadStart.IntegrationTests.Tests;

[ClassDataSource<HttpClientDataClass>]
public class ApiTests(HttpClientDataClass httpClientData)
{
    [Test]
    public async Task GetWeatherForecastReturnsOkStatusCodeAsync()
    {
        // Arrange
        var httpClient = httpClientData.HttpClient;
        // Act
        var response = await httpClient.GetStringAsync("api/users/me");

        await File.WriteAllTextAsync("response.json", response);
        await File.WriteAllTextAsync("url.txt", httpClient.BaseAddress?.ToString());

        Console.WriteLine($"Response: {response}");

        // Assert
        await Assert.That(response).IsNotNull();
    }
}
