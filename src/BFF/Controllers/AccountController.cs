using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HeadStart.BFF.Controllers;

[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    [HttpGet("Login")]
    [EnableRateLimiting("login")]
    public ActionResult Login(string returnUrl)
    {
        _logger.LogInformation("Login attempt from IP: {IpAddress}", HttpContext.Connection.RemoteIpAddress);
        _logger.LogInformation("Return URL: {ReturnUrl}", returnUrl);
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/"
        });
    }

    [ValidateAntiForgeryToken]
    [Authorize]
    [HttpPost("Logout")]
    [EnableRateLimiting("auth")]
    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }
}
