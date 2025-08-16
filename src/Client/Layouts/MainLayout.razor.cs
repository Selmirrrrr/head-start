using Microsoft.AspNetCore.Components;

namespace HeadStart.Client.Layouts;

public partial class MainLayout
{
    public string FooterCopyrightContent { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        FooterCopyrightContent = $"Vasil Kotsev, Copyright Ⓒ {DateTimeOffset.Now.Year}";
    }
}
