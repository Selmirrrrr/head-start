namespace HeadStart.Client.Services;

public static class SupportedLocalization
{
    public const string ResourcesPath = "Resources";

    public static readonly LanguageCode[] SupportedLanguages =
    {
        new()
        {
            Code = "fr",
            DisplayName = "Français"
        },
        new()
        {
            Code = "en",
            DisplayName = "English"
        },
        new()
        {
            Code = "de",
            DisplayName = "Deutsch"
        },
        new()
        {
            Code = "it",
            DisplayName = "Italiano"
        }
    };
}

public class LanguageCode
{
    public string DisplayName { get; set; } = "Français";
    public string Code { get; set; } = "fr";
}
