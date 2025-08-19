namespace HeadStart.SharedKernel.Models.Models.Authorization;

public class ClaimValue(string type, string value)
{
    public string Type { get; } = type;

    public string Value { get; } = value;
}
