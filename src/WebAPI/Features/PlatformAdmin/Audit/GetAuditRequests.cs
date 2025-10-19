using FastEndpoints;
using Gridify;
using Gridify.EntityFramework;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.PlatformAdmin.Audit;

public static class GetAuditRequests
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
            Get("/audit/requests");
            Group<PlatformAdminEndpointGroup>();
            Version(1);

            Summary(s => {
                s.Summary = "Get audit requests with advanced filtering and sorting";
                s.Description = @"
                    Supports Gridify filtering syntax:
                    - Filter examples:
                      - requestMethod=POST
                      - requestPath=*/api/users*
                      - responseStatusCode>=400
                      - dateUtc>2024-01-01,dateUtc<2024-12-31
                      - userEmail=*@example.com
                      - (responseStatusCode=404|responseStatusCode=500)
                      - requestMethod=GET,responseStatusCode<300
                    - OrderBy examples:
                      - dateUtc desc (default)
                      - responseStatusCode asc, dateUtc desc
                      - requestMethod, requestPath
                      - userName desc, dateUtc desc";
            });
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            // Configure Gridify mapper with all available fields for filtering and sorting
            var gridifyMapper = new GridifyMapper<AuditRequest>(true) // true = auto-generate mappings
                .AddMap("userEmail", r => r.User != null ? r.User.Email : null)
                .AddMap("userName", r => r.User != null ? r.User.Nom + " " + r.User.Prenom : null)
                .AddMap("impersonatedByEmail", r => r.ImpersonatedByUser != null ? r.ImpersonatedByUser.Email : null)
                .AddMap("impersonatedByName", r => r.ImpersonatedByUser != null ? r.ImpersonatedByUser.Nom + " " + r.ImpersonatedByUser.Prenom : null)
                .AddMap("userId", r => r.UserId)
                .AddMap("impersonatedByUserId", r => r.ImpersonatedByUserId)
                .AddMap("dateUtc", r => r.DateUtc)
                .AddMap("requestId", r => r.RequestId)
                .AddMap("requestPath", r => r.RequestPath)
                .AddMap("requestQuery", r => r.RequestQuery)
                .AddMap("requestMethod", r => r.RequestMethod)
                .AddMap("responseStatusCode", r => r.ResponseStatusCode)
                .AddMap("statusCode", r => r.ResponseStatusCode) // Alias for convenience
                .AddMap("tenantPath", r => r.TenantPath)
                .AddMap("isImpersonated", r => r.ImpersonatedByUserId != null)
                .AddMap("hasError", r => r.ResponseStatusCode >= 400);

            // Base query with includes
            var query = DbContext.Requests
                .Include(r => r.User)
                .Include(r => r.ImpersonatedByUser)
                .AsNoTracking();

            // Set default ordering if not specified
            if (string.IsNullOrWhiteSpace(req.OrderBy))
            {
                req.OrderBy = "dateUtc desc";
            }

            // Apply Gridify with all its features (filtering, sorting, and paging)
            var paging = await query.GridifyAsync(req, ct, gridifyMapper);

            // Transform to DTOs
            var auditRequestDtos = paging.Data.Select(r => new AuditRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserEmail = r.User?.Email,
                UserName = r.User != null ? $"{r.User.Nom} {r.User.Prenom}" : null,
                ImpersonatedByUserId = r.ImpersonatedByUserId,
                ImpersonatedByUserEmail = r.ImpersonatedByUser?.Email,
                ImpersonatedByUserName = r.ImpersonatedByUser != null ? $"{r.ImpersonatedByUser.Nom} {r.ImpersonatedByUser.Prenom}" : null,
                IsImpersonated = r.ImpersonatedByUserId != null,
                DateUtc = r.DateUtc,
                RequestId = r.RequestId,
                RequestPath = r.RequestPath,
                RequestQuery = r.RequestQuery,
                RequestMethod = r.RequestMethod,
                RequestBody = r.RequestBody,
                ResponseStatusCode = r.ResponseStatusCode,
                TenantPath = r.TenantPath?.ToString(),
                HasError = r.ResponseStatusCode >= 400
            }).ToList();

            await Send.OkAsync(new Response
            {
                Data = auditRequestDtos,
                PageNumber = req.Page,
                PageSize = req.PageSize,
                TotalCount = paging.Count,
                TotalPages = (int)Math.Ceiling(paging.Count / (double)req.PageSize),
                HasPreviousPage = req.Page > 1,
                HasNextPage = req.Page < (int)Math.Ceiling(paging.Count / (double)req.PageSize),
                AppliedFilter = req.Filter,
                AppliedOrderBy = req.OrderBy
            }, ct);
        }
    }

    public record Response
    {
        public required List<AuditRequestDto> Data { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public bool HasPreviousPage { get; init; }
        public bool HasNextPage { get; init; }
        public string? AppliedFilter { get; init; }
        public string? AppliedOrderBy { get; init; }
    }

    public record AuditRequestDto
    {
        public Guid Id { get; init; }
        public Guid? UserId { get; init; }
        public string? UserEmail { get; init; }
        public string? UserName { get; init; }
        public Guid? ImpersonatedByUserId { get; init; }
        public string? ImpersonatedByUserEmail { get; init; }
        public string? ImpersonatedByUserName { get; init; }
        public bool IsImpersonated { get; init; }
        public DateTime DateUtc { get; init; }
        public required string RequestId { get; init; }
        public required string RequestPath { get; init; }
        public string? RequestQuery { get; init; }
        public required string RequestMethod { get; init; }
        public string? RequestBody { get; init; }
        public int? ResponseStatusCode { get; init; }
        public string? TenantPath { get; init; }
        public bool HasError { get; init; }
    }
}
