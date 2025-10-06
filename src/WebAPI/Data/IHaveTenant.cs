using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Data;

public interface IHaveTenant
{
    LTree TenantPath { get; }
}

public interface IMayHaveTenant
{
    LTree? TenantPath { get; }
}

