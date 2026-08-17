# CLAUDE.md - 116 Backend Codebase Guide

This document provides comprehensive context for working with the 116 Backend codebase.

## Quick Overview

- **.NET 9.0 Web API** with Clean Architecture and Domain-Driven Design
- **Database**: PostgreSQL with Entity Framework Core (snake_case naming)
- **Architecture**: Modular Monolith with CQRS, Vertical Slices, and DDD patterns
- **Auth**: JWT Bearer tokens with Role-Based Access Control (RBAC)
- **API Framework**: Carter (minimal APIs wrapper) with API versioning

## Project Structure

```
/src
├── Api/                          # ASP.NET Core Web API entry point
│   └── Program.cs               # App configuration, middleware, Swagger
├── BuildingBlocks/              # Shared constants
│   └── Constants/               # RateLimitPolicies, UserConstants
├── Shared/
│   ├── Shared/                  # Core infrastructure
│   │   ├── Abstractions/        # CQRS base classes, IRepository
│   │   ├── Common/              # BaseModule, ModuleOptions
│   │   ├── Domain/              # Aggregate<T>, Entity<T>, IDomainEvent
│   │   ├── Exceptions/          # Custom exceptions, handlers
│   │   ├── Middlewares/         # Global exception handler
│   │   ├── Pagination/          # Paged queries/responses
│   │   └── Specifications/      # Specification pattern
│   └── Shared.Contracts/        # CQRS interfaces (ICommand, IQuery)
└── Modules/
    ├── Core/                    # File management module
    │   ├── Domain/             # FileEntity
    │   ├── Application/        # File use cases
    │   └── Infrastructure/     # CoreDbContext, migrations
    └── Identity/               # User auth & authorization module
        ├── Domain/             # UserEntity, RoleEntity, ValueObjects
        ├── Application/        # Auth, User, Session, Role use cases
        └── Infrastructure/     # IdentityDbContext, migrations, seeders
```

## Technology Stack

| Category | Technology |
|----------|------------|
| Framework | .NET 9.0, ASP.NET Core |
| Database | PostgreSQL with Npgsql |
| ORM | Entity Framework Core 9.0.4 |
| API Routing | Carter 8.2.1 |
| Validation | FluentValidation 12.0.0 |
| Mapping | Mapster 7.4.2 |
| Logging | Serilog with Seq sink |
| Auth | JWT Bearer (System.IdentityModel.Tokens.Jwt) |
| File Storage | Cloudinary |
| Export | ClosedXML (Excel), CsvHelper (CSV) |
| Code Formatting | CSharpier |

## Architecture Patterns

### CQRS (Command Query Responsibility Segregation)
- **Commands**: `ICommand<TResult>` or `ICommand` (for void)
- **Queries**: `IQuery<TResult>`
- **Handlers**: `ICommandHandler<TCommand, TResponse>`, `IQueryHandler<TQuery, TResponse>`
- **Dispatcher**: Routes requests to handlers via `Dispatcher` service

### Domain-Driven Design
- **Aggregates**: Inherit from `Aggregate<TId>` (has domain events)
- **Entities**: Inherit from `Entity<TId>`
- **Value Objects**: Immutable types (Email, AuthProvider, OtpPurpose)
- **Domain Events**: Implement `IDomainEvent`

### Decorator Pattern
- `ValidationDecorator<TRequest, TResponse>` - Input validation via FluentValidation
- `LoggingDecorator<TRequest, TResponse>` - Request/response logging
- Applied via Scrutor service decoration

### Repository & Unit of Work
- `IRepository<T>` - Generic repository interface
- `IUnitOfWork` - Transaction management
- Module-specific: `IFileRepository`, `IAuthRepository`, `IRoleRepository`, `ISessionRepository`

### Specification Pattern
- `Specification<T>` base class for composable query predicates
- Supports And/Or/Not operators

## Common Commands

```bash
# Run locally with Docker
docker-compose -f docker-compose.yml -f docker-compose.override.yml up

# Run API directly
dotnet run --project src/Api

# Add EF migration (Identity module)
dotnet ef migrations add MigrationName --project src/Modules/Identity/Infrastructure --startup-project src/Api

# Update database
dotnet ef database update --project src/Modules/Identity/Infrastructure --startup-project src/Api

# Format code
dotnet csharpier .

# Build
dotnet build
```

