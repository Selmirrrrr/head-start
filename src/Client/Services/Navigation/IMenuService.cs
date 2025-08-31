using CleanArchitecture.Blazor.Server.UI.Models.NavigationMenu;

namespace HeadStart.Client.Services.Navigation;

public interface IMenuService
{
    IEnumerable<MenuSectionModel> Features { get; }
}
