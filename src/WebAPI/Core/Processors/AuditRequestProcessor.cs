using FastEndpoints;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using HeadStart.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Core.Processors;

/// <summary>
/// Global processor that audits all HTTP requests and responses.
/// Captures request details and stores them in the AuditRequest table.
/// </summary>
public class AuditRequestProcessor : IGlobalPostProcessor
{
    public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        // Skip auditing for health checks and swagger endpoints in production
        if (ShouldSkipAuditing(context.HttpContext))
        {
            return;
        }

        var serviceProvider = context.HttpContext.RequestServices;
        var dbContext = serviceProvider.GetRequiredService<HeadStartDbContext>();
        var currentUserService = serviceProvider.GetService<ICurrentUserService>();
        var logger = serviceProvider.GetRequiredService<ILogger<AuditRequestProcessor>>();

        try
        {
            var auditRequest = await CreateAuditRequestAsync(context.HttpContext, currentUserService);

            dbContext.Requests.Add(auditRequest);
            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation("Audit request created for {Method} {Path}", context.HttpContext.Request.Method, context.HttpContext.Request.Path);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the request
            logger.LogError(ex, "Failed to create audit request for {Method} {Path}", context.HttpContext.Request.Method, context.HttpContext.Request.Path);
        }
    }

    private static async Task<AuditRequest> CreateAuditRequestAsync(HttpContext httpContext, ICurrentUserService? currentUserService)
    {
        var request = httpContext.Request;
        var user = httpContext.User;

        // Check if user is authenticated
        Guid? userId = null;
        if (user.Identity?.IsAuthenticated == true && currentUserService != null)
        {
            userId = currentUserService.UserId;
        }

        var impersonatedByUserId = currentUserService?.IsImpersonated == true
            ? currentUserService.ImpersonatedByUserId
            : null;

        // Get tenant path from header or current user service
        var tenantPath = currentUserService?.SelectedTenantPath;
        if (string.IsNullOrEmpty(tenantPath))
        {
            tenantPath = httpContext.Request.Headers["X-Tenant-Path"].ToString();
        }

        // Try to get response status code from HttpContext
        int? responseStatusCode = null;
        if (httpContext.Response?.StatusCode > 0)
        {
            responseStatusCode = httpContext.Response.StatusCode;
        }

        return new AuditRequest
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            ImpersonatedByUserId = impersonatedByUserId,
            DateUtc = DateTime.UtcNow,
            RequestId = httpContext.TraceIdentifier,
            RequestPath = request.Path.Value ?? string.Empty,
            RequestMethod = request.Method,
            RequestBody = await GetRequestBodyAsync(request),
            ResponseStatusCode = responseStatusCode,
            RequestQuery = request.QueryString.Value,
            TenantPath = string.IsNullOrWhiteSpace(tenantPath) ? null! : new LTree(tenantPath)
        };
    }

    private static async Task<string?> GetRequestBodyAsync(HttpRequest request)
    {
        try
        {
            // Don't read large request bodies or certain content types
            if (request.ContentLength > 1024 * 1024) // 1MB limit
            {
                return "[Request body too large to audit]";
            }

            // Only process JSON content types
            var contentType = request.ContentType?.ToLowerInvariant();
            if (string.IsNullOrEmpty(contentType) || !contentType.Contains("application/json"))
            {
                return null;
            }

            // For GET requests, there's no body
            if (string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Try to read the request body
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            // Truncate very large bodies
            if (body.Length is > 5000 and > 0)
            {
                body = body[..5000] + "...[truncated]";
            }

            return string.IsNullOrEmpty(body) ? null : body;
        }
        catch (Exception)
        {
            // If we can't read the body, don't fail the request
            return "[Failed to read request body]";
        }
    }

    private static bool ShouldSkipAuditing(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Skip health checks
        if (path.Contains("/health"))
        {
            return true;
        }

        // Skip static files
        return path.StartsWith("/wwwroot/");
    }
}
