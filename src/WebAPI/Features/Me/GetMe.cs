using FastEndpoints;
using HeadStart.SharedKernel.Models.Constants;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using HeadStart.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Me;

public static class GetMe
{
    public class Endpoint : EndpointWithoutRequest<ProfilUtilisateur>
    {
        public required HeadStartDbContext DbContext { get; set; }
        public required CurrentUserService CurrentUser { get; set; }

        public override void Configure()
        {
            Get("/me");
            Version(1);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = CurrentUser.UserId;

            if (!await DbContext.Users.AnyAsync(u => u.IdpId == userId, cancellationToken: ct))
            {
                var utilisateur = new Utilisateur
                {
                    Id = Guid.NewGuid(),
                    IdpId = userId,
                    Email = CurrentUser.Email,
                    Nom = CurrentUser.Surname,
                    Prenom = CurrentUser.GivenName,
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