## Coding Conventions

### XML Documentation
Always use the multiline block form for XML doc comments. Never collapse a tag onto a single line.

**Correct:**
```csharp
/// <summary>
/// Records that a user has bookmarked an article.
/// </summary>
public class ArticleBookmarkEntity { }

/// <summary>
/// The identity user UUID of the user who bookmarked the article.
/// </summary>
public Guid UserId { get; set; }
```

**Wrong — inline/single-line form is not allowed:**
```csharp
/// <summary>The identity user UUID of the user who bookmarked the article.</summary>
public Guid UserId { get; set; }
```

Use `/// <inheritdoc />` on overrides and interface implementations instead of repeating the summary.

#### Length and content rules

Docs name the thing; rationale lives in the spec/design doc and the PR — never in the code.

| Element | `<summary>` budget |
| ------- | ------------------ |
| Class / record / interface | Max 2–3 lines. What it is; one key responsibility if not obvious. |
| Method (public, private, interface, handler `Handle`) | Max 2–3 lines. What it does; a real caller-facing constraint if one exists. |
| Property | 1 sentence. What the value is. |
| Field / constant | 1 sentence. |
| Enum / enum member | 1 short phrase. |
| Ctor / primary-ctor `<param>` | One noun phrase (e.g. "Repository for role data access."). |
| Method `<param>` / `<returns>` | One short phrase each, not full sentences. |

Never put in a doc comment:

- Design rationale or architecture narration ("Permissions are baked into the token, so...")
- Bullet lists, descriptions of how other components use this one
- How the behavior differs from a previous version
- `<remarks>` unless it documents a genuine caller-facing constraint (thread-safety, ordering, a side effect the caller must handle)

### Inline `//` comments

Inline comments are rare — roughly 5% of generated lines, only where a line genuinely needs
explanation. A comment survives only when it states a constraint or invariant the code cannot
express (e.g. `// Soft deletion also clears IsActive, so the deleted state is checked first`).

- Max 1–2 lines. Never a paragraph.
- Never narrate what the next line does — the code already says it.
- Never explain why a change is correct or how behavior differs from before; that belongs in the PR.
- Never restate the spec or design doc inside the code.

### Naming
- **C# files/classes**: PascalCase (`UserEntity.cs`, `PublicLoginCommand`)
- **Interfaces**: Prefix with I (`IAuthRepository`, `ICommand`)
- **Private fields**: Underscore prefix (`_domainEvents`)
- **Database columns**: snake_case (automatic via EFCore.NamingConventions)
- **Use case files and classes**: Always prefixed with scope — `Admin` for admin use cases, `Public` for public use cases. Applies to every file and type in the use case folder: Command/Query, Handler, MetaField, Validator, Endpoint, Result, Request, Response (e.g., `AdminPublishArticleCommand.cs`, `PublicAddArticleCommentHandler.cs`). The folder name itself is NOT prefixed — only file names and type names.

### File Organization
- One class per file (exceptions: nested classes, related DTOs)
- Feature-based organization within modules (vertical slices)

### Code Style (.editorconfig enforced)
- 4-space indentation
- LF line endings
- UTF-8 encoding
- CSharpier formatting (pre-commit hook)
- Never use XML comments as section separators in `.csproj` files (e.g., `<!-- Coverage -->`, `<!-- Test Framework -->`). Group related entries with blank lines instead.
- Never write fully-qualified type/member names inline in code (e.g. `_116.Tests.Fixtures.Helpers.TestErrorsFactory.CreateLyricsErrors()` or `_116.Content.Domain.Entities.ContentOrderEntity`). Add a `using` directive and reference the short name instead. Only fall back to a fully-qualified reference (or a `using Alias = ...;`) when two types with the same short name genuinely collide in one file.

## Testing — MUST READ BEFORE WRITING ANY TEST

**Canonical rulebook: [`docs/testing/00-unit-vs-integration-rules.md`](docs/testing/00-unit-vs-integration-rules.md).**
Read it in full before adding or modifying a test. The rules below are the non-negotiable summary;
the doc has the reasoning, worked examples, and the review checklist.

