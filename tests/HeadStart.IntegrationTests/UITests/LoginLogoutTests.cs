using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using Microsoft.Playwright;

namespace HeadStart.IntegrationTests.UITests;

[ClassDataSource<AspireDataClass>]
public class LoginLogoutTests(AspireDataClass playwrightDataClass) : PlaywrightTestBase
{
    [Test]
    public async Task WebAppHomePageTestAsync()
    {
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        var title = await Page.TitleAsync();
        await Assert.That(title).IsEqualTo("Claimly");
    }

    [Test]
    public async Task LoginTestAsync()
    {
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        await Page.WaitForURLAsync($"{playwrightDataClass.KeycloakUrl}**", new() { Timeout = 10000 });

        await Page.WaitForSelectorAsync("input[name='username']", new() { Timeout = 10000 });

        // Fill in the login credentials
        await Page.FillAsync("input[name='username']", "user1@example.com");
        await Page.FillAsync("input[name='password']", "user1");

        // Submit the login form
        await Page.Locator("#kc-login").ClickAsync();
        // Wait to be redirected back to the main application
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync();

        // Verifiy that we see the users name in the user card button
        await Expect(Page.Locator(".mud-typography.mud-typography-body2"))
            .ToContainTextAsync("FirstName1");
    }

    [Test]
    public async Task LogoutTestsAsync()
    {
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        await Page.WaitForURLAsync($"{playwrightDataClass.KeycloakUrl}**", new() { Timeout = 10000 });

        await Page.WaitForSelectorAsync("input[name='username']", new() { Timeout = 10000 });

        // Fill in the login credentials
        await Page.FillAsync("input[name='username']", "user1@example.com");
        await Page.FillAsync("input[name='password']", "user1");

        // Submit the login form
        await Page.Locator("#kc-login").ClickAsync();

        // Wait to be redirected back to the main application
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync();

        // Click the logout button
        await Page.Locator("button.mud-button-root.mud-icon-button.mud-ripple.mud-icon-button-size-small").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        // Wait for redirect back to Keycloak login page after logout
        await Page.WaitForURLAsync($"{playwrightDataClass.KeycloakUrl}**", new() { Timeout = 10000 });

        // Verify we're back at the login page
        var title = await Page.TitleAsync();
        await Assert.That(title).IsEqualTo("Se connecter à HeadStart");
    }
}
