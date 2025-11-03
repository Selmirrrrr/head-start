using FastEndpoints;
using Gridify;
using HeadStart.SharedKernel.Models.Models;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using HeadStart.WebAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.PlatformAdmin;

/// <summary>
/// Get a paginated, filtered, and sorted list of audit trails
/// This endpoint demonstrates the standard pattern for data grid endpoints with Gridify
/// </summary>
/// <remarks>
/// IMPLEMENTATION GUIDE for similar endpoints:
///
/// 1. Request class should have: Page, PageSize, Filter, OrderBy (see Request below)
/// 2. Response should use GridifyPagedResponse&lt;TDto&gt; from SharedKernel.Models
/// 3. Build your base query with Include() for related entities
/// 4. Create GridifyQuery from request parameters
/// 5. Use GridifyQueryableAsync() to apply filtering, sorting, and pagination
/// 6. Map results to DTOs
/// 7. Use ToPagedResponse() extension method to create response
///
/// FRONTEND INTEGRATION:
/// - MudDataGrid ServerData function receives GridState&lt;T&gt;
/// - Map GridState.SortDefinitions to Gridify OrderBy syntax: "PropertyName asc|desc"
/// - Map filters to Gridify Filter syntax: "PropertyName operator value"
/// - Page is 0-based in MudDataGrid but 1-based in Gridify (add 1 when calling API)
///
/// GRIDIFY FILTER EXAMPLES:
/// - Equals: "EntityName = Tenant"
/// - Contains: "EntityName @= Tenant" or "EntityName @=* Tenant" (case-insensitive)
/// - Greater than: "DateUtc > 2024-01-01"
/// - Multiple conditions: "EntityName = Tenant, DateUtc > 2024-01-01"
/// - OR conditions: "EntityName = Tenant | EntityName = User"
/// </remarks>
public static class AuditTrailsGetList
{
    public class Endpoint : Endpoint<Request, GridifyPagedResponse<AuditTrailDto>>
    {
        public required HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Get("/platform-admin/audit-trails");
            Version(1);
            // TODO: Add authorization - should only be accessible to platform admins
            // Policies(PolicyNames.PlatformAdmin);
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            // Build the query with User navigation property included
            var query = DbContext.AuditTrails
                .Include(a => a.User)
                .AsNoTracking();

            // Build Gridify query from request parameters
            var gridifyQuery = new GridifyQuery
            {
                Page = req.Page ?? 1,
                PageSize = req.PageSize ?? 10,
                Filter = req.Filter,
                OrderBy = req.OrderBy ?? "DateUtc desc" // Default sort by most recent
            };

            // Apply Gridify filtering, sorting, and pagination
            var result = await query.GridifyQueryableAsync(gridifyQuery, ct);

            // Map to DTOs and create response using extension method
            var response = result.ToPagedResponse(
                MapToDto,
                gridifyQuery.Page,
                gridifyQuery.PageSize
            );

            await SendOkAsync(response, ct);
        }

        private static AuditTrailDto MapToDto(AuditTrail auditTrail)
        {
            return new AuditTrailDto
            {
                Id = auditTrail.Id,
                UserId = auditTrail.UserId,
                UserName = auditTrail.User?.Email,
                Type = auditTrail.Type.ToString(),
                DateUtc = auditTrail.DateUtc,
                EntityName = auditTrail.EntityName,
                PrimaryKey = auditTrail.PrimaryKey,
                OldValues = auditTrail.OldValues,
                NewValues = auditTrail.NewValues,
                ChangedColumns = auditTrail.ChangedColumns
            };
        }
    }

    /// <summary>
    /// Request parameters for filtering, sorting, and pagination
    /// These map directly to Gridify query parameters
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int? Page { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Filter expression (Gridify syntax)
        /// Example: "EntityName = Tenant, DateUtc > 2024-01-01"
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// Sort expression (Gridify syntax)
        /// Example: "DateUtc desc, EntityName asc"
        /// </summary>
        public string? OrderBy { get; set; }
    }

    /// <summary>
    /// AuditTrail DTO for API responses
    /// </summary>
    public record AuditTrailDto
    {
        public required Guid Id { get; init; }
        public Guid? UserId { get; init; }
        public string? UserName { get; init; }
        public required string Type { get; init; }
        public required DateTime DateUtc { get; init; }
        public required string EntityName { get; init; }
        public string? PrimaryKey { get; init; }
        public required Dictionary<string, object?> OldValues { get; init; }
        public required Dictionary<string, object?> NewValues { get; init; }
        public required List<string> ChangedColumns { get; init; }
    }
}
