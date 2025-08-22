using Aspire.Hosting;
using Microsoft.Playwright;

namespace HeadStart.EndToEndTests;

/// <summary>
/// Base class for Playwright tests, providing common functionality and setup for Playwright testing with ASP.NET Core.
/// </summary>
/// <param name="aspireManager"></param>
public abstract class BasePlaywrightTests(AspireManager aspireManager) : IClassFixture<AspireManager>, IAsyncDisposable
{
    protected AspireManager AspireManager { get; } = aspireManager ?? throw new ArgumentNullException(nameof(aspireManager));
    private PlaywrightManager PlaywrightManager => AspireManager.PlaywrightManager;
    private string? DashboardUrl { get; set; }
	public string DashboardLoginToken { get; private set; } = "";
	private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

	private IBrowserContext? _context;

    protected Task<DistributedApplication> ConfigureAsync<TEntryPoint>(
			string[]? args = null,
			Action<IDistributedApplicationTestingBuilder>? configureBuilder = null) where TEntryPoint : class =>
			AspireManager.ConfigureAsync<TEntryPoint>(args, builder =>
			{
				var aspNetCoreUrls = builder.Configuration["ASPNETCORE_URLS"];
				var urls = aspNetCoreUrls is not null ? aspNetCoreUrls.Split(";") : [];

				DashboardUrl = urls.FirstOrDefault();
				DashboardLoginToken = builder.Configuration["AppHost:BrowserToken"] ?? "";

				configureBuilder?.Invoke(builder);
			});

    protected async Task InteractWithPageAsync(string serviceName,
		Func<IPage, Task> test,
		ViewportSize? size = null)
	{

		Uri urlSought;
		var cancellationToken = new CancellationTokenSource(_defaultTimeout).Token;

		// Empty string means the dashboard URL
		if (!string.IsNullOrEmpty(serviceName))
		{
			if (AspireManager.App.GetEndpoint(serviceName) is null)
			{
				throw new InvalidOperationException($"Service '{serviceName}' not found in the application endpoints");
			}

			urlSought = AspireManager.App.GetEndpoint(serviceName);
		}
		else
		{
			urlSought = new Uri(DashboardUrl ?? throw new InvalidOperationException());
		}

		await AspireManager.App?.ResourceNotifications.WaitForResourceHealthyAsync(serviceName, cancellationToken).WaitAsync(_defaultTimeout, cancellationToken)!;

		var page = await CreateNewPageAsync(urlSought, size);

		try
		{
			await test(page);
		}
		finally
		{
			await page.CloseAsync();
		}
	}

	private async Task<IPage> CreateNewPageAsync(Uri uri, ViewportSize? size = null)
	{
		_context = await PlaywrightManager.Browser.NewContextAsync(new BrowserNewContextOptions
		{
			IgnoreHTTPSErrors = true,
			ColorScheme = ColorScheme.Dark,
			ViewportSize = size,
			BaseURL = uri.ToString()
		});

		return await _context.NewPageAsync();

	}


	public async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (_context is not null)
		{
			await _context.DisposeAsync();
		}
	}
}
