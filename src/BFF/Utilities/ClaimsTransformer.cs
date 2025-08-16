using System.Security.Claims;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;

namespace HeadStart.BFF.Utilities;

/// <summary>
/// Transforms WS Federation claim types to OpenIDdict ones.
/// </summary>
public sealed class ClaimsTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimsIdentity = principal.Identity as ClaimsIdentity;

        Guard.Against.Null(claimsIdentity);

        var claimsMap = new Dictionary<string, string>()
        {
            { ClaimTypes.Email, OpenIddictConstants.Claims.Email}
        };

        foreach (var claimMap in claimsMap)
        {
            var foundClaim = principal.FindFirst(claimMap.Key);
            if (foundClaim != null && claimsIdentity.TryRemoveClaim(foundClaim))
            {
                claimsIdentity.AddClaim(new Claim(claimMap.Value, foundClaim.Value));
            }
        }

        return Task.FromResult(principal);
    }
}
