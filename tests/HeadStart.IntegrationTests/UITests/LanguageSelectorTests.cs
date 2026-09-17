using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using HeadStart.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Shouldly;

namespace HeadStart.IntegrationTests.UITests;

[ClassDataSource<AspireDataClass>]
public class LanguageSelectorTests(AspireDataClass playwrightDataClass) : PlaywrightTestBase
{
    private const string UserInfoCardSelector = "#user-info-card";

    // Expected greetings in different languages
    private readonly Dictionary<string, string> _expectedGreetings = new()
    {
        { "fr", $"Bonjour {Users.UserUiTest1.UserFirstName}" },
        { "en", $"Hello {Users.UserUiTest1.UserFirstName}" },
        { "de", $"Hallo {Users.UserUiTest1.UserFirstName}" },
        { "it", $"Ciao {Users.UserUiTest1.UserFirstName}" }
    };

    [Test]
    [Category(TestConfiguration.Categories.UserInterface)]
    [Timeout(TestConfiguration.Timeouts.UITest)]
    public async Task LanguageSelector_ShouldChangeUILanguage_AndPersistInDatabaseAsync(CancellationToken ct)
    {
        // Reset the language to French before the test
        await ResetLanguageAsync(ct);

        // Navigate to the application
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        // Login first
        await LoginAsync(playwrightDataClass.KeycloakUrl, Users.UserUiTest1.UserName, Users.UserUiTest1.UserPassword);

        // Wait for the page to fully load after login
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync(new() { Timeout = 30000 });

        // Verify initial language is French
        await AssertGreetingAsync("fr");

        // Verify the language setting in the database
        var languageSetting = await GetLanguageSettingFromDbAsync(Users.UserUiTest1.UserEmail);
        languageSetting.ShouldBe("fr");

        // Wait for the language selector to be visible - use a more robust selector with an ID or aria-label
        var languageIcon = Page.GetByRole(AriaRole.Button, new() { Name = "Change language" }).Or(Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button).Nth(3));
        await languageIcon.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await languageIcon.ClickAsync();

        // Wait for menu to open - the language dropdown doesn't have role='menu', just wait for the language options
        await Page.Locator("p:has-text('English')").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

        // Verify that French is highlighted (has the special styling)
        var frenchMenuItem = Page.Locator("#language-selector-fr");
        var frenchMenuItemStyle = await frenchMenuItem.GetAttributeAsync("style");
        frenchMenuItemStyle?.ShouldContain("font-weight: bold");
        frenchMenuItemStyle?.ShouldContain("background: var(--mud-palette-primary)");

        // Switch to English
        await Page.Locator("p:has-text('English')").ClickAsync();

        // Wait for the page to reload (language change triggers a reload)
        // Use DOMContentLoaded instead of NetworkIdle for more reliable CI execution
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new() { Timeout = 30000 });

        // Wait for the page to fully load after reload
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync(new() { Timeout = 15000 });

        // Verify the greeting changed to English
        await AssertGreetingAsync("en");

        // Verify the change in the database
        languageSetting = await GetLanguageSettingFromDbAsync(Users.UserUiTest1.UserEmail);
        languageSetting.ShouldBe("en");

        // Open the menu again to verify English is now highlighted
        await languageIcon.ClickAsync();
        await Page.Locator("p:has-text('Français')").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

        // Verify that English is now highlighted
        var englishMenuItem = Page.Locator("#language-selector-en");
        var englishMenuItemStyle = await englishMenuItem.GetAttributeAsync("style");
        englishMenuItemStyle?.ShouldContain("font-weight: bold");
        englishMenuItemStyle?.ShouldContain("background: var(--mud-palette-primary)");

        // Verify that French is no longer highlighted
        frenchMenuItem = Page.Locator("#language-selector-fr");
        frenchMenuItemStyle = await frenchMenuItem.GetAttributeAsync("style");
        frenchMenuItemStyle.ShouldBeNullOrEmpty();

        // Close the menu
        await Page.Keyboard.PressAsync("Escape");

        // Refresh the page to verify persistence
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30000 });

        // Wait for the page to reload
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync(new() { Timeout = 15000 });

        // Verify the greeting is still in English
        await AssertGreetingAsync("en");

        // Test switching to German
        await languageIcon.ClickAsync();
        await Page.Locator("p:has-text('Deutsch')").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await Page.Locator("p:has-text('Deutsch')").ClickAsync();

        // Wait for the page to reload
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new() { Timeout = 30000 });
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync(new() { Timeout = 15000 });

        // Verify the greeting changed to German
        await AssertGreetingAsync("de");

        // Verify the change in the database
        languageSetting = await GetLanguageSettingFromDbAsync(Users.UserUiTest1.UserEmail);
        languageSetting.ShouldBe("de");

        // Test switching to Italian
        await languageIcon.ClickAsync();
        await Page.Locator("#language-selector-it").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await Page.Locator("#language-selector-it").ClickAsync();

        // Wait for the page to reload
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new() { Timeout = 30000 });
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync(new() { Timeout = 15000 });

        // Verify the greeting changed to Italian
        await AssertGreetingAsync("it");

        // Verify the change in the database
        languageSetting = await GetLanguageSettingFromDbAsync(Users.UserUiTest1.UserEmail);
        languageSetting.ShouldBe("it");

        // Switch back to French
        await languageIcon.ClickAsync();
        await Page.Locator("#language-selector-fr").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await Page.Locator("#language-selector-fr").ClickAsync();

        // Wait for the page to reload
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new() { Timeout = 30000 });
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync(new() { Timeout = 15000 });

        // Verify the greeting is back to French
        await AssertGreetingAsync("fr");

        // Verify the change in the database
        languageSetting = await GetLanguageSettingFromDbAsync(Users.UserUiTest1.UserEmail);
        languageSetting.ShouldBe("fr");
    }


    private async Task AssertGreetingAsync(string expectedLanguageCode)
    {
        // Get the greeting text from the UserInfoCard
        var greetingLocator = Page.Locator($"{UserInfoCardSelector} .mud-typography-body2").First;

        // Use Playwright's built-in waiting mechanism with a custom assertion
        await Assertions.Expect(greetingLocator).ToHaveTextAsync(_expectedGreetings[expectedLanguageCode]);
    }

    private static async Task ResetLanguageAsync(CancellationToken ct)
    {
        await using var dbContext = await GetDbContextAsync();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == Users.UserUiTest1.UserEmail, cancellationToken: ct);
        if (user != null)
        {
            user.LanguageCode = "fr";
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private static async Task<string> GetLanguageSettingFromDbAsync(string userEmail)
    {
        await using var dbContext = await GetDbContextAsync();
        var user = await dbContext.Users.SingleAsync(u => u.Email == userEmail);
        return user.LanguageCode;
    }
}
