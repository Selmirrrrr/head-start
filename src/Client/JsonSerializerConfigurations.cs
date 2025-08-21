using System.Text.Json;
using System.Text.Json.Serialization;

namespace HeadStart.Client;

/// <summary>
/// Provides commons JSON serializer configurations.
/// </summary>
public static class JsonSerializerConfigurations
{
    /// <summary>
    /// The default set of JSON serializer options.
    /// </summary>
    /// <returns>An instance of <see cref="JsonSerializerOptions"/>.</returns>
    public static JsonSerializerOptions Default => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// The JSON serializer options, used for Blazor's local storage
    /// operations.
    /// </summary>
    /// <returns>An instance of <see cref="JsonSerializerOptions"/>.</returns>
    public static JsonSerializerOptions LocalStorageSettings => new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = false
    };

    /// <summary>
    /// The JSON serializer options, used for Blazor's logging
    /// operations.
    /// </summary>   
    /// <returns>An instance of <see cref="JsonSerializerOptions"/>.</returns>
    public static JsonSerializerOptions LoggingSettings => new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };
}
