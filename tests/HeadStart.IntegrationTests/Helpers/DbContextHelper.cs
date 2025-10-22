using HeadStart.SharedKernel.Models.Constants;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Services;
using Microsoft.AspNetCore.Http;
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

        // Create a mock HttpContextAccessor with a service provider that can resolve ICurrentUserService
        var httpContextAccessor = new HttpContextAccessorMock();

        // Create a db context instance with the connection string
        var dbContext = new HeadStartDbContext(optionsBuilder.Options, httpContextAccessor);

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

public class HttpContextAccessorMock : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }

    public HttpContextAccessorMock()
    {
        // Create a service collection and register the mock CurrentUserService
        var services = new ServiceCollection();
        services.AddScoped<ICurrentUserService, CurrentUserServiceMock>();
        var serviceProvider = services.BuildServiceProvider();

        // Create a mock HttpContext with the service provider
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider.CreateScope().ServiceProvider
        };

        HttpContext = httpContext;
    }
}

public class CurrentUserServiceMock : ICurrentUserService
{
    public Guid UserId { get; } = Users.AdminApiTest1.Id;
    public string? SelectedTenantPath { get; }
    public bool IsAuthenticated { get; } = true;
    public bool IsImpersonated { get; }
    public Guid? ImpersonatedByUserId { get; }
    public string Email { get; } = Users.AdminApiTest1.UserEmail;
    public string GivenName { get; } = Users.AdminApiTest1.UserFirstName;
    public string Surname { get; } = Users.AdminApiTest1.UserLastName;
    public string[] PlatformRoles { get; } = [RoleName.PlatformAdmin];
}
