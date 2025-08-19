using HeadStart.WebAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Data;

public class HeadStartDbContext : DbContext
{
    public HeadStartDbContext(DbContextOptions<HeadStartDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
}
