using HeadStart.IntegrationTests.Core;
using HeadStart.IntegrationTests.Data;
using HeadStart.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Shouldly;

namespace HeadStart.IntegrationTests.UITests;

[ClassDataSource<AspireDataClass>]
public class DarkModeSelectorTests(AspireDataClass playwrightDataClass) : PlaywrightTestBase
{
    private const string DarkModeToggleSelector = "#darkmode-switcher-button";
    private const string LightModeBackground = "rgb(248, 250, 252)";
    private const string DarkModeBackground = "rgb(12, 10, 9)";
    private const string LightModeColor = "rgb(10, 10, 10)";
    private const string DarkModeColor = "rgb(229, 229, 229)";

    private const string ColorSelectorExpression = "el => getComputedStyle(el).color";
    private const string BackgroundSelectorExpression = "el => getComputedStyle(el).backgroundColor";

    [Test]
    [Category(TestConfiguration.Categories.UserInterface)]
    [Timeout(TestConfiguration.Timeouts.UITest)]
    public async Task DarkModeToggle_ShouldChangeUIState_AndPersistAcrossSessionsAsync(CancellationToken ct)
    {
        // Réinitialiser le paramètre de mode sombre avant le test
        await ResetDarkModeAsync(ct);

        // Naviguer vers l'application
        await Page.GotoAsync(playwrightDataClass.BaseUrl.ToString());

        // Se connecter d'abord
        await LoginAsync(playwrightDataClass.KeycloakUrl, Users.UserUiTest1.UserName, Users.UserUiTest1.UserPassword);

        // Attendre que la page se charge complètement après la connexion
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync();

        // Vérifier les couleurs initiales
        await AssertLightModeAsync();

        // Attendre que le sélecteur de mode sombre soit visible
        await Page.WaitForSelectorAsync(DarkModeToggleSelector, new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

        // Obtenir l'état initial du mode sombre
        var darkModeToggle = Page.Locator(DarkModeToggleSelector).First;

        // Cliquer pour basculer vers le mode sombre
        await darkModeToggle.ClickAsync();

        // Attendre un court instant pour que les styles soient appliqués
        await Task.Delay(100, ct);

        // Vérifier le changement d'état dans la base de données
        var darkModeSetting = await GetDarkModeSettingFromDbAsync(Users.UserUiTest1.UserEmail);
        darkModeSetting.ShouldBe(true);

        // Vérifier les couleurs mises à jour
        await AssertDarkModeAsync();

        // Actualiser la page pour vérifier la persistance
        await Page.ReloadAsync();

        // Attendre que la page se recharge
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Claimly" }).First.WaitForAsync();

        // Vérifier les couleurs mises à jour
        await AssertDarkModeAsync();

        // Naviguer vers une autre page
        await Page.GetByRole(AriaRole.Link, new() { Name = "Tenants" }).ClickAsync();

        // Attendre que la page se recharge
        await Page.GetByRole(AriaRole.Heading, new() { Name = "Tenants" }).First.WaitForAsync();

        // Vérifier les couleurs mises à jour
        await AssertDarkModeAsync();

        // Basculer de nouveau vers le mode clair
        await darkModeToggle.ClickAsync();

        // Attendre un court instant pour que les styles soient appliqués
        await Task.Delay(100, ct);

        // Vérifier le changement d'état dans la base de données
        darkModeSetting = await GetDarkModeSettingFromDbAsync(Users.UserUiTest1.UserEmail);
        darkModeSetting.ShouldBe(false);

        // Vérifier les couleurs mises à jour
        await AssertLightModeAsync();
    }

    private async Task AssertDarkModeAsync()
    {
        var updatedBackgroundColor = await Page.Locator("body").EvaluateAsync<string>(BackgroundSelectorExpression);
        var updatedColor = await Page.Locator("body").EvaluateAsync<string>(ColorSelectorExpression);

        updatedColor.ShouldBe(DarkModeColor);
        updatedBackgroundColor.ShouldBe(DarkModeBackground);
    }

    private async Task AssertLightModeAsync()
    {
        var updatedBackgroundColor = await Page.Locator("body").EvaluateAsync<string>(BackgroundSelectorExpression);
        var updatedColor = await Page.Locator("body").EvaluateAsync<string>(ColorSelectorExpression);

        updatedColor.ShouldBe(LightModeColor);
        updatedBackgroundColor.ShouldBe(LightModeBackground);
    }

    private static async Task ResetDarkModeAsync(CancellationToken ct)
    {
        await using var dbContext = await GetDbContextAsync();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "user1@example.com", cancellationToken: ct);
        if (user != null)
        {
            user.DarkMode = false;
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private static async Task<bool> GetDarkModeSettingFromDbAsync(string userEmail)
    {
        await using var dbContext = await GetDbContextAsync();

        var user = await dbContext.Users.SingleAsync(u => u.Email == userEmail);

        return user.DarkMode;
    }
}
