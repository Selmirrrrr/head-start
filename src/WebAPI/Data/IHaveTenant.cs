using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Data;

public interface IHaveTenant
{
    LTree TenantPath { get; set; }
}

public interface IMayHaveTenant
{
    LTree? TenantPath { get; set; }
}

