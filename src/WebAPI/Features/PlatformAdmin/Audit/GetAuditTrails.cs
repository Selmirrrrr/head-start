using FastEndpoints;
using Gridify;
using Gridify.EntityFramework;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.PlatformAdmin.Audit;

public static class GetAuditTrails
{
    public class Request : IGridifyQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Filter { get; set; }
        public string? OrderBy { get; set; }
    }

    public class Endpoint : Endpoint<Request, Response>
    {
        public required HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Get("/audit/trails");
            Group<PlatformAdminEndpointGroup>();
            Version(1);

            Summary(s => {
                s.Summary = "Get audit trails with advanced filtering and sorting";
                s.Description = @"
                    Supports Gridify filtering syntax:
                    - Filter examples:
                      - type=Create
                      - entityName=*User
                      - dateUtc>2024-01-01
                      - userEmail=*@example.com
                      - (type=Update|type=Delete),entityName=Order
                    - OrderBy examples:
                      - dateUtc desc
                      - entityName asc, dateUtc desc
                      - userName, type desc";
            });
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            // Configure Gridify mapper for complex navigation properties and computed fields
            var gridifyMapper = new GridifyMapper<AuditTrail>(true) // true = auto-generate mappings for simple properties
                .AddMap("userEmail", a => a.User != null ? a.User.Email : null)
                .AddMap("userName", a => a.User != null ? a.User.Nom + " " + a.User.Prenom : null)
                .AddMap("userId", a => a.UserId)
                .AddMap("type", a => a.Type)
                .AddMap("dateUtc", a => a.DateUtc)
                .AddMap("entityName", a => a.EntityName)
                .AddMap("primaryKey", a => a.PrimaryKey)
                .AddMap("traceId", a => a.TraceId);

            // Base query with includes
            var query = DbContext.AuditTrails
                .Include(a => a.User)
                .AsNoTracking();

            // Apply Gridify with all its features (filtering, sorting, and paging)
            var paging = await query.GridifyAsync(req, ct, gridifyMapper);

            // Transform to DTOs
            var auditTrailDtos = paging.Data.Select(a => new AuditTrailDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.User?.Email,
                UserName = a.User != null ? $"{a.User.Nom} {a.User.Prenom}" : null,
                TraceId = a.TraceId,
                Type = a.Type,
                DateUtc = a.DateUtc,
                EntityName = a.EntityName,
                PrimaryKey = a.PrimaryKey,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                ChangedColumns = a.ChangedColumns
            }).ToList();

            await Send.OkAsync(new Response
            {
                Data = auditTrailDtos,
                PageNumber = req.Page,
                PageSize = req.PageSize,
                TotalCount = paging.Count,
                TotalPages = (int)Math.Ceiling(paging.Count / (double)req.PageSize),
                HasPreviousPage = req.Page > 1,
                HasNextPage = req.Page < (int)Math.Ceiling(paging.Count / (double)req.PageSize),
                AppliedFilter = req.Filter,
                AppliedOrderBy = req.OrderBy ?? "dateUtc desc"
            }, ct);
        }
    }

    public record Response
    {
        public required List<AuditTrailDto> Data { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public bool HasPreviousPage { get; init; }
        public bool HasNextPage { get; init; }
        public string? AppliedFilter { get; init; }
        public string? AppliedOrderBy { get; init; }
    }

    public record AuditTrailDto
    {
        public Guid Id { get; init; }
        public Guid? UserId { get; init; }
        public string? UserEmail { get; init; }
        public string? UserName { get; init; }
        public string? TraceId { get; init; }
        public TrailType Type { get; init; }
        public DateTime DateUtc { get; init; }
        public required string EntityName { get; init; }
        public string? PrimaryKey { get; init; }
        public Dictionary<string, object?> OldValues { get; init; } = new();
        public Dictionary<string, object?> NewValues { get; init; } = new();
        public List<string> ChangedColumns { get; init; } = new();
    }
}
