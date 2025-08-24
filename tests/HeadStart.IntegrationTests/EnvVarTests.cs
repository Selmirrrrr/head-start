using Aspire.Hosting;

namespace HeadStart.IntegrationTests;

public class EnvVarTests
{
	[Test]
	public async Task WebResourceEnvVarsResolveToApiServiceAsync()
	{
		// Arrange
		var appHost = await DistributedApplicationTestingBuilder
				.CreateAsync<Projects.HeadStart_Aspire_AppHost>();

		var frontend = (IResourceWithEnvironment)appHost.Resources
				.Single(static r => r.Name == "bff");

		// Act
		var envVars = await frontend.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Publish);

		// Assert
        await Assert.That(envVars).Contains(new KeyValuePair<string, string>("services__webapi__https__0", "{webapi.bindings.https.url}"));
    }
}
