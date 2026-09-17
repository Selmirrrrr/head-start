using HeadStart.SharedKernel.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace HeadStart.SharedKernel.Logging;

public class UserDataLoggingMiddleware(RequestDelegate next, ILogger<UserDataLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context, ICurrentUserService currentUser)
    {
        if (currentUser.IsAuthenticated)
        {
            var props = new Dictionary<string, object> { ["UserId"] = currentUser.UserId };

            if (currentUser.SelectedTenantPath != null)
            {
                logger.LogInformation("User selected tenant: {TenantPath}", currentUser.SelectedTenantPath);
                props.Add("TenantPath", currentUser.SelectedTenantPath);
            }

            if (currentUser is { IsImpersonated: true, ImpersonatedByUserId: not null })
            {
                props.Add("ImpersonatedByUserId", currentUser.ImpersonatedByUserId);
            }

            using (logger.BeginScope(props))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }
}