### The difference in one line

- **Unit test** proves a method *works*. **Integration test** proves the method is *used*.
- Only integration tests can catch dead code. That is their most valuable job.

### Integration tests (`tests/Integration/`)

An integration test MUST reach its target through a real entry point — there are exactly two:

1. **Real HTTP**: class inherits `BaseApiTest(db)`, drives `Client.PostAsJsonAsync(...)` etc., asserts status + persisted side effect.
2. **Real repository from DI**: class inherits `BaseRepositoryTest(postgres)`, uses `Resolve<IFooRepository>()`.

Both are `[Collection("Database")]`. Inside `tests/Integration/` it is **forbidden** to:

- `new` a validator, handler, specification, entity, or error factory (that is a unit test in the wrong folder)
- mock a repository, service, or `DbContext` (external-service stubs like Cloudinary are the only exception)
- use reflection to invoke private members
- build your own `ServiceCollection` to assert DI registrations

Specifications are **never** referenced directly — cover them by calling the repository method that
uses them, and name the spec in the doc comment. There is **no `Domain/` and no `Specifications/`
folder** under `tests/Integration/`; creating one means you are writing a unit test in the wrong place.
Folder paths mirror `src/`, endpoint tests are named `<UseCase>EndpointV1Tests.cs`.

### Unit tests (`tests/Unit/`)

Own these exhaustively, because integration deliberately skips them: domain entity guards and every
state transition (including no-op/early-return branches), validator rules and boundaries, handler
orchestration with mocked repositories, error factory methods, specification predicate logic.
Never touch a real database, HTTP pipeline, or DI container.

### Coverage is a signal, not a target

A file with high **unit** coverage but near-zero **integration** coverage means the code is not wired
into the application — that is a defect in `src/`, not a missing test. Correct response:

1. `grep` all of `src/` for callers. Only its own definition, DI registration, and i18n facade? It is dead.
2. Either **wire it up** (usually right — e.g. make the handler throw `i18n.Translation.RevisionNotFound(id)`
   instead of a generic repository throw, so the client gets a localized error) or **delete it**.
3. Only then add the integration test that drives the newly wired path.

**Never** close such a gap by constructing the object directly inside `tests/Integration/`. That turns
green the exact metric that was warning you, and the dead code ships. Never delete a defensive guard to
chase coverage; if a line is provably unreachable by construction, mark it `[ExcludeFromCodeCoverage]`
with a reason and say so in the PR — never cover it by reflection.

## API Structure

### URL Pattern
```
/api/v{version}/{scope}/{resource}/{action}
```
- **Scopes**: `public` (unauthenticated), `admin` (authenticated)
- **Versions**: URL path (`/api/v1/...`). Every route is mapped under `api/v{version:apiVersion}`, so the path segment is always required. The `X-Api-Version: 1` header is read as well and must agree with the path segment; a header naming a different version is refused.

### Endpoint Examples
```
POST /api/v1/public/auth/login          # Public login
POST /api/v1/public/auth/signup         # Public signup
POST /api/v1/admin/auth/login           # Admin login
GET  /api/v1/admin/users                # List users (admin)
GET  /api/v1/admin/sessions             # List sessions
POST /api/v1/admin/roles                # Create role
```

### Creating New Endpoints (Carter Module)
```csharp
public class MyEndpointV1 : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/resource", HandleAsync)
            .WithName("MyEndpoint")
            .WithApiVersionSet(VersionSets.Default)
            .MapToApiVersion(1)
            .RequireAuthorization()
            .WithRateLimiting(RateLimitPolicies.ContentBrowsing);
    }

    private static async Task<IResult> HandleAsync(
        MyRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new MyCommand(request.Data);
        var result = await dispatcher.DispatchAsync(command, ct);
        return Results.Ok(result);
    }
}
```

