using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using HeadStart.IntegrationTests.Helpers;
using Microsoft.Playwright;

namespace HeadStart.IntegrationTests.UITests;

[ClassDataSource<AspireDataClass>]
public class LoginLogoutTests(AspireDataClass playwrightDataClass) : PlaywrightTestBase
{
    [Test]
    [Category(TestConfiguration.Categories.UserInterface)]
    [Timeout(TestConfiguration.Timeouts.UITest)]
    public async Task WebAppHomePageTestAsync(CancellationToken ct)
    {
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        var title = await Page.TitleAsync();
        await Assert.That(title).IsEqualTo("Claimly");
    }

    [Test]
    [Category(TestConfiguration.Categories.UserInterface)]
    [Timeout(TestConfiguration.Timeouts.UITest)]
    public async Task LoginTestAsync(CancellationToken ct)
    {
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        await Page.WaitForURLAsync($"{playwrightDataClass.KeycloakUrl}**");

        await Page.WaitForSelectorAsync("input[name='username']");

        // Fill in the login credentials
        await Page.FillAsync("input[name='username']", Users.UserUiTest1.UserName);
        await Page.FillAsync("input[name='password']", Users.UserUiTest1.UserPassword);

        // Submit the login form
        await Page.Locator("#kc-login").ClickAsync();
        // Wait to be redirected back to the main application
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync();

        // Verifiy that we see the users name in the user card button
        await Expect(Page.Locator(".mud-typography.mud-typography-body2"))
            .ToContainTextAsync(Users.UserUiTest1.UserFirstName);
    }

    [Test]
    [Category(TestConfiguration.Categories.UserInterface)]
    [Timeout(TestConfiguration.Timeouts.UITest)]
    public async Task LogoutTestsAsync(CancellationToken ct)
    {
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        await Page.WaitForURLAsync($"{playwrightDataClass.KeycloakUrl}**");

        await Page.WaitForSelectorAsync("input[name='username']");

        // Fill in the login credentials
        await Page.FillAsync("input[name='username']", Users.UserUiTest1.UserName);
        await Page.FillAsync("input[name='password']", Users.UserUiTest1.UserPassword);

        // Submit the login form
        await Page.Locator("#kc-login").ClickAsync();

        // Wait to be redirected back to the main application
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync();

        // Click the logout button
        await Page.Locator("button.mud-button-root.mud-icon-button.mud-ripple.mud-icon-button-size-small").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        // Wait for redirect back to Keycloak login page after logout
        await Page.WaitForURLAsync($"{playwrightDataClass.KeycloakUrl}**");

        // Verify we're back at the login page
        var title = await Page.TitleAsync();
        await Assert.That(title).IsEqualTo("Se connecter à HeadStart");
    }
}
