using HeadStart.Client.Services;
using MudBlazor;

namespace HeadStart.Client.Components.Autocompletes;

public class LanguageAutocomplete<T> : MudAutocomplete<string>
{
    public LanguageAutocomplete()
    {
        SearchFunc = SearchFunc_;
        Dense = true;
        ResetValueOnEmptyText = true;
        ToStringFunc = x =>
        {
            var language = Languages.FirstOrDefault(lang => lang.Code.Equals(x, StringComparison.OrdinalIgnoreCase));
            return language != null ? $"{language.DisplayName}" : x;
        };
    }

    private List<LanguageCode> Languages { get; set; } = SupportedLocalization.SupportedLanguages.ToList();

    private Task<IEnumerable<string>> SearchFunc_(string value, CancellationToken cancellation = default)
    {
        return string.IsNullOrEmpty(value)
            ? Task.FromResult(Languages.Select(lang => lang.Code).AsEnumerable())
            : Task.FromResult(Languages
                .Where(lang => Contains(lang, value))
                .Select(lang => lang.Code));
    }

    private static bool Contains(LanguageCode language, string value)
    {
        return language.Code.Contains(value, StringComparison.InvariantCultureIgnoreCase) ||
               language.DisplayName.Contains(value, StringComparison.InvariantCultureIgnoreCase);
    }
}
