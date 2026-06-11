# Project Setup

## 1. Create the Integration Test Project

```bash
cd apps/backend
dotnet new xunit -n _116.Integration.Tests -o tests/Integration --framework net9.0
```

## 2. Project File (`_116.Integration.Tests.csproj`)

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
        <NoWarn>$(NoWarn);xUnit1051</NoWarn>
    </PropertyGroup>

    <PropertyGroup>
        <RootNamespace>_116.Integration.Tests</RootNamespace>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="xunit.v3" Version="1.1.0" />
        <PackageReference Include="xunit.runner.visualstudio" Version="3.0.2">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
        </PackageReference>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />

        <PackageReference Include="AwesomeAssertions" Version="9.0.0" />
        <PackageReference Include="AwesomeAssertions.Analyzers" Version="9.0.0">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
        </PackageReference>

        <PackageReference Include="Bogus" Version="35.6.3" />

        <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
        <PackageReference Include="Testcontainers.PostgreSql" Version="4.3.0" />
        <PackageReference Include="Respawn" Version="6.2.1" />

        <PackageReference Include="coverlet.msbuild" Version="6.0.4">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
        </PackageReference>
        <PackageReference Include="coverlet.collector" Version="6.0.4">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
        </PackageReference>
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\src\Api\Api.csproj" />
        <ProjectReference Include="..\..\src\Modules\Identity\Identity\Identity.csproj" />
        <ProjectReference Include="..\..\src\Modules\Core\Core\Core.csproj" />
        <ProjectReference Include="..\..\src\Modules\Content\Content\Content.csproj" />
        <ProjectReference Include="..\..\src\Shared\Shared\Shared.csproj" />
        <ProjectReference Include="..\..\src\Shared\Shared.Contracts\Shared.Contracts.csproj" />
        <ProjectReference Include="..\..\src\BuildingBlocks\BuildingBlocks.csproj" />

        <ProjectReference Include="..\Fixtures\_116.Tests.Fixtures.csproj" />
    </ItemGroup>
</Project>
```

### Key Differences from Unit Test Project

| Concern | Unit Tests | Integration Tests |
|---------|-----------|-------------------|
| `Api.csproj` reference | No | Yes (for `WebApplicationFactory`) |
| `Microsoft.AspNetCore.Mvc.Testing` | No | Yes |
| `Testcontainers.PostgreSql` | No | Yes |
| `Respawn` | No | Yes |
| `Moq` | Yes | No (no mocking) |
| `Microsoft.EntityFrameworkCore.InMemory` | Yes | No (real PostgreSQL) |
| `Microsoft.EntityFrameworkCore.Sqlite` | Yes | No |

## 3. Add to Solution

```bash
dotnet sln add tests/Integration/_116.Integration.Tests.csproj
```

## 4. Program.cs Accessibility

`WebApplicationFactory<T>` needs access to the entry point assembly. The API project uses top-level statements in `Program.cs`, which generates an internal `Program` class. Make it visible to the integration test project.

Add to `src/Api/Program.cs` (at the very bottom):

```csharp
/// <summary>
/// Entry point class made partial for integration test access via WebApplicationFactory.
/// </summary>
public partial class Program;
```

Alternatively, add to `src/Api/Api.csproj`:

```xml
<ItemGroup>
    <InternalsVisibleTo Include="_116.Integration.Tests" />
