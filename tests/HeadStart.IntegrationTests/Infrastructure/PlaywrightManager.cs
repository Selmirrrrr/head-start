using Microsoft.Playwright;
using TUnit.Core.Interfaces;

namespace HeadStart.IntegrationTests.Infrastructure;

/// <summary>
/// Configure Playwright for interacting with the browser in tests.
/// </summary>
public class PlaywrightManager :  IAsyncInitializer, IAsyncDisposable
{
	private static bool IsHeadless => false;

	private IPlaywright? _playwright;

	internal IBrowser Browser { get; set; } = null!;

	public async Task InitializeAsync()
	{
		Assertions.SetDefaultExpectTimeout(10_000);

		_playwright = await Playwright.CreateAsync();

		var options = new BrowserTypeLaunchOptions
		{
			Headless = IsHeadless
		};

		Browser = await _playwright.Chromium.LaunchAsync(options).ConfigureAwait(false);
	}

    public async ValueTask DisposeAsync()
    {
        await Browser.CloseAsync();
        _playwright?.Dispose();
    }
}
