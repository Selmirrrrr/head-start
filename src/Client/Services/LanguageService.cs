using System.Globalization;

namespace HeadStart.Client.Services;

public class LanguageService
{
    public event Action? OnLanguageChanged;

    public void SetLanguage(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        OnLanguageChanged?.Invoke();
    }
}
