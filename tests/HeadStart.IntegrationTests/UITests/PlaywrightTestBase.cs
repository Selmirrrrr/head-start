using Microsoft.Playwright;
using TUnit.Playwright;

namespace HeadStart.IntegrationTests.UITests;

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
}
