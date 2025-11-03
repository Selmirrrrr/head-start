using Gridify;
using HeadStart.WebAPI.Data.Models;

namespace HeadStart.WebAPI.Infrastructure;

/// <summary>
/// Configures Gridify mappings for entities
/// This allows custom property mappings and nested property filtering
/// </summary>
public static class GridifyConfiguration
{
    /// <summary>
    /// Configure all Gridify mappings for the application
    /// Call this during application startup if custom mappings are needed
    /// </summary>
    public static void ConfigureMappings()
    {
        // Example: Configure custom mappings for AuditTrail
        // This is optional - Gridify works out of the box for most cases
        // But you can add custom mappings for nested properties, calculated fields, etc.

        // var auditTrailMapper = new GridifyMapper<AuditTrail>()
        //     .AddMap("UserName", a => a.User!.Email) // Map UserName to nested User.Email
        //     .AddMap("UserEmail", a => a.User!.Email);

        // GlobalConfiguration.CustomMappings.Add(typeof(AuditTrail), auditTrailMapper);
    }

    /// <summary>
    /// Gets a default GridifyMapper for a specific entity type
    /// Use this to create custom mappings for specific queries
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>A GridifyMapper instance</returns>
    public static IGridifyMapper<T> GetMapper<T>()
    {
        var mapper = new GridifyMapper<T>(autoGenerateMappings: true);

        // Add entity-specific custom mappings
        if (typeof(T) == typeof(AuditTrail))
        {
            var auditMapper = mapper as GridifyMapper<AuditTrail>;
            // Custom mappings can be added here
            // auditMapper?.AddMap("custom", a => a.SomeProperty);
        }

        return (IGridifyMapper<T>)mapper;
    }
}

/// <summary>
/// Extension methods for Gridify to make it easier to use with FastEndpoints
/// </summary>
public static class GridifyExtensions
{
    /// <summary>
    /// Converts a Gridify GridifyResult to a GridifyPagedResponse
    /// </summary>
    public static SharedKernel.Models.Models.GridifyPagedResponse<TDto> ToPagedResponse<TEntity, TDto>(
        this Paging<TEntity> gridifyResult,
        Func<TEntity, TDto> mapper,
        int page,
        int pageSize)
    {
        return new SharedKernel.Models.Models.GridifyPagedResponse<TDto>
        {
            Data = gridifyResult.Data.Select(mapper).ToList(),
            TotalCount = gridifyResult.Count,
            Page = page - 1, // Convert to 0-based for frontend
            PageSize = pageSize
        };
    }
}
