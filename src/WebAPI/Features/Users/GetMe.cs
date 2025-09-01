using System.Security.Claims;
using FastEndpoints;
using HeadStart.SharedKernel.Models.Constants;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Users;

public static class GetMe
{
    public class Endpoint : EndpointWithoutRequest<ProfilUtilisateur>
    {
        public required HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Get("/users/me");
            Version(1);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? throw new InvalidOperationException());

            if (!await DbContext.Users.AnyAsync(u => u.IdpId == userId, cancellationToken: ct))
            {
                var utilisateur = new Utilisateur
                {
                    Id = Guid.NewGuid(),
                    IdpId = userId,
                    Email = User.FindFirst(ClaimTypes.Email)?.Value ?? throw new Exception("Email claim is missing"),
                    Nom = User.FindFirst(ClaimTypes.Surname)?.Value ?? throw new Exception("Name claim is missing"),
                    Prenom = User.FindFirst(ClaimTypes.GivenName)?.Value ?? throw new Exception("GivenName claim is missing"),
                    LanguageCode = LanguesCodes.Français,
                };
                DbContext.Users.Add(utilisateur);
                await DbContext.SaveChangesAsync(ct);
            }

            var user = await DbContext.Users
                .Include(u => u.Droits)
                    .ThenInclude(d => d.Role)
                .SingleAsync(u => u.IdpId == userId, ct);

            var profile = new ProfilUtilisateur(
                user.Id,
                user.Nom,
                user.Prenom,
                user.Email,
                user.Droits.Select(utr => new DroitUtilisateur(utr.TenantPath, utr.Role.Code)),
                user.DernierTenantSelectionneId,
                user.LanguageCode,
                user.DarkMode);

            await Send.OkAsync(profile, ct);
        }
    }

    public sealed record ProfilUtilisateur(
    Guid Id,
    string Nom,
    string Prenom,
    string Email,
    IEnumerable<DroitUtilisateur> Roles,
    string? DernierTenantSelectionnePath = null,
    string? LangueCode = null,
    bool DarkMode = false);

    public sealed record DroitUtilisateur(string TenantPath, string RoleCode);
}


