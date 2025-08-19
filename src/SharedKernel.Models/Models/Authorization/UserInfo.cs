namespace HeadStart.SharedKernel.Models.Models.Authorization;

public class UserInfo
{
    public static readonly UserInfo Anonymous = new();

    public ICollection<ClaimValue> Claims { get; set; } = [];

    public string EmailClaimType { get; set; } = string.Empty;

    public bool IsAuthenticated { get; set; }

    public string NameClaimType { get; set; } = string.Empty;

    public string RoleClaimType { get; set; } = string.Empty;
}
