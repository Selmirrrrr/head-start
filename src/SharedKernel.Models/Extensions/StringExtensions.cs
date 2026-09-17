namespace HeadStart.SharedKernel.Models.Extensions;

public static class StringExtensions
{
    public static string? SanitizeForLogging(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? value : value.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", "");
    }
}
