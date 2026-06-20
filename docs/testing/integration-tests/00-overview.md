# Integration Tests Overview

## Why Integration Tests

Unit tests verify isolated logic — handlers, validators, entities — with mocked dependencies. They cannot catch:

- **Database behavior**: ILike case-insensitive queries, unique constraints, cascade deletes, snake_case column mapping, schema isolation, transactions
- **HTTP pipeline**: Carter endpoint routing, middleware ordering, rate limiting, authentication/authorization, API versioning, CORS, exception-to-ProblemDetails conversion
- **DI wiring**: Module registration correctness, service lifetime conflicts, interceptor ordering
- **Interceptors and decorators**: AuditableEntityInterceptor (audit fields), DispatchDomainEventsInterceptor (event publishing), ValidationDecorator, LoggingDecorator — all 4 have 0% unit test coverage
- **Cross-module interactions**: Identity auth tokens consumed by Content endpoints, Core file references from Content entities, order-to-payment lifecycle
- **EF Core specifics**: Navigation property loading, query translation (LINQ-to-SQL), migration correctness, entity configurations (FK cascades, unique indexes), concurrency handling
- **Seeders**: SuperAdminSeeder and VisitorRoleSeeder are not unit tested (EF Core change tracking issues)
- **Mappers**: 11 of 15 mappers have no unit tests — round-trip mapping needs real data
- **Interaction entities**: 13 domain entities (likes, bookmarks, shares, comments, ratings) have 0% unit test coverage
- **Background jobs**: AbandonedDraftCleanupJob handler is unit tested but full execution against real data is not
- **Endpoint routing**: All 211 Carter endpoints have unit tests for response record construction only — the actual `AddRoutes()` method (routing, auth, rate limiting) is at 0%

## Unit Test Gap Analysis

The unit test suite (6100+ tests, 3 skipped) has the following known gaps that integration tests must fill:

| Component | Total | Unit Tested | Gap | Notes |
|-----------|-------|-------------|-----|-------|
| Carter Endpoints (AddRoutes) | 211 | 0 (response records only) | 211 | Routing, auth, pipeline untested |
| Repositories | 18 | 18 (3 skip ILike) | 3 | ILike queries deferred |
| Interceptors | 2 | 0 | 2 | AuditableEntity, DomainEventDispatch |
| Decorators | 2 | 0 | 2 | Validation, Logging |
| Seeders | 3 | 1 | 2 | SuperAdmin, VisitorRole |
| Mappers | 15 | 4 | 11 | Most Content + all Identity mappers |
| Interaction Entities | 13 | 0 | 13 | Likes, bookmarks, shares, comments, ratings |
| EF Core Configurations | ~30 | 0 | ~30 | FK cascades, unique constraints, indexes |
| Module Registration | 3 | 0 | 3 | DI wiring for Identity, Core, Content |
| Background Jobs | 1 | 1 (handler) | 1 | Full execution untested |
| Cross-Module Flows | N/A | 0 | N/A | Auth→Content, Order→Payment, File→Content |

## Scope

Integration tests for this project cover four layers:

| Layer | What It Tests | Database | HTTP Pipeline |
|-------|---------------|----------|---------------|
| **Repository integration** | EF Core queries against real PostgreSQL | Real (Testcontainers) | No |
| **API integration** | Full HTTP request/response cycle | Real (Testcontainers) | Yes (WebApplicationFactory) |
| **Infrastructure integration** | Interceptors, decorators, seeders, mappers, module wiring | Real (Testcontainers) | Partial |
| **Workflow integration** | End-to-end cross-module flows | Real (Testcontainers) | Yes |

## Technology Stack

| Package | Version | Purpose |
|---------|---------|---------|
| `Testcontainers.PostgreSql` | latest | Disposable PostgreSQL container per test class |
| `Microsoft.AspNetCore.Mvc.Testing` | 9.0.x | `WebApplicationFactory<T>` for in-process HTTP testing |
| `Respawn` | latest | Fast database reset between tests (truncate, not recreate) |
| `xunit.v3` | 1.1.0 | Test framework (same as unit tests) |
| `AwesomeAssertions` | 9.0.0 | Assertion library (same as unit tests) |
| `Bogus` | 35.6.3 | Fake data generation (same as unit tests) |

## Project Structure

