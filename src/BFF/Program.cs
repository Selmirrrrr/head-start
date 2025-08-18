using CorrelationId;
using HeadStart.Aspire.ServiceDefaults;
using HeadStart.BFF.Extensions;
using HeadStart.SharedKernel.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Serilog
builder.AddSeqEndpoint(connectionName: "seq");
builder.Host.UseSerilog((builderContext, loggerConfig) =>
    loggerConfig.ConfigureWebApplicationLogging(
        builderContext.Configuration,
        builderContext.HostingEnvironment,
        "HeadStart.BFF"));

// Add services
builder.Services.AddSharedKernelServices();
builder.Services.AddApiServices(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddDataProtection(o => o.ApplicationDiscriminator = "HeadStart");

// Add rate limiting for authentication endpoints
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Add rate limiter for authentication endpoints
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(30);
        limiterOptions.PermitLimit = 5;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    // Add a stricter rate limiter for login attempts
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(30);
        limiterOptions.PermitLimit = 10;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        if (activity != null)
        {
            context.ProblemDetails.Extensions.TryAdd("traceId", activity.Id);
        }
    };
});

var app = builder.Build();

app.MapDefaultEndpoints();

try
{
    // Configure middleware
    app.UseResponseCompression();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseSecurityHeaders(GetSecurityHeaderPolicy(app.Environment.IsDevelopment(), "http://localhost:8080"));
    app.UseHttpsRedirection();
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    app.UseCorrelationId();
    app.UseSerilogIngestion();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStatusCodePages();
    app.MapRazorPages();
    app.MapControllers();
    app.MapReverseProxy();
    app.MapFallbackToPage("/_Host");

    await app.RunAsync();
}
catch (Exception ex)
{
    if (Log.Logger.GetType().Name == "SilentLogger")
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }

    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Shut down complete");
    await Log.CloseAndFlushAsync();
}

static HeaderPolicyCollection GetSecurityHeaderPolicy(bool isDev, string idpHost)
{
    var policy = new HeaderPolicyCollection()
        .AddFrameOptionsDeny()
        .AddXssProtectionBlock()
        .AddContentTypeOptionsNoSniff()
        .AddReferrerPolicyStrictOriginWhenCrossOrigin()
        .AddCrossOriginOpenerPolicy(builder => builder.SameOrigin())
        .AddCrossOriginResourcePolicy(builder => builder.SameOrigin())
        .AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp())
        .AddContentSecurityPolicy(builder =>
        {
            builder.AddObjectSrc().None();
            builder.AddBlockAllMixedContent();
            builder.AddImgSrc().Self().From("data:");
            // Allow form actions to self and identity provider
            builder.AddFormAction().Self().From(idpHost);
            builder.AddFontSrc().Self();
            builder.AddStyleSrc().Self().UnsafeInline();
            builder.AddBaseUri().Self();
            builder.AddFrameAncestors().None();

            builder.AddScriptSrc()
                .Self()
                .WithHash256("v8v3RKRPmN4odZ1CWM5gw80QKPCCWMcpNeOmimNL2AA=")
                .UnsafeEval();
        });

    if (!isDev)
    {
        policy.AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365);
    }

    return policy;
}
