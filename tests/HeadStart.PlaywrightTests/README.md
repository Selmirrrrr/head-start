# HeadStart Playwright Tests

This project contains Playwright end-to-end tests for the HeadStart Blazor WebAssembly application.

## Test Scenarios

### LoginTests

1. **LoginWithValidCredentials_ShouldShowUserNameInSidebar**
   - Tests successful login with credentials: username="user", password="user"
   - Verifies that "Hello, Default User!" appears in the sidebar after login
   - Confirms Sign out button is visible

2. **LoginWithInvalidCredentials_ShouldShowError**
   - Tests failed login with invalid credentials
   - Verifies user remains on login page or sees error
   - Ensures welcome message does NOT appear

## Prerequisites

1. Install Playwright browsers:
   ```bash
   dotnet build tests/HeadStart.PlaywrightTests
   playwright install
   ```

## Running the Tests

1. **Build the test project:**
   ```bash
   dotnet build tests/HeadStart.PlaywrightTests/HeadStart.PlaywrightTests.csproj
   ```

2. **Run all tests:**
   ```bash
   dotnet test tests/HeadStart.PlaywrightTests/HeadStart.PlaywrightTests.csproj
   ```

3. **Run specific test:**
   ```bash
   dotnet test tests/HeadStart.PlaywrightTests/HeadStart.PlaywrightTests.csproj --filter "LoginWithValidCredentials"
   ```

## How It Works

- Tests use the same Aspire testing infrastructure as your existing integration tests
- Full application stack is started automatically (PostgreSQL, Keycloak, WebAPI, BFF)
- Playwright browser automation handles the UI interactions
- Tests wait for services to be ready before executing
- HTTPS certificate errors are ignored for testing

## Configuration

- Tests run in headless Chrome by default
- Screenshots and videos are captured on test failures
- Test timeout is set to 30 seconds
- Certificate errors are ignored for CI/test environments

## Troubleshooting

If tests fail:
1. Check that all services start properly in the Aspire dashboard
2. Verify Keycloak is configured with the test user (username: "user", password: "user")
3. Ensure the BFF service is accessible and responding
4. Check browser console logs for JavaScript errors