```
tests/
├── Fixtures/                              # Shared (already exists)
│   ├── Builders/
│   ├── Factories/
│   ├── Constants/
│   └── Helpers/
├── Unit/                                  # Existing unit tests
│   └── ...
└── Integration/                           # NEW
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

## Key Design Decisions

### 1. Testcontainers over InMemory/SQLite

InMemory and SQLite do not support:
- PostgreSQL `ILike` (case-insensitive search)
- Schema separation (`identity`, `core`, `content`)
- `UseSnakeCaseNamingConvention()` behavior
- PostgreSQL-specific types and index behavior

Testcontainers spins up a real PostgreSQL instance in Docker, giving exact parity with production.

### 2. Respawn over Recreate

Dropping and recreating the database between tests is slow. Respawn uses `TRUNCATE ... CASCADE` to reset data in milliseconds while preserving schema. This makes test isolation fast.

### 3. Shared Container, Isolated Data

One PostgreSQL container is shared across all tests in a collection (via xUnit `ICollectionFixture`). Each test class gets a clean database state via Respawn. This balances startup cost against isolation.

### 4. Reuse Existing Fixtures

Builders, factories, and constants from `_116.Tests.Fixtures` are shared between unit and integration tests. No duplication.

### 5. No Mocks in Integration Tests

Integration tests use real implementations — real repositories, real DbContexts, real HTTP pipeline. The only things replaced are external services (Cloudinary, YouTube thumbnail) which are stubbed to avoid network calls.

## Relationship to Unit Tests

| Concern | Unit Tests | Integration Tests |
|---------|-----------|-------------------|
| Handler logic | Mocked repos, verify calls | Skip (covered by unit) |
| Validator rules | TestValidateAsync | Skip (covered by unit) |
| Entity domain logic (most) | Direct construction | Skip (covered by unit) |
| Interaction entities (13) | Not tested | Domain logic + DB persistence |
| Repository queries | Skipped (ILike, etc.) | Real PostgreSQL |
| Endpoint routing (211) | Response records only | Full HTTP cycle |
| Auth/authz pipeline | Mocked ClaimsPrincipal | Real JWT tokens |
| Rate limiting | Not tested | Real middleware |
| Error responses | Exception type only | Full ProblemDetails |
| Database constraints (~30) | Not tested | Unique, FK, cascade |
| Interceptors (2) | Not tested | Real EF Core pipeline |
| Decorators (2) | Not tested | Real CQRS pipeline |
| Seeders (2) | Skipped (EF issues) | Real DB seeding |
| Mappers (11) | Not tested | Round-trip with real data |
| Module registration (3) | Not tested | Full DI container |
| Background jobs (1) | Handler only | Full execution |
| Cross-module flows | Not tested | End-to-end scenarios |

## Running Integration Tests

The project includes a coverage script at `scripts/run-tests-with-coverage.sh` that handles Docker validation, coverage collection, and report generation.

```bash
# Run integration tests only (with coverage)
./scripts/run-tests-with-coverage.sh integration

# Run all tests — unit first, then integration (coverage merged)
./scripts/run-tests-with-coverage.sh all

# Run unit tests only (no Docker required)
./scripts/run-tests-with-coverage.sh unit
```

The script:

1. Checks that Docker is running (exits with error if not)
2. Sets `DOTNET_ENVIRONMENT=Testing` so modules disable seeding/migrations
3. Collects coverage via `coverlet.msbuild` (accurate with C# 12 primary constructors)
4. Merges integration coverage with unit coverage into a single report
5. Generates HTML + text summary at `coverage/report/index.html`

### Running without the script

```bash
# Prerequisites: Docker must be running

# Run all integration tests
dotnet test tests/Integration

# Run a specific module
dotnet test tests/Integration --filter "Category=Content"

# Run with verbose output
dotnet test tests/Integration --logger "console;verbosity=detailed"
```

## CI/CD Considerations

- Integration tests require Docker (GitHub Actions: `services: postgres` or Docker-in-Docker)
- They are slower than unit tests — run separately in CI pipeline
- Suggested CI split:
  - `./scripts/run-tests-with-coverage.sh unit` — fast gate (< 30s)
  - `./scripts/run-tests-with-coverage.sh integration` — thorough gate (< 2 min)
  - `./scripts/run-tests-with-coverage.sh all` — full gate with merged coverage
