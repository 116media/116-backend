# Integration Tests Documentation

Comprehensive guide for setting up and writing integration tests for the 116 Backend API.

## Documents

| # | Document | Description |
|---|----------|-------------|
| 00 | [Overview](00-overview.md) | Why integration tests, scope, technology stack, unit test gap analysis, relationship to unit tests |
| 01 | [Project Setup](01-project-setup.md) | .csproj configuration, solution setup, directory structure, environment variables |
| 02 | [Testcontainers Fixture](02-testcontainers-fixture.md) | PostgreSQL container lifecycle, Respawn database reset, collection definition |
| 03 | [WebApplicationFactory](03-webapplicationfactory.md) | API fixture, DbContext replacement, external service stubbing, auth helpers |
| 04 | [Base Test Classes](04-base-test-classes.md) | BaseRepositoryTest and BaseApiTest patterns, anti-patterns |
| 05 | [Writing Repository Tests](05-writing-repository-tests.md) | ILike queries, pagination, navigation loading, unique constraints, examples |
| 06 | [Writing API Tests](06-writing-api-tests.md) | Endpoint routing, auth, validation, error responses, rate limiting, examples |
| 07 | [External Service Stubs](07-external-service-stubs.md) | Cloudinary and YouTube stubs, when to stub vs. use real implementations |
| 08 | [Test Data Seeding](08-test-data-seeding.md) | Inline, SeedAsync, TestDataSeeder, data dependencies, Respawn configuration |
| 09 | [Authentication Testing](09-authentication-testing.md) | JWT flow, auth helpers, role-based access, authorization matrix |
| 10 | [Test Coverage Plan](10-test-coverage-plan.md) | 11-phase rollout plan, ~361 tests across 86 test classes, priority matrix |
| 11 | [Common Pitfalls](11-common-pitfalls.md) | Lessons from unit tests, integration-specific gotchas, pre-flight checklist |
| 12 | [Assertions Cheatsheet](12-assertions-cheatsheet.md) | HTTP, database, pagination, auth, collection, and ProblemDetails assertions |
| 13 | [Interceptors & Decorators](13-interceptors-decorators.md) | AuditableEntity, DomainEventDispatch interceptors, Validation and Logging decorators |
| 14 | [Mappers & Seeders](14-mappers-seeders.md) | Round-trip mapper tests, SuperAdmin/Visitor/ContentType seeder tests |
| 15 | [Cross-Module Workflows](15-workflow-tests.md) | Authentication, content publication, interaction, and order lifecycle flows |

## Quick Start

```bash
# 1. Ensure Docker is running
docker info

# 2. Create the project (if not yet created)
cd apps/backend
dotnet new xunit -n _116.Integration.Tests -o tests/Integration --framework net9.0

# 3. Install packages
cd tests/Integration
dotnet add package Testcontainers.PostgreSql
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Respawn
dotnet add package AwesomeAssertions

# 4. Run tests
dotnet test tests/Integration
```

## Architecture

```
┌─────────────────────────────────────────┐
│              Test Method                │
│  [Fact] Get_ShouldReturn200()          │
└───────────────┬─────────────────────────┘
                │
    ┌───────────▼───────────┐
    │   BaseApiTest         │
    │   - HttpClient        │
    │   - ApiFixture        │
    │   - PostgresFixture   │
    └───────────┬───────────┘
                │
    ┌───────────▼───────────────────────┐
    │   ApiFixture (WebApplicationFactory)│
    │   - Full ASP.NET Core pipeline    │
    │   - Real DI container             │
    │   - Stubbed external services     │
    └───────────┬───────────────────────┘
                │
    ┌───────────▼───────────────────────┐
    │   PostgresFixture (Testcontainers)│
    │   - Real PostgreSQL in Docker     │
    │   - EF Core migrations applied    │
    │   - Respawn for fast reset        │
    └───────────────────────────────────┘
```

## Component Coverage Summary

| Category | Total Components | Unit Tested | Integration Tests Needed |
|----------|-----------------|-------------|-------------------------|
| Endpoints (AddRoutes) | 211 | 0 (response records only) | 211 |
| Repositories | 18 | 18 (3 skip ILike) | 3+ |
| Interceptors | 2 | 0 | 2 |
| Decorators | 2 | 0 | 2 |
| Seeders | 3 | 1 | 2 |
| Mappers | 15 | 4 | 11 |
| Interaction Entities | 13 | 0 | 13 |
| EF Core Configurations | ~30 | 0 | ~30 |
| Module Registration | 3 | 0 | 3 |
| Background Jobs | 1 | 1 (handler) | 1 |
| Cross-Module Flows | N/A | 0 | 4 |

## Key Principles

1. **Real database** — Testcontainers PostgreSQL, never InMemory
2. **Real pipeline** — WebApplicationFactory boots the full app
3. **Stub only network** — Cloudinary and YouTube are stubbed; everything else is real
4. **Fast reset** — Respawn truncates tables between test classes
5. **Reuse fixtures** — Builders, factories, and constants from `_116.Tests.Fixtures`
6. **No duplication** — Don't re-test handler logic that unit tests already cover
7. **Self-contained** — Each test works in isolation, no ordering dependencies
