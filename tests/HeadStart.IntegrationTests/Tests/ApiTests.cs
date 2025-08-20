using System.Net.Http.Json;
using System.Text.Json;
using HeadStart.Client.Generated;
using HeadStart.IntegrationTests.Data;
using HeadStart.IntegrationTests.Models;
using HeadStart.WebAPI.Features.Users;
using TUnit.Core.Logging;

namespace HeadStart.IntegrationTests.Tests;

[ClassDataSource<HttpClientDataClass>]
public class ApiTests(HttpClientDataClass httpClientData)
{
    [Test]
    public async Task GetWeatherForecastReturnsOkStatusCode()
    {
        // Arrange
        var httpClient = httpClientData.HttpClient;
        // Act
        var response = await httpClient.GetStringAsync("api/users/me");

        File.WriteAllText("response.json", response);
        File.WriteAllText("url.txt", httpClient.BaseAddress.ToString());

        Console.WriteLine(
            $"Response: {response}");
        // Assert
        await Assert.That(response).IsNotNull();
    }
}
