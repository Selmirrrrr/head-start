using HeadStart.WebAPI.Data;
using Microsoft.EntityFrameworkCore;
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
            ColorScheme = ColorScheme.Light

        };
    }

    protected async Task LoginAsync(Uri keycloakUrl, string username, string password)
    {
        await Page.WaitForURLAsync($"{keycloakUrl}**");
        await Page.WaitForSelectorAsync("input[name='username']");

        // Fill in the login credentials
        await Page.FillAsync("input[name='username']", username);
        await Page.FillAsync("input[name='password']", password);

        // Submit the login form
        await Page.Locator("#kc-login").ClickAsync();
    }

    protected static async Task<HeadStartDbContext> GetDbContextAsync()
    {
        var connectionString = await GlobalSetup.App!.GetConnectionStringAsync("postgresdb");
        var optionsBuilder = new DbContextOptionsBuilder<HeadStartDbContext>();
        optionsBuilder.UseNpgsql(connectionString!);
        return new HeadStartDbContext(optionsBuilder.Options);
    }
}
