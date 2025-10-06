using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HeadStart.IntegrationTests.Helpers;

public static class DbContextHelper
{
    /// <summary>
    /// Crée un DbContext avec les bonnes mappings pour LTree. Sans ça, les tests ne passent pas.
    /// </summary>
    /// <param name="connectionString">La connection string à la base de données.</param>
    /// <returns>Un DbContext avec les bonnes mappings pour LTree.</returns>
    public static async Task<HeadStartDbContext> CreateDbContextAsync(string connectionString)
    {
        // Create a data source with proper type mappings for LTree
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableUnmappedTypes();
        var dataSource = dataSourceBuilder.Build();

        // Create DbContextOptions properly with the configured data source
        var optionsBuilder = new DbContextOptionsBuilder<HeadStartDbContext>();
        optionsBuilder.UseNpgsql(dataSource);

        // Create a db context instance with the connection string
        var dbContext = new HeadStartDbContext(optionsBuilder.Options, new CurrentUserServiceMock());

        // Ensure the ltree extension is installed
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync($"CREATE EXTENSION IF NOT EXISTS ltree");
        }
        catch
        {
            // Extension might already exist, which is fine
        }
        await dbContext.Database.EnsureCreatedAsync();
        return dbContext;
    }
}

public class CurrentUserServiceMock : ICurrentUserService
{
    public Guid UserId { get; } = Users.AdminApiTest1.Id;
    public bool IsAuthenticated { get; } = true;
    public string Email { get; } = Users.AdminApiTest1.UserEmail;
    public string GivenName { get; } = Users.AdminApiTest1.UserFirstName;
    public string Surname { get; } = Users.AdminApiTest1.UserLastName;
}
