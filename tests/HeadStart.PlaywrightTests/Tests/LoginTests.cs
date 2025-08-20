using HeadStart.PlaywrightTests.Data;

namespace HeadStart.PlaywrightTests.Tests;

[ClassDataSource<PlaywrightDataClass>]
public class LoginTests(PlaywrightDataClass playwrightData)
{
    [Test]
    public async Task LoginWithValidCredentials_ShouldShowUserNameInSidebar()
    {
        // Arrange
        var page = playwrightData.Page;
        var baseUrl = playwrightData.BaseUrl;

        // Navigate to the application
        await page.GotoAsync(baseUrl);

        // Wait for the page to load and check if we need to login
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Look for login elements (this might redirect to Keycloak)
        await page.WaitForSelectorAsync("input[name='username'], input[id='username']", new PageWaitForSelectorOptions { Timeout = 10000 });

        // Fill in valid credentials
        await page.FillAsync("input[name='username'], input[id='username']", "user");
        await page.FillAsync("input[name='password'], input[id='password']", "user");

        // Submit the login form
        await page.ClickAsync("input[type='submit'], button[type='submit']");

        // Wait for redirect back to the application
        await page.WaitForURLAsync($"{baseUrl}/**");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify that the user is logged in by checking for the welcome message in the sidebar
        var welcomeMessage = page.Locator("text=Hello, Default User!");
        await Assert.That(welcomeMessage).IsVisible();

        // Also verify that a Sign out button/link is present
        var signOutElement = page.Locator("text=Sign out, text=Logout, text=Sign Out").First;
        await Assert.That(signOutElement).IsVisible();
    }

    [Test]
    public async Task LoginWithInvalidCredentials_ShouldShowError()
    {
        // Arrange
        var page = playwrightData.Page;
        var baseUrl = playwrightData.BaseUrl;

        // Navigate to the application
        await page.GotoAsync(baseUrl);

        // Wait for the page to load and check if we need to login
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Look for login elements (this might redirect to Keycloak)
        await page.WaitForSelectorAsync("input[name='username'], input[id='username']", new PageWaitForSelectorOptions { Timeout = 10000 });

        // Fill in invalid credentials
        await page.FillAsync("input[name='username'], input[id='username']", "wronguser");
        await page.FillAsync("input[name='password'], input[id='password']", "wrongpassword");

        // Submit the login form
        await page.ClickAsync("input[type='submit'], button[type='submit']");

        // Wait a moment for the error to appear
        await page.WaitForTimeoutAsync(2000);

        // Verify that we're still on the login page or an error is shown
        // Check for error message or that we're still on the login form
        var usernameField = page.Locator("input[name='username'], input[id='username']");
        await Assert.That(usernameField).IsVisible();

        // Verify that the welcome message does NOT appear (user is not logged in)
        var welcomeMessage = page.Locator("text=Hello, Default User!");
        await Assert.That(welcomeMessage).IsNotVisible();
    }
}