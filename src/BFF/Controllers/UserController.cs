using System.Security.Claims;
using HeadStart.SharedKernel.Models.Models.Authorization;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeadStart.BFF.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetCurrentUser()
    {
        return Ok(User.Identity?.IsAuthenticated == true ? CreateUserInfo(User) : UserInfo.Anonymous);
    }

    private static UserInfo CreateUserInfo(ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal.Identity is not { IsAuthenticated: true })
        {
            return UserInfo.Anonymous;
        }

        var userInfo = new UserInfo
        {
            IsAuthenticated = true
        };

        if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            userInfo.NameClaimType = claimsIdentity.NameClaimType;
            userInfo.RoleClaimType = claimsIdentity.RoleClaimType;
        }
        else
        {
            userInfo.NameClaimType = JwtClaimTypes.Name;
            userInfo.RoleClaimType = JwtClaimTypes.Role;
        }

        userInfo.EmailClaimType = JwtClaimTypes.Email;

        if (!claimsPrincipal.Claims.Any()) return userInfo;
        
        var claims = claimsPrincipal.FindAll(userInfo.NameClaimType).Select(claim => new ClaimValue(userInfo.NameClaimType, claim.Value)).ToList();

        claims.AddRange(claimsPrincipal.FindAll(userInfo.EmailClaimType).Select(claim => new ClaimValue(userInfo.EmailClaimType, claim.Value)));

        claims.AddRange(claimsPrincipal.Claims.Select(claim => new ClaimValue(claim.Type, claim.Value)));

        userInfo.Claims = claims;

        return userInfo;
    }
}