### Rate Limiting Policies
| Policy | Use Case | Strategy |
|--------|----------|----------|
| `Authentication` | Login, credentials | Sliding window |
| `Otp` | OTP verification | Sliding window |
| `PasswordManagement` | Password reset/change | Sliding window |
| `FileUpload` | File uploads | Token bucket |
| `DataExport` | Session/data export | Token bucket |
| `ContentBrowsing` | Read endpoints | Fixed window |
| `UserProfile` | Profile operations | Fixed window |
| `SessionManagement` | Session operations | Fixed window |
| `AdminMetrics` | Admin metrics | Fixed window |

## Database

### Schemas
- `core` - File management (CoreDbContext)
- `authentication` - Users, roles, sessions, OTPs (IdentityDbContext)

### Key Entities
| Entity | Table | Description |
|--------|-------|-------------|
| `UserEntity` | users | User accounts with email, password hash |
| `RoleEntity` | roles | Role definitions (SuperAdmin, Admin, Visitor) |
| `PermissionEntity` | permissions | Permission definitions |
| `UserRoleEntity` | user_roles | User-role assignments (M:N) |
| `RolePermissionEntity` | role_permissions | Role-permission assignments (M:N) |
| `SessionEntity` | sessions | Active login sessions with tokens |
| `OtpEntity` | otps | One-time passwords for verification |
| `FileEntity` | files | Uploaded files with metadata |

### Auditable Fields (automatic)
All entities inheriting from `Aggregate<T>` or auditable base:
- `created_at` - Creation timestamp
- `updated_at` - Last update timestamp
- `created_by` - User ID who created
- `updated_by` - User ID who last updated

### Adding Migrations
```bash
# For Identity module
dotnet ef migrations add MigrationName \
  --project src/Modules/Identity/Infrastructure \
  --startup-project src/Api \
  --context IdentityDbContext

# For Core module
dotnet ef migrations add MigrationName \
  --project src/Modules/Core/Infrastructure \
  --startup-project src/Api \
  --context CoreDbContext
```

## Authentication & Authorization

### JWT Configuration (Environment Variables)
```
JWT_SECRET=your-secret-key-min-32-chars
JWT_ISSUER=116_frontend
JWT_AUDIENCE=116_client
JWT_ACCESS_TOKEN_EXPIRATION_IN_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRATION_IN_DAYS=30
```

### Auth Flow
1. User logs in via `/api/v1/public/auth/login` (email + password)
2. Server returns `accessToken` (short-lived) and `refreshToken` (long-lived)
3. Client sends `Authorization: Bearer {accessToken}` header
4. When access token expires, use `/api/v1/public/auth/refresh` with refresh token

### Roles
- `SuperAdmin` - Full system access
- `Admin` - Administrative access
- `Visitor` - Limited/guest access

### Protecting Endpoints
```csharp
// Require authentication
.RequireAuthorization()

// Require specific role
.RequireAuthorization(policy: UserRolePolicies.AdminOnly)

// Allow anonymous
.AllowAnonymous()
```

### Custom Authorization Requirements
- `AccountStatusRequirement` - User must be active and verified

## Adding New Features

### 1. Add New Command/Query (CQRS)

**Command** (for mutations):
```csharp
// Command record
public record MyCommand(string Data) : ICommand<MyResponse>;

// Handler
public class MyCommandHandler : ICommandHandler<MyCommand, MyResponse>
{
    public async Task<MyResponse> Handle(MyCommand command, CancellationToken ct)
    {
        // Implementation
    }
}

// Validator (optional)
public class MyCommandValidator : AbstractValidator<MyCommand>
{
    public MyCommandValidator()
    {
        RuleFor(x => x.Data).NotEmpty();
    }
}
```

**Query** (for reads):
```csharp
public record MyQuery(Guid Id) : IQuery<MyResponse>;

public class MyQueryHandler : IQueryHandler<MyQuery, MyResponse>
{
    public async Task<MyResponse> Handle(MyQuery query, CancellationToken ct)
    {
        // Implementation
    }
}
```

### 2. Add New Entity

1. Create entity in `Domain/Entities/`:
```csharp
public class MyEntity : Aggregate<Guid>
{
    public string Name { get; private set; }

    private MyEntity() { } // EF Core

    public static MyEntity Create(string name)
    {
        return new MyEntity { Id = Guid.NewGuid(), Name = name };
    }
}
```

2. Add configuration in `Infrastructure/Persistence/Configurations/`:
```csharp
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
    }
}
```

