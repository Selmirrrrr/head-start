# Claimly BFF

A .NET 9 solution using Backend-for-Frontend pattern with Blazor WebAssembly, FastEndpoints API, and PostgreSQL.

## Architecture



## Quick Start

```bash
# Run entire application stack (recommended)
dotnet run --project src/Aspire/AppHost/HeadStart.Aspire.AppHost.csproj
```

## Tech Stack

- **Frontend**: Blazor WebAssembly + MudBlazor + Tailwind CSS
- **Backend**: ASP.NET Core BFF + FastEndpoints API
- **Database**: PostgreSQL with Entity Framework Core
- **Auth**: OpenID Connect (Keycloak) + OpenIddict
- **Infrastructure**: .NET Aspire orchestration + YARP reverse proxy

## Architecture

BFF serves the Blazor client and proxies API calls with automatic token forwarding. WebAPI uses FastEndpoints for clean endpoint organization. Aspire handles service discovery and orchestration.

## E2E Testing Setup

### Install Playwright CLI
```bash
dotnet tool install --global Microsoft.Playwright.CLI

# Add to PATH (choose based on your OS)
# macOS/Linux:
export PATH="$PATH:$HOME/.dotnet/tools"
# Windows (PowerShell):
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"

# Verify installation
playwright

# Install browsers (required for test execution)
playwright install
```

## API Client Generation

To generate the API clients, run the following command:

```bash
dotnet run --generateclients true --project src/WebAPI/HeadStart.WebAPI.csproj
```

> [!TIP]
> A placeholder connection string must be set in the `appsettings.json` file to  run the API client generation.

```json
{
  "ConnectionStrings": {
    "postgresdb": "Host=localhost;Port=5432;Database=headstart;Username=postgres;Password=postgres"
  }
}
```

## ToDo

- [ ] Fix audit trail filtering
- [ ] Implement impersonation
- [ ] Implement MudTranslations `dotnet add package MudBlazor.Translations`
- [ ] Implement Finish translation resources files per component
