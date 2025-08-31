namespace HeadStart.Client.Services.UserPreferences;

public class UserPreferences
{
    /// <summary>
    /// The current dark light mode that is used
    /// </summary>
    public DarkLightMode DarkLightTheme { get; set; }
}

public enum DarkLightMode
{
    System = 0,
    Light = 1,
    Dark = 2
}
