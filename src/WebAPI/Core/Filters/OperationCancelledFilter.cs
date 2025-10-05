namespace HeadStart.WebAPI.Core.Filters;

internal sealed class OperationCancelledFilter(ILogger<OperationCancelledFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (OperationCanceledException x)
        {
            logger.LogDebug(x, "Request was cancelled!");
            return Results.StatusCode(499);
        }
    }
}
