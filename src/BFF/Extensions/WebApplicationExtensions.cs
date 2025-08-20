using CorrelationId;
using HeadStart.BFF.Utilities;
using NetEscapades.AspNetCore.SecurityHeaders;
using Serilog;

namespace HeadStart.BFF.Extensions;

public static class WebApplicationExtensions
{
    public static void ConfigureBffPipeline(this WebApplication app)
    {
        app.ConfigureExceptionHandling();
        app.ConfigureSecurityMiddleware();
        app.ConfigureRequestProcessing();
        app.ConfigureRoutingAndEndpoints();
    }

    private static void ConfigureExceptionHandling(this WebApplication app)
    {
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
    }

    private static void ConfigureSecurityMiddleware(this WebApplication app)
    {
        app.UseSecurityHeaders(GetSecurityHeaderPolicy(app.Environment.IsDevelopment(), "http://localhost:8080"));
        app.UseHttpsRedirection();
    }

    private static void ConfigureRequestProcessing(this WebApplication app)
    {
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseCorrelationId();
        app.UseSerilogRequestLogging();
        app.UseRouting();
        app.UseRateLimiter();
    }

    private static void ConfigureRoutingAndEndpoints(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStatusCodePages();
        app.MapRazorPages();
        app.MapControllers();
        app.MapReverseProxy();
        app.MapFallbackToPage("/_Host");
    }

    private static HeaderPolicyCollection GetSecurityHeaderPolicy(bool isDev, string idpHost)
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
}
