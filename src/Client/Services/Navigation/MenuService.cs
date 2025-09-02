using CleanArchitecture.Blazor.Server.UI.Models.NavigationMenu;
using HeadStart.SharedKernel.Models.Constants;
using MudBlazor;

namespace HeadStart.Client.Services.Navigation;

public class MenuService : IMenuService
{
    private readonly List<MenuSectionModel> _features =
    [
        new()
        {
            Title = "Application",
            SectionItems = new List<MenuSectionItemModel>
            {
                new() { Title = "Home", Icon = Icons.Material.Filled.Home, Href = "/" },
                new()
                {
                    Title = "Claims",
                    Icon = Icons.Material.Filled.LocalHospital,
                    PageStatus = PageStatus.Completed,
                    IsParent = true,
                    MenuItems = new List<MenuSectionSubItemModel>
                    {
                        new() { Title = "Absences", Href = "/pages/products", PageStatus = PageStatus.Completed },
                        new() { Title = "Documents", Href = "/pages/documents", PageStatus = PageStatus.Completed },
                        new() { Title = "Contacts", Href = "/pages/contacts", PageStatus = PageStatus.Completed }
                    }
                },
                new()
                {
                    Title = "Tenants",
                    Roles = [RoleName.Admin, RoleName.Users],
                    Icon = Icons.Material.Filled.Compare,
                    Href = "/tenants",
                    PageStatus = PageStatus.Completed
                },
                new()
                {
                    Title = "Analytics",
                    Roles = [RoleName.Admin, RoleName.Users],
                    Icon = Icons.Material.Filled.Analytics,
                    Href = "/analytics",
                    PageStatus = PageStatus.ComingSoon
                },
                new()
                {
                    Title = "Planning",
                    Roles = [RoleName.Admin, RoleName.Users],
                    Icon = Icons.Material.Filled.CalendarToday,
                    Href = "/planning",
                    PageStatus = PageStatus.ComingSoon
                }
            }
        },
    ];

    public IEnumerable<MenuSectionModel> Features => _features;
}
