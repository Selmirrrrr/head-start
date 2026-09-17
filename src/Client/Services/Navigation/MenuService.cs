using HeadStart.SharedKernel.Models.Constants;
using HeadStart.SharedKernel.Models.NavigationMenu;
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
                    Title = "Analytics",
                    Icon = Icons.Material.Filled.Analytics,
                    Href = "/analytics",
                    PageStatus = PageStatus.ComingSoon
                },
                new()
                {
                    Title = "Planning",
                    Icon = Icons.Material.Filled.CalendarToday,
                    Href = "/planning",
                    PageStatus = PageStatus.ComingSoon
                },
                new()
                {
                    Title = "Admin",
                    Roles = [RoleName.PlatformAdmin],
                    Icon = Icons.Material.Filled.Security,
                    Href = "/admin",
                    PageStatus = PageStatus.Completed,
                    IsParent = true,
                    MenuItems = (List<MenuSectionSubItemModel>)
                    [
                        new() { Title = "Tenants", Href = "/platform-admin/tenants", PageStatus = PageStatus.Completed, Roles = [RoleName.PlatformAdmin]},
                        new() { Title = "Data audit trail", Href = "/platform-admin/audit/data", PageStatus = PageStatus.Completed, Roles = [RoleName.PlatformAdmin]},
                        new() { Title = "Requests audit trail", Href = "/platform-admin/audit/request", PageStatus = PageStatus.Completed, Roles = [RoleName.PlatformAdmin]}
                    ]
                }
            }
        },
    ];

    public IEnumerable<MenuSectionModel> Features => _features;
}
