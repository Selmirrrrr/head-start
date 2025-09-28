using System.Diagnostics;
using HeadStart.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.IntegrationTests;

public static class Hooks
{
    [Before(TestSession)]
    public static void InstallPlaywright()
    {
        if (Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("PWDEBUG", "1");
        }

        Microsoft.Playwright.Program.Main(["install"]);
    }

    [Before(TestSession)]
    public static async Task ResetDatabaseAsync()
    {
        var connectionString = await GlobalSetup.App!.GetConnectionStringAsync("postgresdb");

        // Create DbContextOptions properly
        var optionsBuilder = new DbContextOptionsBuilder<HeadStartDbContext>();
        optionsBuilder.UseNpgsql(connectionString!);

        // Create a db context instance with the connection string
        await using var dbContext = new HeadStartDbContext(optionsBuilder.Options);

        // TODO: Reset the database
    }
}
