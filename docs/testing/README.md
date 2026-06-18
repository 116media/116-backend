# Content Module — Unit Test Plan

> **Read this file first** at the start of any session working on Content module tests.
> Last updated: 2026-03-10 (Session 3 — endpoint record tests + Identity GetOwnRoles)

---

## Status: COMPLETE ✅

**199 test files written — ~3806 tests passing, 20 skipped (ILike/InMemory + seeder), 0 failing**

Coverage: **30.5% → ~85-90%+** (Session 2 boost; Session 3 covers endpoint records)

---

## Index of Files in This Folder

| File | Purpose |
|------|---------|
| README.md | This file — entry point, current status |
| TODO.md | Complete checkbox list of every test file — all done |
| PROGRESS.md | Session-by-session detailed log of what was done |
| PATTERNS.md | Test patterns, conventions, exact code templates |
| SOURCE_ANALYSIS.md | Every source file analyzed — entities, handlers, validators, repos |
| FIXTURES.md | Builders, Factories, Constants created in tests/Fixtures/ |
| MOCKS.md | Mock classes and their methods |
| CSPROJ_CHANGES.md | .csproj changes made |

---

## What Was Done — Session 1 (2026-03-10)

### 1. Project Setup
- Added `Content.csproj` reference to both `tests/Unit/_116.Unit.Tests.csproj` and `tests/Fixtures/_116.Tests.Fixtures.csproj`
- Added `Content` nested class to `tests/Fixtures/Constants/TestConstants.cs`
- Created `tests/Unit/Common/BaseContentHandlerTest.cs` (mapper setup)

### 2. Fixtures Created (`tests/Fixtures/`)
- **9 Builders** in `Builders/Entities/Content/` — one per entity
- **9 Factories** in `Factories/Content/` — one per entity

### 3. Mocks Created (`tests/Unit/Common/Mocks/`)
- `Repositories/MockLookupRepository.cs`
- `Repositories/MockCategoryRepository.cs`
- `Repositories/MockCustomerRepository.cs`
- `Repositories/MockPackageRepository.cs`
- `Infrastructure/MockContentUnitOfWork.cs`

### 4. Test Files Created (`tests/Unit/Modules/Content/`)
```
Domain/Entities/          9 files  — ContentType, PricingTier, PromotionLevel, Tag,
                                      Category, CategoryPricing, Customer, Package, PackageSlot

Application/Shared/Errors/ 7 files  — all error factory classes

Application/Lookup/
  Specifications/           1 file   — all 10 lookup specs (extended in Session 2)
  UseCases/Admin/Commands/ 26 files  — 13 handlers + 12 validators (UpdateContentType added S2)
  UseCases/Admin/Queries/   3 files  — GetAll* handlers

Application/Catalog/
  Specifications/           1 file   — all catalog specs (extended in Session 2)
  UseCases/Admin/Commands/ 30 files  — 14 handlers + 10 validators + 4 activate/deactivate validators
                                        + ActivatePackage + DeactivatePackage validators (added S2)
  UseCases/Admin/Queries/   6 files  — GetAll* + GetById* handlers
  UseCases/Public/Queries/  1 file   — GetPublicCategories

Infrastructure/
  Repositories/             4 files  — Lookup, Category, Customer, Package
  Persistence/              2 files  — ContentDbContext, ContentUnitOfWork
  Seeds/                    1 file   — ContentTypeSeeder (added Session 2)

ContentModuleTests.cs       1 file   — module registration (extended in Session 2)
```

---

## What Was Done — Session 2 (2026-03-10 — Coverage Boost)

### Problem
Coverage was 30.5% despite 86 test files. Root cause: 39 MetaField classes (all 0%),
missing validators, missing specs, uncovered `isRequired=false` branches in shared validators,
and missing collection mapper extension tests.

### New Test Files Added (11 files)

| File | Tests | Classes Fixed |
|------|-------|---------------|
| `Application/Lookup/MetaFields/LookupMetaFieldTests.cs` | 18 | 18 Lookup MetaField classes (0% → 100%) |
| `Application/Catalog/MetaFields/CatalogMetaFieldTests.cs` | 21 | 21 Catalog MetaField classes (0% → 100%) |
| `Lookup/UseCases/.../UpdateContentType/UpdateContentTypeValidatorTests.cs` | 4 | `UpdateContentTypeValidator` (0% → 100%) |
| `Catalog/UseCases/.../ActivatePackage/ActivatePackageValidatorTests.cs` | 2 | `ActivatePackageValidator` (0% → 100%) |
| `Catalog/UseCases/.../DeactivatePackage/DeactivatePackageValidatorTests.cs` | 2 | `DeactivatePackageValidator` (0% → 100%) |
| `Infrastructure/Seeds/ContentTypeSeederTests.cs` | 5 | `ContentTypeSeeder` (14.2% → ~100%) |
| `Application/Shared/Validators/SharedValidatorsTests.cs` | 24 | 5 shared validator classes (59-74% → ~100%) |
| `Application/Shared/Mappers/MapperExtensionTests.cs` | 10 | Mapper extensions (75-92% → ~100%) |

### Existing Files Extended (3 files)

