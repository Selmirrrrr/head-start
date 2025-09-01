using System.Security.Claims;
using FastEndpoints;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Users;

public static class OnboardUser
{
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        public required HeadStartDbContext DbContext { get; set; }
        public required IKeycloakService KeycloakService { get; set; }

        public override void Configure()
        {
            Post("/users/onboard");
            Version(1);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(keycloakId))
            {
                await SendUnauthorizedAsync(ct);
                return;
            }

            // Check if user already exists with this Keycloak ID
            var existingUser = await DbContext.Users
                .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, ct);

            if (existingUser != null)
            {
                await SendOkAsync(new Response
                {
                    UserId = existingUser.Id,
                    IsNewUser = false,
                    Message = "User already exists"
                }, ct);
                return;
            }

            // Check if user has headStartId claim (for backward compatibility)
            var headStartIdClaim = User.FindFirstValue("headStartId");
            if (!string.IsNullOrEmpty(headStartIdClaim) && Guid.TryParse(headStartIdClaim, out var existingId))
            {
                existingUser = await DbContext.Users.FindAsync([existingId], ct);
                if (existingUser != null)
                {
                    await SendOkAsync(new Response
                    {
                        UserId = existingUser.Id,
                        IsNewUser = false,
                        Message = "User already exists"
                    }, ct);
                    return;
                }
            }

            // Create new user
            var newUserId = Guid.CreateVersion7();
            var email = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
            var lastName = User.FindFirstValue(ClaimTypes.Surname) ?? "unknown";
            var firstName = User.FindFirstValue(ClaimTypes.GivenName) ?? "unknown";

            var newUser = new User
            {
                Id = newUserId,
                KeycloakId = keycloakId,
                Email = email,
                Nom = lastName,
                Prenom = firstName,
                CreatedAt = DateTime.UtcNow
            };

            DbContext.Users.Add(newUser);
            await DbContext.SaveChangesAsync(ct);

            // Update Keycloak with the headStartId for backward compatibility
            await KeycloakService.UpdateHeadStartIdAsync(keycloakId, newUserId.ToString());

            await SendOkAsync(new Response
            {
                UserId = newUserId,
                IsNewUser = true,
                Message = "User successfully onboarded"
            }, ct);
        }
    }

    public record Response
    {
        public Guid UserId { get; init; }
        public bool IsNewUser { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}