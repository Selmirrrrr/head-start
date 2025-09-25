using HeadStart.SharedKernel.Models.NavigationMenu;

namespace HeadStart.Client.Services.Navigation;

public interface IMenuService
{
    IEnumerable<MenuSectionModel> Features { get; }
}
