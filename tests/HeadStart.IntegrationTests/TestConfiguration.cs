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
        public const string Smoke = "Smoke"; // Quick, basic functionality tests
        public const string Security = "Security"; // Authentication/authorization tests
        public const string Performance = "Performance"; // Performance-related tests
        public const string Integration = "Integration"; // Full integration tests
        public const string Validation = "Validation"; // Input validation tests
        public const string ErrorHandling = "ErrorHandling"; // Error scenario tests
        public const string Concurrency = "Concurrency"; // Concurrent execution tests
        public const string StateManagement = "StateManagement"; // CRUD and state tests
        public const string UserInterface = "UserInterface"; // UI-related tests
    }

    /// <summary>
    /// Test timeout configurations
    /// </summary>
    public static class Timeouts
    {
        public const int QuickTest = 2000;      // 2 seconds for quick tests
        public const int StandardTest = 5000;    // 5 seconds for standard tests
        public const int IntegrationTest = 10000; // 10 seconds for complex integration tests
        public const int LongRunningTest = 30000; // 30 seconds for long-running tests
    }
}
