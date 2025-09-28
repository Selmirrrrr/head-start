using Microsoft.Playwright;
using TUnit.Playwright;

namespace HeadStart.IntegrationTests.Core;

public abstract class PlaywrightTestBase : PageTest
{
    public override BrowserNewContextOptions ContextOptions(TestContext testContext)
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            AcceptDownloads = true,
            Locale = "fr-CH",
            ColorScheme = ColorScheme.Light,
        };
    }

    protected async Task LoginAsync(Uri keycloakUrl, string username, string password)
    {
        await Page.WaitForURLAsync($"{keycloakUrl}**", new() { Timeout = 10000 });
        await Page.WaitForSelectorAsync("input[name='username']", new() { Timeout = 10000 });

        // Fill in the login credentials
        await Page.FillAsync("input[name='username']", username);
        await Page.FillAsync("input[name='password']", password);

        // Submit the login form
        await Page.Locator("#kc-login").ClickAsync();
    }
}
