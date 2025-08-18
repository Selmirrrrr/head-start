namespace HeadStart.SharedKernel.Models.Models.Authorization;

public class ClaimValue
{
    public ClaimValue()
    {
    }

    public ClaimValue(string type, string value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; set; }

    public string Value { get; set; }
}