</ItemGroup>
```

## 5. Environment Variables for Tests

Integration tests need environment variables that the modules read at startup. These are set programmatically in `ApiFixture.SetEnvironmentVariables()` — no `.env` file needed. All values come from `TestConstants` (see [02-testcontainers-fixture.md](02-testcontainers-fixture.md#new-constants)):

| Variable | Source |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | `"Testing"` (literal) |
| `POSTGRES_HOST` | Parsed from Testcontainers connection string |
| `POSTGRES_PORT` | Parsed from Testcontainers connection string |
| `POSTGRES_DB` | `TestConstants.Database.Name` |
| `POSTGRES_USER` | `TestConstants.Database.User` |
| `POSTGRES_PASSWORD` | `TestConstants.Database.Password` |
| `JWT_SECRET` | `TestConstants.Jwt.ValidSecret` |
| `JWT_ISSUER` | `TestConstants.Jwt.ValidIssuer` |
| `JWT_AUDIENCE` | `TestConstants.Jwt.ValidAudience` |
| `JWT_ACCESS_TOKEN_EXPIRATION` | `TestConstants.Jwt.AccessTokenExpirationMinutes` |
| `JWT_REFRESH_TOKEN_EXPIRATION` | `TestConstants.Jwt.RefreshTokenExpirationDays` |
| `DEFAULT_USER_PASSWORD` | `TestConstants.ExternalServices.DefaultUserPassword` |
| `CLOUDINARY_CLOUD_NAME` | `TestConstants.ExternalServices.CloudinaryCloudName` |
| `CLOUDINARY_API_KEY` | `TestConstants.ExternalServices.CloudinaryApiKey` |
| `CLOUDINARY_API_SECRET` | `TestConstants.ExternalServices.CloudinaryApiSecret` |
| `DASHBOARD_ORIGIN` | `TestConstants.ExternalServices.DashboardOrigin` |
| `WEBAPP_ORIGIN` | `TestConstants.ExternalServices.WebappOrigin` |

## 6. Directory Structure

```
tests/Integration/
├── _116.Integration.Tests.csproj
├── Common/
│   ├── Fixtures/
│   │   ├── PostgresFixture.cs             # Testcontainers lifecycle
│   │   ├── ApiFixture.cs                  # WebApplicationFactory + Testcontainers
│   │   └── DatabaseCollection.cs          # xUnit collection definition
│   ├── Extensions/
│   │   ├── HttpClientExtensions.cs        # Auth header helpers
│   │   └── HttpResponseExtensions.cs      # ProblemDetails deserialization
│   ├── Seeders/
│   │   └── TestDataSeeder.cs              # Per-test or per-class data setup
│   ├── Stubs/
│   │   ├── StubCloudinaryService.cs       # Fake cloud storage
│   │   └── StubYoutubeThumbnailService.cs # Fake YouTube thumbnails
│   └── Abstractions/
│       ├── BaseRepositoryTest.cs          # Repository test base class
│       └── BaseApiTest.cs                 # API test base class
├── Modules/
│   ├── Identity/
│   │   ├── Repositories/                  # AuthRepository, SessionRepository, etc.
│   │   ├── Endpoints/                     # Login, Signup, Session, Role endpoints
│   │   └── Seeders/                       # SuperAdminSeeder, VisitorRoleSeeder
│   ├── Core/
│   │   ├── Repositories/                  # FileRepository
│   │   └── Endpoints/                     # File upload
│   └── Content/
│       ├── Repositories/                  # CategoryRepository, VideoRepository, etc.
│       ├── Endpoints/                     # CRUD categories, videos, articles, etc.
│       ├── Mappers/                       # Round-trip mapper tests
│       ├── Seeders/                       # ContentTypeSeeder
│       └── BackgroundJobs/                # AbandonedDraftCleanupJob
├── Shared/
│   ├── Middleware/                         # Exception handler, rate limiting
│   ├── Interceptors/                      # AuditableEntity, DomainEventDispatch
│   ├── Decorators/                        # Validation, Logging
│   └── Infrastructure/                    # Module registration, DI wiring
└── Workflows/                             # End-to-end cross-module flows
    ├── AuthenticationFlowTests.cs         # Signup→Login→Token→Refresh→SignOut
    ├── OrderLifecycleTests.cs             # Create→AddItems→Submit→Pay→Verify
    ├── ContentPublicationFlowTests.cs     # Create→Edit→Approve→Publish→View
    └── InteractionFlowTests.cs            # Like, Bookmark, Comment, Rate, Share
```
