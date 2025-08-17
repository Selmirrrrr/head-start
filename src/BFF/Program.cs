using CorrelationId;
using HeadStart.Aspire.ServiceDefaults;
using HeadStart.BFF.Extensions;
using HeadStart.SharedKernel.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Serilog
builder.Host.UseSerilog((builderContext, loggerConfig) =>
    loggerConfig.ConfigureFromSettings(builderContext.Configuration));

// Add services
builder.Services.AddSharedKernelServices();
builder.Services.AddApiServices(builder.Configuration, builder.Environment.IsDevelopment());

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
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
    app.MapReverseProxy();
    app.MapFallbackToPage("/_Host");

    app.Run();
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
    Log.CloseAndFlush();
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
