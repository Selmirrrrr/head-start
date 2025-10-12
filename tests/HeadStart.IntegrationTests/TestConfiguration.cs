namespace HeadStart.IntegrationTests;

/// <summary>
/// Global test configuration for ensuring fast, independent test execution
/// </summary>
public static class TestConfiguration
{
    /// <summary>
    /// Categories for organizing and filtering tests
    /// </summary>
    public static class Categories
    {
        public const string Standard = "Standard"; // Quick, basic functionality tests
        public const string Security = "Security"; // Authentication/authorization tests
        public const string Performance = "Performance"; // Performance-related tests
        public const string UserInterface = "UserInterface"; // UI-related tests
        public const string HealthCheck = "HealthCheck"; // Health check tests
    }

    /// <summary>
    /// Test timeout configurations
    /// </summary>
    public static class Timeouts
    {
        public const int QuickTest = 2000;      // 2 seconds for quick tests
        public const int UITest = 30000; // 30 seconds for UI tests
    }
}
