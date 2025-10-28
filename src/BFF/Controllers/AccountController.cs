using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HeadStart.BFF.Controllers;

[Route("api/[controller]")]
public class AccountController(ILogger<AccountController> logger) : ControllerBase
{
    [HttpGet("Login")]
    [EnableRateLimiting("login")]
    public ActionResult Login(string returnUrl)
    {
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
