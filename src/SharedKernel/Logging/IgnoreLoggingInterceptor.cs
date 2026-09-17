using Microsoft.AspNetCore.HttpLogging;

namespace HeadStart.SharedKernel.Logging;

public class IgnoreLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        var path = logContext.HttpContext.Request.Path.Value;

        if (path?.StartsWith("/api", StringComparison.OrdinalIgnoreCase) != false)
        {
            logContext.LoggingFields = HttpLoggingFields.RequestPath |
                                       HttpLoggingFields.RequestMethod |
                                       HttpLoggingFields.RequestQuery |
                                       HttpLoggingFields.RequestBody |
                                       HttpLoggingFields.ResponseStatusCode;
        }
        else
        {
            logContext.LoggingFields = HttpLoggingFields.None;
        }

        return default;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        return default;
    }
}
