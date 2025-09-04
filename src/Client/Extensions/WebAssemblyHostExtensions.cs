using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace HeadStart.Client.Extensions;

public static class WebAssemblyHostExtensions
{
    public static async Task SetDefaultUICulture(this WebAssemblyHost host)
    {
        var localStorage = host.Services.GetRequiredService<ILocalStorageService>();

        var result = await localStorage.GetItemAsync<string>("currentCulture");
        var culture =
            result != null ? new CultureInfo(result) :
                new CultureInfo("fr");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