3. Add DbSet to DbContext:
```csharp
public DbSet<MyEntity> MyEntities { get; set; }
```

4. Create and run migration.

### 3. Add New Module

1. Create folder structure:
```
src/Modules/MyModule/
├── Domain/
│   └── Entities/
├── Application/
│   ├── UseCases/
│   ├── Contracts/
│   └── Exceptions/
└── Infrastructure/
    ├── Persistence/
    │   ├── Configurations/
    │   └── Migrations/
    └── Repositories/
```

2. Create `MyModuleModule.cs` extending `BaseModule`:
```csharp
public class MyModuleModule : BaseModule
{
    public override void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        // Register services, repositories, handlers
    }
}
```

3. Register in `Program.cs`.

## Exception Handling

### Custom Exceptions
Located in `Shared/Exceptions/` and module-specific `Application/Exceptions/`:
- Inherit from `BaseException`
- Include error code, message, HTTP status

### Exception Strategies
Implement `IExceptionStrategy` for custom exception handling:
```csharp
public class MyExceptionStrategy : IExceptionStrategy
{
    public bool CanHandle(Exception ex) => ex is MyException;
    public ProblemDetails Handle(Exception ex) { /* ... */ }
}
```

### Global Exception Middleware
All unhandled exceptions are caught and converted to ProblemDetails responses.

## Environment Variables

Required variables (see `.env.template`):

```bash
# Database
DB_HOST=localhost
DB_PORT=5432
DB_USER=postgres
DB_PASSWORD=your-password
DB_NAME=116_db

# JWT
JWT_SECRET=your-secret-key-minimum-32-characters
JWT_ISSUER=116_frontend
JWT_AUDIENCE=116_client
JWT_ACCESS_TOKEN_EXPIRATION_IN_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRATION_IN_DAYS=30

# Cloudinary (file uploads)
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=your-api-key
CLOUDINARY_API_SECRET=your-api-secret

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Development
```

## Logging

### Serilog Configuration
- Structured logging with Serilog
- Console sink for development
- Seq sink for centralized logging (port 5341)
- Log levels configured in `appsettings.json`

### Request Logging
HTTP requests automatically logged via Serilog middleware with:
- Request path, method, status code
- Response time
- Trace ID for correlation

## Docker

### Build and Run
```bash
# Development (with hot reload)
docker-compose -f docker-compose.yml -f docker-compose.override.yml up

# Production
docker-compose up -d
```

### Services
- `116_api` - API on port 5025
- `116_db` - PostgreSQL on port 5432
- `116_seq` - Seq logging on ports 5341 (API) / 9091 (Web UI)

## Git Workflow

- **Main branch**: `develop`
- **Feature branches**: `feat-*` or `feature/*`
- **Bug fixes**: `bug-*` or `fix-*`
- **Pre-commit hook**: CSharpier formatting check
- **PR target**: Always `develop` unless specified

## Commit Messages

