using Aspire.Hosting;

namespace HeadStart.EndToEndTests;

public class EnvVarTests
{
	[Fact]
	public async Task WebResourceEnvVarsResolveToApiService()
	{
		// Arrange
		var appHost = await DistributedApplicationTestingBuilder
				.CreateAsync<Projects.HeadStart_Aspire_AppHost>();
		var frontend = (IResourceWithEnvironment)appHost.Resources
				.Single(static r => r.Name == "bff");

		// Act
		var envVars = await frontend.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Publish);

		// Assert
        Assert.Contains(new KeyValuePair<string, string>("services__webapi__https__0", "{webapi.bindings.https.url}"), envVars);
    }
}
