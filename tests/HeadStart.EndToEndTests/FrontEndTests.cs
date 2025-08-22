using Microsoft.Playwright;

namespace HeadStart.EndToEndTests;

public class FrontEndTests(AspireManager aspireManager) : BasePlaywrightTests(aspireManager)
{
    [Fact]
	public async Task TestWebAppHomePage()
	{
		await ConfigureAsync<Projects.HeadStart_Aspire_AppHost>();

        await InteractWithPageAsync("bff", async page =>
        {
            await page.GotoAsync("/");

            var title = await page.TitleAsync();
            Assert.Equal("Claimly", title);
        });
    }

    [Fact]
    public async Task LoginTests()
    {
        await ConfigureAsync<Projects.HeadStart_Aspire_AppHost>([
            "DcpPublisher:RandomizePorts=false"
        ]);

        await InteractWithPageAsync("bff", async page =>
        {
            var baseUrl = page.Url;

            await page.GotoAsync("");

            await page.WaitForURLAsync("http://localhost:8080/**");

            var title = await page.TitleAsync();

            Assert.Equal("Sign in to HeadStart", title);

            await page.WaitForSelectorAsync("input[name='username']", new() { Timeout = 10000 });

            // Fill in the login credentials
            await page.FillAsync("input[name='username']", "user");
            await page.FillAsync("input[name='password']", "user");

            // Submit the login form
            await page.Locator("#kc-login").ClickAsync();
            // Wait to be redirected back to the main application
            await page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" }).WaitForAsync();
            await Expect(page.GetByText("Hello, Default User!", new() { Exact = true })).ToBeVisibleAsync();

        });
    }
}
