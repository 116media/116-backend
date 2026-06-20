# Phase 0: Project Setup Spec

## Tasks

- [ ] Create `tests/_116.Integration.Tests/_116.Integration.Tests.csproj`
- [ ] Add NuGet packages
- [ ] Add project references to all src modules
- [ ] Create `GlobalUsings.cs`
- [ ] Create folder structure
- [ ] Verify `dotnet build` succeeds

## NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="latest" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="latest" />
<PackageReference Include="Testcontainers.PostgreSql" Version="latest" />
<PackageReference Include="xunit.v3" Version="1.1.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="latest" />
<PackageReference Include="AwesomeAssertions" Version="9.0.0" />
<PackageReference Include="Bogus" Version="35.6.3" />
<PackageReference Include="Respawn" Version="latest" />
<PackageReference Include="EFCore.NamingConventions" Version="latest" />
```

> Run `npm show <package> dist-tags.latest` equivalent (`dotnet package search`) or check NuGet before pinning versions.

## Project References

```xml
<ProjectReference Include="..\..\src\API\_116.API.csproj" />
<ProjectReference Include="..\..\src\Modules\Identity\Identity\_116.Identity.csproj" />
<ProjectReference Include="..\..\src\Modules\Content\Content\_116.Content.csproj" />
<ProjectReference Include="..\..\src\Modules\Core\Core\_116.Core.csproj" />
<ProjectReference Include="..\..\src\Shared\Shared\_116.Shared.csproj" />
<ProjectReference Include="..\Fixtures\_116.Tests.Fixtures.csproj" />
```

## GlobalUsings.cs

```csharp
global using Xunit;
global using AwesomeAssertions;
global using System.Net;
global using System.Net.Http.Json;
global using Microsoft.EntityFrameworkCore;
global using _116.Integration.Tests.Common.Fixtures;
global using _116.Integration.Tests.Common.Base;
global using _116.Integration.Tests.Common.Extensions;
global using static _116.Tests.Fixtures.Constants.TestConstants;
```

## Folder Structure to Create

```
tests/_116.Integration.Tests/
├── Common/
│   ├── Fixtures/
│   ├── Base/
│   ├── Extensions/
│   ├── Seeders/
│   ├── Stubs/
│   └── Constants/
├── Shared/
│   ├── Interceptors/
│   ├── Decorators/
│   ├── ExceptionHandlers/
│   └── Middleware/
├── Identity/
│   ├── Repositories/
│   ├── Api/
│   │   ├── Auth/
│   │   ├── Roles/
│   │   ├── Session/
│   │   └── User/
│   ├── Services/
│   └── Mappers/
├── Core/
│   ├── Repositories/
│   └── Services/
└── Content/
    ├── Repositories/
    ├── Api/
    │   ├── Catalog/
    │   ├── Commerce/
    │   ├── Editorial/
    │   ├── Interactions/
    │   └── Lookup/
    ├── Mappers/
    └── Seeders/
```

## Acceptance Criteria

1. `dotnet build tests/_116.Integration.Tests` compiles without errors
2. Empty test project is recognized by `./scripts/run-tests-with-coverage.sh integration`
3. All project references resolve correctly