| File | Added Tests | What |
|------|------------|------|
| `LookupSpecificationsTests.cs` | +7 | ContentTypeByName/PricingTierByName/PromotionLevelByName (ILike ToExpression-only), TagByName (full eval), TagSearch (ILike ToExpression-only) |
| `CatalogSpecificationsTests.cs` | +2 | `InactivePackageSpecification` (0% → 100%) |
| `ContentModuleTests.cs` | +1 | Testing environment branch for `GetModuleOptions` |

### Coverage Impact
Before Session 2: **30.5%** | After Session 2: **~85-90%**

---

## What Was Done — Session 3 (2026-03-10 — Identity GetOwnRoles + Endpoint Records)

### Identity GetOwnRoles (3 new files, 14 tests)
- `Roles/UseCases/Admin/Queries/GetOwnRoles/AdminGetOwnRolesHandlerTests.cs` — 6 tests
- `Roles/UseCases/Public/Queries/GetOwnRoles/PublicGetOwnRolesHandlerTests.cs` — 6 tests
- `Roles/MetaFields/GetOwnRolesMetaFieldTests.cs` — 2 tests (AdminGetOwnRolesMetaField + PublicGetOwnRolesMetaField)

### Endpoint Record Tests (99 new files, ~121 tests)
Records defined inside `*EndpointV1.cs` files (`*Response`, `*Request`) can be unit-tested by simple instantiation — no real HTTP host needed. Only records with nullable parameters show as coverable in Coverlet; records with all non-nullable parameters are excluded from the report.

**99 individual test files**, one per endpoint file, named `{EndpointClassName}Tests.cs` in the exact mirrored directory:

| Group | Files |
|-------|-------|
| Identity Auth (`Auth/UseCases/.../V1/`) | 19 |
| Identity Session (`Session/UseCases/.../V1/`) | 9 |
| Identity User (`User/UseCases/.../V1/`) | 9 |
| Identity Roles/Permissions (`Roles/UseCases/.../V1/`) | 23 |
| Content Lookup (`Lookup/UseCases/.../V1/`) | 18 |
| Content Catalog (`Catalog/UseCases/.../V1/`) | 21 |

### Bug Fixed
`PublicSignUpEndpointV1.cs` namespace was `SignUp.v1` (lowercase) — fixed to `SignUp.V1`.

### Coverage Impact
After Session 3: **~3806 tests passing, 20 skipped, 0 failing**

---

## Quick Reference

### Run Content tests only
```bash
dotnet test tests/Unit/_116.Unit.Tests.csproj --filter "FullyQualifiedName~Content"
```

### Run all unit tests
```bash
dotnet test tests/Unit/_116.Unit.Tests.csproj
```

### Key test file locations
| What | Where |
|------|-------|
| Entity tests | `tests/Unit/Modules/Content/Domain/Entities/` |
| Lookup handler tests | `tests/Unit/Modules/Content/Application/Lookup/UseCases/` |
| Catalog handler tests | `tests/Unit/Modules/Content/Application/Catalog/UseCases/` |
| MetaField tests | `tests/Unit/Modules/Content/Application/Lookup/MetaFields/` and `.../Catalog/MetaFields/` |
| Shared validator tests | `tests/Unit/Modules/Content/Application/Shared/Validators/SharedValidatorsTests.cs` |
| Mapper extension tests | `tests/Unit/Modules/Content/Application/Shared/Mappers/MapperExtensionTests.cs` |
| Seeder tests | `tests/Unit/Modules/Content/Infrastructure/Seeds/ContentTypeSeederTests.cs` |
| Repository tests | `tests/Unit/Modules/Content/Infrastructure/Repositories/` |
| Builders | `tests/Fixtures/Builders/Entities/Content/` |
| Factories | `tests/Fixtures/Factories/Content/` |
| Mocks | `tests/Unit/Common/Mocks/Repositories/Mock{Lookup,Category,Customer,Package}Repository.cs` |

---

## Known Limitations

### ILike (PostgreSQL case-insensitive) not supported by InMemoryDatabase
- `CategoryRepositoryTests.GetBySlugAsync_WhenFound` — SKIPPED
- `CustomerRepositoryTests.GetByEmailAsync_WhenFound` — SKIPPED
- `LookupRepositoryTests` — ExistsByName* methods not tested (commented out)
- `ContentTypeByNameSpecification`, `PricingTierByNameSpecification`, `PromotionLevelByNameSpecification`, `TagSearchSpecification` — ToExpression/Compile only, no in-memory evaluation

These require integration tests with a real PostgreSQL instance.

### EndpointV1 `AddRoutes` methods remain at 0%
The `ICarterModule.AddRoutes(IEndpointRouteBuilder app)` method inside each `*EndpointV1` class still requires integration tests with a real ASP.NET Core host. The `*Response` and `*Request` record constructors inside those same files are now covered by unit tests.

---

## Next Steps (future sessions)

1. Add integration tests for ILike-based repository methods
2. Add `EndpointV1.AddRoutes` integration tests (Carter endpoints) — new test project needed
3. Expand catalog tests as more sub-modules are added (03-editorial, 04-commerce, 05-interactions)
