using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Data;

public class HeadStartDbContext : DbContext
{
    public HeadStartDbContext(DbContextOptions<HeadStartDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
}

public class Tenant
{
    public Guid Id { get; set; }

    [StringLength(250)]
    public string Name { get; set; } = string.Empty;
}