Follow [Conventional Commits 1.0.0](https://www.conventionalcommits.org/) specification.

**Never mention AI, Claude, or assistants in commit messages.**

### Format
```
<type>[optional scope][!]: <description>

[optional body]

[optional footer(s)]
```

### Types
Must match branch naming pattern from pre-push hook.

| Type | Description | SemVer |
|------|-------------|--------|
| `feat` | New feature or functionality | MINOR |
| `fix` | Bug fix (preferred over `bug`) | PATCH |
| `bug` | Bug fix (alias for `fix`) | PATCH |
| `docs` | Documentation only changes | - |
| `style` | Code style/formatting (no logic change) | - |
| `refactor` | Code change without feature/fix | - |
| `perf` | Performance improvements | - |
| `test` | Adding or modifying tests | - |
| `build` | Build system or external dependencies | - |
| `ci` | CI configuration changes | - |
| `chore` | Maintenance tasks, dependency updates | - |
| `revert` | Reverting previous changes | - |

### Scope (Optional)
Add scope in parentheses to specify the affected area:
```bash
feat(auth): add session validation
fix(session): correct token expiration
refactor(identity): simplify user factory
```

Common scopes for this project:
- `auth` - Authentication logic
- `session` - Session management
- `user` - User management
- `role` - Role/permission management
- `api` - API endpoints
- `db` - Database/migrations
- `config` - Configuration changes

### Breaking Changes
Use `!` after type/scope for breaking changes:
```bash
feat!: change authentication flow
feat(api)!: update response format
```

Or add `BREAKING CHANGE:` footer:
```bash
feat: update session token format

BREAKING CHANGE: session tokens now use JWT instead of [.gitignore](.gitignore)opaque tokens
```

### Rules
- **Lowercase** type and description
- **No period** at end of description
- **Imperative mood**: "add", "fix", "use", "create", "remove" (not "added", "fixes")
- **Be specific**: mention component/service names
- **Under 72 characters** for description line
- **One logical change per commit**: atomic commits
- **Blank line** between description and body (if body exists)

### Examples

**Simple commits (most common):**
```bash
feat: add session validation with IsSessionValidAsync
feat(auth): add rate limiting to public login endpoint
fix: correct SessionMetadataService ip-address parsing
fix(session): use DateTimeOffset.UtcNow for token expiration
refactor: use SessionFactory CreateSessionAsync
refactor(identity): remove unused UserNotLoggedInException
docs: update API endpoint documentation
style: format code with CSharpier
perf(db): optimize user query with index
test: add unit tests for session validation
build: add .csharpierignore to ignore files
ci: update GitHub Actions workflow
chore: update NuGet dependencies
revert: undo session token changes
```

**With scope:**
```bash
feat(auth): implement social login with Google
fix(api): handle null reference in user endpoint
refactor(session): extract token generation to factory
```

**Breaking change:**
```bash
feat(api)!: change authentication response format

BREAKING CHANGE: login response now returns tokens in nested object
```

**With body:**
```bash
fix: prevent racing of requests

Introduce a request id and a reference to latest request.
Dismiss incoming responses other than from latest request.
```

**With footer:**
```bash
fix(session): correct refresh token validation

Refs: #123
```

### Bad Examples (Avoid)
```bash
# Too vague
feat: update code
fix: fix issue

# Past tense
feat: added new feature
fix: fixed the bug

# Too long description
feat: add new session validation feature that checks if session is valid using IsSessionValidAsync

# Ends with period
feat: add login endpoint.

# Capitalized description
feat: Add new feature

# Mentions AI/assistant
feat: add feature (generated by Claude)
fix: bug fix suggested by AI
```

### Commit Flow
```bash
# Stage changes
git add <files>

# Simple commit
git commit -m "type: description"

# With scope
git commit -m "type(scope): description"

# With body (use heredoc for multi-line)
git commit -m "$(cat <<'EOF'
fix: prevent racing of requests

Introduce a request id and a reference to latest request.
Dismiss incoming responses other than from latest request.

Refs: #123
EOF
)"

# Push to feature branch
git push origin feat-branch-name
```

### Branch Naming (enforced by pre-push hook)
Branch names must match pattern: `^(feat|chore|bug|fix|doc|docs|style|refactor|perf|test|build|ci|revert)-[a-z]+(-[a-z]+)*$`

Examples:
- `feat-user-authentication`
- `fix-api-response-validation`
- `chore-update-dependencies`
- `docs-git-workflow`
- `refactor-session-factory`

## Key Files Reference

| File | Purpose |
|------|---------|
| `src/Api/Program.cs` | App entry, middleware, Swagger config |
| `src/Shared/Shared/Abstractions/Dispatcher.cs` | CQRS dispatcher |
| `src/Shared/Shared/Common/BaseModule.cs` | Module base class |
| `src/Modules/Identity/Infrastructure/Persistence/IdentityDbContext.cs` | Identity database context |
| `src/Modules/Core/Infrastructure/Persistence/CoreDbContext.cs` | Core database context |
| `.env.template` | Environment variable template |
| `docker-compose.yml` | Docker services definition |

## Troubleshooting

### Database Connection Issues
1. Check `.env` file exists with correct values
2. Ensure PostgreSQL is running: `docker-compose ps`
3. Verify connection string in logs

### Migration Errors
1. Ensure startup project is `src/Api`
2. Check DbContext is correctly specified
3. Run `dotnet ef migrations list` to see pending migrations

### JWT Errors
1. Verify `JWT_SECRET` is at least 32 characters
2. Check token expiration settings
3. Ensure issuer/audience match between generation and validation

---

## Role & Permission Management

### Domain Entities

| Entity | Location | Description |
|--------|----------|-------------|
| `RoleEntity` | `Identity/Domain/Entities/RoleEntity.cs` | Role with Name, Description, IsActive, IsDeleted |
| `PermissionEntity` | `Identity/Domain/Entities/PermissionEntity.cs` | Permission with Resource, Action, IsActive, IsDeleted |
| `UserRoleEntity` | `Identity/Domain/Entities/UserRoleEntity.cs` | M:N junction: User ↔ Role |
| `RolePermissionEntity` | `Identity/Domain/Entities/RolePermissionEntity.cs` | M:N junction: Role ↔ Permission |

### Entity Fields

**RoleEntity:**
| Field | Type | Description |
|-------|------|-------------|
| `Id` | Guid | Primary key |
| `Name` | string (20) | Role name, unique |
| `Description` | string (300) | Human-readable description |
| `IsActive` | bool | Whether role can be assigned (default: true) |
| `IsDeleted` | bool | Soft delete flag (default: false) |
| `DeletedAt` | DateTime? | When soft deleted |

**PermissionEntity:**
| Field | Type | Description |
|-------|------|-------------|
| `Id` | Guid | Primary key |
| `Resource` | string (15) | Resource name (e.g., "user", "article") |
| `Action` | string (15) | Action type (e.g., "read", "create") |
| `Description` | string (300) | Human-readable description |
| `IsActive` | bool | Whether permission is usable (default: true) |
| `IsDeleted` | bool | Soft delete flag (default: false) |
| `DeletedAt` | DateTime? | When soft deleted |

### Entity Methods

**RoleEntity / PermissionEntity:**
| Method | Returns | Description |
|--------|---------|-------------|
| `Create(...)` | Entity | Factory method with validation |
| `Update(...)` | void | Update name/description |
| `Activate()` | bool | Set IsActive = true |
| `Deactivate()` | bool | Set IsActive = false |
| `SoftDelete()` | bool | Set IsDeleted = true, IsActive = false, DeletedAt = now |
| `Restore()` | bool | Set IsDeleted = false, DeletedAt = null |

### Core Roles (Enum)
Location: `Identity/Domain/Enums/CoreUserRole.cs`
- `SuperAdmin` - Full system access, can manage all roles/permissions
- `Admin` - Administrative access, cannot modify core system roles
- `Visitor` - Standard public user with content access

### Existing DTOs
```csharp
// Identity/Application/Shared/DTOs/
record RoleDto(Guid Id, string Name, string Description);
record PermissionDto(Guid Id, string Resource, string Action, string Description);
```

### Existing Specifications
Location: `Identity/Application/Roles/Specifications/`

**RoleSpecifications:**
- `RoleByNameSpecification(string roleName)`
- `RoleByIdSpecification(Guid roleId)`

**RolePermissionSpecifications:**
- `RolePermissionByRoleIdSpecification(Guid roleId)`
- `RolePermissionByPermissionIdSpecification(Guid permissionId)`
- `RolePermissionByRoleAndPermissionSpecification(Guid roleId, Guid permissionId)`

**UserRoleSpecifications:**
- `UserHasAdminRoleSpecification`
- `UserHasRoleSpecification(string roleName)`
- `UserHasVisitorRoleSpecification`
- `UserIsActiveAdminSpecification`

### Authorization Policies
Location: `Identity/Application/Shared/Authorizations/Policies/UserRolePolicies.cs`
- `RequireSuperAdminOnly`
- `RequireAdminOnly`
- `RequireVisitorOnly`
- `RequireAdminOrSuperAdmin`

### Predefined Visitor Permissions (28 total)
Location: `Identity/Domain/ValueObjects/VisitorPermissions.cs`

| Category | Permissions |
|----------|-------------|
| Content | articles.read, videos.read, contents.read |
| Profile | own_profile.read, own_profile.update |
| Likes | likes.create, own_likes.delete, likes.read |
| Comments | comments.read, comments.create, own_comments.update, own_comments.delete |
| Bookmarks | bookmarks.create, own_bookmarks.delete, own_bookmarks.read, bookmarks.read |
| Navigation | tags.read, categories.read |
| Playlists | playlists.create, own_playlists.update, own_playlists.delete, own_playlists.read |
| Ads | ads_banners.read, ads_stories.read |
| Rates | rates.create, rates.read |
| Shares | shares.create, shares.read, own_shares.read |

### Existing Seeders
- `VisitorRoleSeeder` - Creates Visitor role with 28 permissions
- `SuperAdminSeeder` - Seeds SuperAdmin user account

### CQRS Structure
```
Identity/Application/Roles/UseCases/
├── Admin/
│   ├── Commands/    # Create, Update, Delete, Activate, Deactivate roles
│   └── Queries/     # List, Get role details
└── Public/
    ├── Commands/
    └── Queries/
```

---

## Recommended Admin Role Endpoints

### Role CRUD

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/admin/roles` | List all roles (paginated, filterable by IsActive/IsDeleted) | Admin |
| `GET` | `/api/v1/admin/roles/{id}` | Get role details with permissions | Admin |
| `POST` | `/api/v1/admin/roles` | Create new role | SuperAdmin |
| `PUT` | `/api/v1/admin/roles/{id}` | Update role name/description | SuperAdmin |

### Role Status Management

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `PATCH` | `/api/v1/admin/roles/{id}/activate` | Activate a role | SuperAdmin |
| `PATCH` | `/api/v1/admin/roles/{id}/deactivate` | Deactivate a role | SuperAdmin |
| `DELETE` | `/api/v1/admin/roles/{id}` | Soft delete role | SuperAdmin |
| `DELETE` | `/api/v1/admin/roles/{id}/hard` | Hard delete role (permanent) | SuperAdmin |
| `PATCH` | `/api/v1/admin/roles/{id}/restore` | Restore soft-deleted role | SuperAdmin |

### Permission CRUD

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/admin/permissions` | List all permissions (filterable) | Admin |
| `GET` | `/api/v1/admin/permissions/{id}` | Get permission details | Admin |
| `POST` | `/api/v1/admin/permissions` | Create new permission | SuperAdmin |
| `PUT` | `/api/v1/admin/permissions/{id}` | Update permission | SuperAdmin |

### Permission Status Management

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `PATCH` | `/api/v1/admin/permissions/{id}/activate` | Activate a permission | SuperAdmin |
| `PATCH` | `/api/v1/admin/permissions/{id}/deactivate` | Deactivate a permission | SuperAdmin |
| `DELETE` | `/api/v1/admin/permissions/{id}` | Soft delete permission | SuperAdmin |
| `DELETE` | `/api/v1/admin/permissions/{id}/hard` | Hard delete permission | SuperAdmin |
| `PATCH` | `/api/v1/admin/permissions/{id}/restore` | Restore soft-deleted permission | SuperAdmin |

### Role-Permission Assignment

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/v1/admin/roles/{id}/permissions` | Assign permissions to role | SuperAdmin |
| `DELETE` | `/api/v1/admin/roles/{id}/permissions/{permissionId}` | Remove permission from role | SuperAdmin |
| `PUT` | `/api/v1/admin/roles/{id}/permissions` | Bulk update role permissions | SuperAdmin |

### User-Role Assignment

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/admin/users/{id}/roles` | Get user's roles | Admin |
| `POST` | `/api/v1/admin/users/{id}/roles` | Assign role to user | SuperAdmin |
| `DELETE` | `/api/v1/admin/users/{id}/roles/{roleId}` | Remove role from user | SuperAdmin |

### Implementation Rules
1. **Protect core roles**: SuperAdmin, Admin, Visitor cannot be hard deleted
2. **Deactivated roles**: Cannot be assigned to new users, existing assignments remain
3. **Soft deleted roles**: Hidden from lists by default, restorable
4. **Hard delete**: Permanent removal, cascades to UserRole/RolePermission
5. **Audit trail**: Log all role/permission changes
6. **Validation**: Role/permission names must be unique among non-deleted items
