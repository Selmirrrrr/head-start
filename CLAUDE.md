# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Running the Application
- **Start the entire application stack**: `dotnet run --project src/Aspire/AppHost` - Launches all services including PostgreSQL, Keycloak logging, WebAPI, and BFF
- **Run individual projects**:
  - BFF only: `dotnet run --project src/BFF`
  - WebAPI only: `dotnet run --project src/WebAPI`
  - Client only: `dotnet run --project src/Client`

### Building and Testing
- **Build solution**: `dotnet build`
- **Restore packages**: `dotnet restore`
- **Clean solution**: `dotnet clean`
- **Run all tests**: `dotnet test`
- **Run tests with coverage**: `dotnet test --collect:"XPlat Code Coverage"`
- **Run specific test project**: `dotnet test tests/HeadStart.IntegrationTests`

### API Client Generation
The WebAPI project automatically generates Kiota-based API clients for the Blazor client during build. Generated files are placed in `src/Client/Generated/`.

To manually regenerate clients:
```bash
dotnet run --generateclients true --project src/WebAPI/HeadStart.WebAPI.csproj
```

### E2E Testing with Playwright
Integration tests use Playwright for browser automation:

**First-time setup:**
```bash
# Install Playwright CLI globally
dotnet tool install --global Microsoft.Playwright.CLI

# Install browsers
playwright install
```

**Running E2E tests:**
- Tests are in `tests/HeadStart.IntegrationTests/UITests/`
- Use `BaseApiTest` for API integration tests
- Use `PlaywrightTestBase` for UI tests

## Architecture Overview

### Project Structure
This is a .NET 9 solution using a Backend-for-Frontend (BFF) pattern with the following main components:

1. **HeadStart.Aspire.AppHost** - Orchestrates all services using .NET Aspire
2. **HeadStart.BFF** - ASP.NET Core BFF that serves the Blazor WebAssembly client and proxies API calls
3. **HeadStart.WebAPI** - FastEndpoints-based API with PostgreSQL integration
4. **HeadStart.Client** - Blazor WebAssembly frontend using MudBlazor and Tailwind CSS
5. **HeadStart.SharedKernel** - Shared utilities, models, and extensions
6. **HeadStart.SharedKernel.Models** - Shared domain models and DTOs
7. **HeadStart.Aspire.ServiceDefaults** - Common Aspire service configurations
8. **HeadStart.IntegrationTests** - TUnit-based integration tests with Playwright for E2E testing

### Authentication & Authorization
- **Authentication**: OpenID Connect with Keycloak (BFF) and OpenIddict validation (WebAPI)
- **Token Flow**: BFF handles user authentication and forwards bearer tokens to API via YARP reverse proxy
- **Claims**: Custom claims transformation in `ClaimsTransformer.cs`

### Data Layer
- **Database**: PostgreSQL with Entity Framework Core
- **Context**: `HeadStartDbContext` with async seeding
- **Connection**: Configured through Aspire service discovery

### Frontend Architecture
- **Framework**: Blazor WebAssembly with MudBlazor components
- **State Management**: Built-in Blazor state management
- **API Communication**: Kiota-generated clients from OpenAPI specs

### Infrastructure Services
- **Logging**: Serilog
- **Monitoring**: Aspire dashboard with OpenTelemetry
- **Reverse Proxy**: YARP for API proxying with automatic token forwarding
- **Security**: Comprehensive security headers, CSRF protection, and data protection

### Key Patterns
- **FastEndpoints**: Used in WebAPI for endpoint organization
- **YARP**: Reverse proxy pattern for API calls from BFF
- **Service Discovery**: Aspire-based service registration and discovery
- **Configuration**: Hierarchical with `Directory.Build.props` and `Directory.Packages.props`

### Development Environment
- **Hot Reload**: Enabled for Blazor development
- **Code Quality**: Roslynator and SonarAnalyzer integration
- **Editor Config**: Standardized formatting and style rules
- **Package Management**: Central package version management via `Directory.Packages.props`
- **Testing Framework**: TUnit with Playwright for E2E and Shouldly for assertions
