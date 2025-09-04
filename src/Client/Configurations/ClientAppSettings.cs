namespace HeadStart.Client.Configurations;

public class ClientAppSettings
{
    public const string KEY = nameof(ClientAppSettings);
    public string AppName { get; set; } = "Claimly";
    public string Version { get; set; } = "0.0.1";
    public string Copyright { get; set; } = string.Empty;
}

