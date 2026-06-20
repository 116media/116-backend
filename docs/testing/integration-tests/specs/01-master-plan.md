# Integration Test Master Plan

## Goal

Achieve 100% integration test coverage across all layers of the 116 backend. Unit tests currently cover 64% — integration tests will cover the remaining gaps and verify real infrastructure behavior (database, HTTP pipeline, middleware, auth, rate limiting).

## Execution Rules

1. **Verify twice before marking done** — run the test, read the output, run again
2. **No src/ changes unless critical** — document every src/ change in `00-progress-and-memory.md`
3. **Tests must pass** — `./scripts/run-tests-with-coverage.sh integration` must succeed before moving to the next phase
4. **Follow existing patterns** — use conventions from docs 01-15
5. **One phase at a time** — complete and verify each phase before starting the next

## Phase Execution Order

```
Phase 0: Project Setup ──────────────────────────────► (spec: 02-project-setup-spec.md)
    │
Phase 1: Test Infrastructure ────────────────────────► (spec: 03-test-infrastructure-spec.md)
    │
Phase 2: Shared Layer ──────────────────────────────► (spec: 04-shared-layer-spec.md)
    │
    ├── Phase 3: Identity Repositories ──────────────► (spec: 05-identity-repositories-spec.md)
    ├── Phase 4: Identity API ───────────────────────► (spec: 06-identity-api-spec.md)
    ├── Phase 5: Identity Services ──────────────────► (spec: 07-identity-services-spec.md)
    │
Phase 6: Core Module ───────────────────────────────► (spec: 08-core-module-spec.md)
    │
    ├── Phase 7: Content Repositories ───────────────► (spec: 09-content-repositories-spec.md)
    ├── Phase 8: Content Catalog API ────────────────► (spec: 10-content-catalog-spec.md)
    ├── Phase 9: Content Commerce API ───────────────► (spec: 11-content-commerce-spec.md)
    ├── Phase 10: Content Editorial API ─────────────► (spec: 12-content-editorial-spec.md)
    ├── Phase 11: Content Interactions API ──────────► (spec: 13-content-interactions-spec.md)
    ├── Phase 12: Content Lookup API ────────────────► (spec: 14-content-lookup-spec.md)
    ├── Phase 13: Content Mappers ───────────────────► (spec: 15-content-mappers-spec.md)
    ├── Phase 14: Seeders ──────────────────────────► (spec: 16-seeders-spec.md)
    │
Phase 15: Cross-Module Workflows ───────────────────► (spec: 17-workflow-spec.md)
    │
Phase 16: Identity Mappers ─────────────────────────► (spec: 18-identity-mappers-spec.md)
```

## Test Count Estimates

| Phase | Layer | Est. Test Files | Est. Test Methods |
|-------|-------|-----------------|-------------------|
| 0-1 | Setup + Infrastructure | 8 | 0 (infrastructure) |
| 2 | Shared (interceptors, decorators, middleware) | 7 | ~35 |
| 3 | Identity repositories | 7 | ~70 |
| 4 | Identity API endpoints | 27 | ~200 |
| 5 | Identity services | 8 | ~60 |
| 6 | Core module | 3 | ~15 |
| 7 | Content repositories | 10 | ~120 |
| 8 | Content catalog API | 12 | ~80 |
| 9 | Content commerce API | 10 | ~70 |
| 10 | Content editorial API | 20 | ~150 |
| 11 | Content interactions API | 15 | ~100 |
| 12 | Content lookup API | 10 | ~60 |
| 13 | Content mappers | 13 | ~65 |
| 14 | Seeders | 3 | ~15 |
| 15 | Cross-module workflows | 5 | ~25 |
| 16 | Identity mappers | 3 | ~15 |
| **Total** | | **~161** | **~1080** |

## File Structure

```
tests/
└── _116.Integration.Tests/
    ├── _116.Integration.Tests.csproj
    ├── GlobalUsings.cs
    ├── Common/
    │   ├── Fixtures/
    │   │   ├── PostgresFixture.cs
    │   │   └── ApiFixture.cs
    │   ├── Base/
    │   │   ├── BaseApiTest.cs
    │   │   └── BaseRepositoryTest.cs
    │   ├── Extensions/
    │   │   └── HttpClientExtensions.cs
    │   ├── Seeders/
    │   │   └── TestDataSeeder.cs
    │   ├── Stubs/
    │   │   ├── StubCloudinaryService.cs
    │   │   └── StubYoutubeThumbnailService.cs
    │   └── Constants/
    │       └── TestConstants.cs (partial classes)
    ├── Shared/
    │   ├── Interceptors/
    │   │   ├── AuditableEntityInterceptorTests.cs
    │   │   └── DispatchDomainEventsInterceptorTests.cs
    │   ├── Decorators/
    │   │   ├── ValidationDecoratorTests.cs
    │   │   └── LoggingDecoratorTests.cs
    │   ├── ExceptionHandlers/
    │   │   └── (13 handler tests)
    │   └── Middleware/
    │       └── (2 middleware tests)
    ├── Identity/
    │   ├── Repositories/
    │   │   └── (7 repository test files)
    │   ├── Api/
    │   │   ├── Auth/
    │   │   ├── Roles/
    │   │   ├── Session/
    │   │   └── User/
    │   ├── Services/
    │   │   └── (8 service test files)
    │   └── Mappers/
    │       └── (3 mapper test files)
    ├── Core/
    │   ├── Repositories/
    │   └── Services/
    └── Content/
        ├── Repositories/
        │   └── (10 repository test files)
        ├── Api/
        │   ├── Catalog/
        │   ├── Commerce/
        │   ├── Editorial/
        │   ├── Interactions/
        │   └── Lookup/
        ├── Mappers/
        │   └── (13 mapper test files)
        └── Seeders/
            └── (seeder test files)
```

## Verification Protocol

After completing each phase:

1. Run `dotnet build` — must compile
2. Run `./scripts/run-tests-with-coverage.sh integration` — must pass
3. Read `coverage/report/Summary.txt` to check coverage numbers
4. Review test output for flaky tests
5. Update `00-progress-and-memory.md` with checkmarks and latest coverage %
6. If any src/ file was changed, add entry to Source Code Changes Log
