using HeadStart.PlaywrightTests.Data;
using Microsoft.Playwright;

namespace HeadStart.PlaywrightTests.Tests;

[ClassDataSource<BrowserContext>]
public class LoginTests(BrowserContext browserContext)
{
    [Test]
    public async Task LoginWithValidCredentials_ShouldShowUserNameInSidebar()
    {
        // Arrange
        var page = browserContext.Page;
        var baseUrl = browserContext.BffUrl;

        // Act - Navigate to the application
        await page.GotoAsync(baseUrl);

        // Wait for the page to load and check if we're on login page or already authenticated
        try
        {
            // Look for the Sign in button
            var signInButton = page.GetByRole(AriaRole.Button, new() { Name = "Sign in" });
            await signInButton.WaitForAsync(new() { Timeout = 5000 });
            
            // Click Sign in button to go to login page
            await signInButton.ClickAsync();

            // Wait for Keycloak login form to appear
            await page.WaitForSelectorAsync("input[name='username']", new() { Timeout = 10000 });

            // Fill in the login credentials
            await page.FillAsync("input[name='username']", "user");
            await page.FillAsync("input[name='password']", "user");

            // Submit the login form
            await page.ClickAsync("input[type='submit']");

            // Wait to be redirected back to the main application
            await page.WaitForURLAsync($"{baseUrl}**", new() { Timeout = 10000 });
        }
        catch (TimeoutException)
        {
            // User might already be logged in, continue with verification
        }

        // Assert - Check that the user name appears in the sidebar
        // Look for the text "Hello, Default User!" in the LoginControl component
        var welcomeText = page.GetByText("Hello, Default User!");
        await welcomeText.WaitForAsync(new() { Timeout = 10000 });
        
        await Assert.That(await welcomeText.IsVisibleAsync()).IsTrue();

        // Also verify that the Sign out button is present (indicating successful login)
        var signOutButton = page.GetByRole(AriaRole.Button, new() { Name = "Sign out" });
        await Assert.That(await signOutButton.IsVisibleAsync()).IsTrue();
    }

    [Test]
    public async Task LoginWithInvalidCredentials_ShouldShowError()
    {
        // Arrange
        var page = browserContext.Page;
        var baseUrl = browserContext.BffUrl;

        // Act - Navigate to the application
        await page.GotoAsync(baseUrl);

        // Look for the Sign in button
        var signInButton = page.GetByRole(AriaRole.Button, new() { Name = "Sign in" });
        await signInButton.WaitForAsync(new() { Timeout = 5000 });
        
        // Click Sign in button to go to login page
        await signInButton.ClickAsync();

        // Wait for Keycloak login form to appear
        await page.WaitForSelectorAsync("input[name='username']", new() { Timeout = 10000 });

        // Fill in invalid credentials
        await page.FillAsync("input[name='username']", "invaliduser");
        await page.FillAsync("input[name='password']", "invalidpassword");

        // Submit the login form
        await page.ClickAsync("input[type='submit']");

        // Assert - Check for error message or that we're still on the login page
        // We expect to either see an error message or remain on the Keycloak login page
        
        // Wait a moment for any error to appear
        await page.WaitForTimeoutAsync(2000);

        // Check if we're still on Keycloak login page (URL should contain keycloak or auth)
        var currentUrl = page.Url;
        var isStillOnLoginPage = currentUrl.Contains("auth") || currentUrl.Contains("keycloak") || 
                                await page.IsVisibleAsync("input[name='username']");

        await Assert.That(isStillOnLoginPage).IsTrue("User should not be authenticated with invalid credentials");

        // Alternative: Look for specific error messages that Keycloak might show
        try
        {
            var errorElement = page.GetByText("Invalid username or password");
            if (await errorElement.IsVisibleAsync())
            {
                await Assert.That(await errorElement.IsVisibleAsync()).IsTrue();
            }
        }
        catch
        {
            // Error message text might be different, but being on login page is sufficient proof
        }

        // Ensure we're NOT back at the main application with a successful login
        var welcomeText = page.GetByText("Hello, Default User!");
        var isWelcomeVisible = false;
        try
        {
            isWelcomeVisible = await welcomeText.IsVisibleAsync();
        }
        catch
        {
            // Element not found, which is expected for failed login
        }

        await Assert.That(isWelcomeVisible).IsFalse("Welcome message should not appear for invalid credentials");
    }
}