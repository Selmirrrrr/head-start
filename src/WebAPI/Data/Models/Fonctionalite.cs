using HeadStart.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Data.Models;

public class Fonctionalite : IMayHaveTenant, IAuditable
{
    public required LTree Code { get; set; } = null!;
    public required string Libelle { get; set; } = null!;
    public required Dictionary<string, string> LibelleTrads { get; set; }

    public LTree? TenantPath { get; set; }
    public Tenant? Tenant { get; set; }

    public Audit Audit { get; set; } = new Audit();
}
