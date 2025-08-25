using HeadStart.IntegrationTests.Data;
using Microsoft.Playwright;

namespace HeadStart.IntegrationTests.UITests;

[ClassDataSource<PlaywrightDataClass>]
public class FrontEndTests(PlaywrightDataClass playwrightDataClass) : PlaywrightTestBase
{
    [Test]
    public async Task TestWebAppHomePageAsync()
    {
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        var title = await Page.TitleAsync();
        await Assert.That(title).IsEqualTo("Claimly");
    }

    [Test]
    public async Task LoginTestsAsync()
    {
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        await Page.WaitForURLAsync($"{playwrightDataClass.KeycloakUrl}**", new() { Timeout = 100000 });

        var title = await Page.TitleAsync();

        await Assert.That(title).IsEqualTo("Sign in to HeadStart");

        await Page.WaitForSelectorAsync("input[name='username']", new() { Timeout = 10000 });

        // Fill in the login credentials
        await Page.FillAsync("input[name='username']", "user");
        await Page.FillAsync("input[name='password']", "user");

        // Submit the login form
        await Page.Locator("#kc-login").ClickAsync();
        // Wait to be redirected back to the main application
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" }).WaitForAsync();
        await Expect(Page.GetByText("Hello, Default User!", new() { Exact = true })).ToBeVisibleAsync();
    }
}
