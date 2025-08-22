namespace HeadStart.EndToEndTests;


public class IntegrationTests
{
	[Fact]
	public async Task TestWebAppHomePage()
	{
		// Arrange
		var appHost = await DistributedApplicationTestingBuilder
				.CreateAsync<Projects.HeadStart_Aspire_AppHost>();

		appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
		{
			clientBuilder.AddStandardResilienceHandler();
		});

		await using var app = await appHost.BuildAsync();

		var resourceNotificationService = app.Services
				.GetRequiredService<ResourceNotificationService>();

		await app.StartAsync();

		// Act
		var httpClient = app.CreateHttpClient("bff");

		await resourceNotificationService.WaitForResourceAsync(
						"bff",
						KnownResourceStates.Running
				)
				.WaitAsync(TimeSpan.FromSeconds(30));

		var response = await httpClient.GetAsync("/api/User");

		// Assert
		response.EnsureSuccessStatusCode();
		var content = await response.Content.ReadAsStringAsync();
		Assert.Contains("isAuthenticated\":false", content);
	}
}
