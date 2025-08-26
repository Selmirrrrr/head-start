using HeadStart.IntegrationTests.Data;
using Microsoft.Playwright;

namespace HeadStart.IntegrationTests.UITests;

[ClassDataSource<AspireDataClass>]
public class FrontEndTests(AspireDataClass playwrightDataClass) : PlaywrightTestBase
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

        await Page.WaitForURLAsync($"{playwrightDataClass.KeycloakUrl}**", new() { Timeout = 10000 });

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

        // Verifiy that we see the logout button
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Default User", new() { Exact = true })).ToBeVisibleAsync();
    }
}
