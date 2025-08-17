using System.Security.Claims;
using HeadStart.SharedKernel.Models.Authorization;
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
        return Ok(User.Identity.IsAuthenticated ? CreateUserInfo(User) : UserInfo.Anonymous);
    }

    private static UserInfo CreateUserInfo(ClaimsPrincipal claimsPrincipal)
    {
        if (!claimsPrincipal.Identity.IsAuthenticated)
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

        if (claimsPrincipal.Claims.Any())
        {
            var claims = new List<ClaimValue>();

            foreach (var claim in claimsPrincipal.FindAll(userInfo.NameClaimType))
            {
                claims.Add(new ClaimValue(userInfo.NameClaimType, claim.Value));
            }

            foreach (var claim in claimsPrincipal.FindAll(userInfo.EmailClaimType))
            {
                claims.Add(new ClaimValue(userInfo.EmailClaimType, claim.Value));
            }

#pragma warning disable S125 // Sections of code should not be commented out
            // Uncomment this code if you want to send additional claims to the client.
            //foreach (var claim in claimsPrincipal.Claims.Except(nameClaims))
            //{
            //    claims.Add(new ClaimValue(claim.Type, claim.Value));
            //}
#pragma warning restore S125 // Sections of code should not be commented out

            userInfo.Claims = claims;
        }

        return userInfo;
    }
}
