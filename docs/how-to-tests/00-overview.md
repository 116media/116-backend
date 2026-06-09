# Test Infrastructure Overview

This guide documents the complete test infrastructure for the 116 backend. Every pattern here is derived directly from existing test files — nothing is invented.

---

## Directory Structure

```
tests/
├── Fixtures/                              # Shared test data — referenced by ALL tests
│   ├── Builders/
│   │   ├── AuthDataBuilder.cs
│   │   ├── Commands/
│   │   │   └── Roles/
│   │   │       ├── CreatePermissionCommandBuilder.cs
│   │   │       ├── CreateRoleCommandBuilder.cs
│   │   │       ├── UpdatePermissionCommandBuilder.cs
│   │   │       └── UpdateRoleCommandBuilder.cs
│   │   └── Entities/
│   │       ├── FileBuilder.cs
│   │       ├── OtpBuilder.cs
│   │       ├── PermissionBuilder.cs
│   │       ├── RoleBuilder.cs
│   │       ├── RolePermissionBuilder.cs
│   │       ├── SessionBuilder.cs
│   │       ├── UserBuilder.cs
│   │       ├── UserRoleBuilder.cs
│   │       └── Content/
│   │           ├── ArticleBuilder.cs
│   │           ├── ArticleImageBuilder.cs
│   │           ├── CategoryBuilder.cs
│   │           ├── CategoryPricingBuilder.cs
│   │           ├── ContentItemTierBuilder.cs
│   │           ├── ContentOrderBuilder.cs
│   │           ├── ContentOrderItemBuilder.cs
│   │           ├── ContentPaymentBuilder.cs
│   │           ├── ContentTypeBuilder.cs
│   │           ├── CustomerBuilder.cs
│   │           ├── LyricsBuilder.cs
│   │           ├── PackageBuilder.cs
│   │           ├── PackageSlotBuilder.cs
│   │           ├── ShortVideoBuilder.cs
│   │           ├── VideoBuilder.cs
│   │           └── PricingTierBuilder.cs
│   ├── Constants/
│   │   └── TestConstants.cs
│   ├── Factories/
│   │   ├── CommandFactory.cs
│   │   ├── FileFactory.cs
│   │   ├── OtpFactory.cs
│   │   ├── PermissionFactory.cs
│   │   ├── RoleFactory.cs
│   │   ├── RolePermissionFactory.cs
│   │   ├── SessionFactory.cs
│   │   ├── UserFactory.cs
│   │   └── Content/
│   │       ├── ArticleFactory.cs
│   │       ├── ArticleImageFactory.cs
│   │       ├── CategoryFactory.cs
│   │       ├── CategoryPricingFactory.cs
│   │       ├── ContentItemTierFactory.cs
│   │       ├── ContentOrderFactory.cs
│   │       ├── ContentOrderItemFactory.cs
│   │       ├── ContentPaymentFactory.cs
│   │       ├── ContentTypeFactory.cs
│   │       ├── CustomerFactory.cs
│   │       ├── LyricsFactory.cs
│   │       ├── PackageFactory.cs
│   │       ├── PackageSlotFactory.cs
│   │       ├── PricingTierFactory.cs
│   │       ├── PromotionLevelFactory.cs
│   │       ├── ShortVideoFactory.cs
│   │       ├── TagFactory.cs
│   │       └── VideoFactory.cs
│   └── Helpers/
│       ├── AuthTestHelpers.cs
│       ├── FileTestHelpers.cs
│       └── HttpTestHelpers.cs
│
└── Unit/                                  # xUnit test project
    ├── _116.Unit.Tests.csproj
    ├── Common/
    │   ├── BaseHandlerTest.cs
    │   ├── BaseContentHandlerTest.cs
    │   └── Mocks/
    │       ├── Repositories/              # 17 mock repository helpers
    │       ├── Infrastructure/            # MockDispatcher, MockUnitOfWork x3
    │       ├── Services/                  # 7 mock service helpers
    │       └── Factories/                 # 5 mock factory-interface helpers
    ├── BuildingBlocks/
    ├── Shared/
    └── Modules/
        ├── Content/
        │   ├── Domain/
        │   └── Application/
        │       ├── Catalog/
        │       ├── Commerce/
        │       ├── Editorial/
        │       ├── Interactions/
        │       └── Lookup/
        ├── Core/
        └── Identity/
```

---

## NuGet Packages

**File:** `tests/Unit/_116.Unit.Tests.csproj`

| Package | Version | Purpose |
|---------|---------|---------|
| `xunit.v3` | 1.1.0 | Test runner |
| `AwesomeAssertions` | 9.0.0 | Fluent assertions (`.Should()`) |
| `Moq` | 4.20.72 | Mocking |
| `Bogus` | 35.6.3 | Fake data generation |
| `Microsoft.EntityFrameworkCore.InMemory` | 9.0.4 | In-memory DB for mapper/repository tests |
| `Microsoft.EntityFrameworkCore.Sqlite` | 9.0.4 | SQLite alternative for integration tests |
| `coverlet.msbuild` / `coverlet.collector` | 6.0.4 | Coverage reporting |

---

## Two Base Test Classes

### `BaseHandlerTest`
**File:** `tests/Unit/Common/BaseHandlerTest.cs`
**Use for:** Identity and Core module handler tests that need the Mapster mapper.

```csharp
public abstract class BaseHandlerTest
{
    protected readonly IMapper Mapper;

    protected BaseHandlerTest()
    {
        TypeAdapterConfig config = MappingRegistration.CreateConfiguration(); // Identity module
        Mapper = new Mapper(config);
    }
}
```

### `BaseContentHandlerTest`
**File:** `tests/Unit/Common/BaseContentHandlerTest.cs`
**Use for:** Content module handler/mapper tests that need the Mapster mapper.

```csharp
public abstract class BaseContentHandlerTest
{
    protected readonly IMapper Mapper;

    protected BaseContentHandlerTest()
    {
        TypeAdapterConfig config = MappingRegistration.CreateConfiguration(); // Content module
        Mapper = new Mapper(config);
    }
}
```

**Rule:** Only extend a base class if the test class needs `Mapper`. Domain entity tests, specification tests, and validator tests do NOT use a base class.

---

## Naming Conventions

### Test class names
Mirror the file under test with a `Tests` suffix:
```
AdminPublishArticleHandler       → AdminPublishArticleHandlerTests
AdminSubmitArticleValidator      → AdminSubmitArticleValidatorTests
ContentOrderByIdSpecification    → ContentOrderSpecificationTests (group all specs)
ContentOrderMapper               → ContentOrderMapperTests
AdminAddItemTierFactory          → AdminAddItemTierFactoryTests
ArticleEntity                    → ArticleEntityTests
```

### Test method names
Pattern: `MethodName_WhenCondition_ShouldExpectedBehavior`

```csharp
// Good
Handle_WhenArticleIsApproved_ShouldPublishAndReturnSuccess
Handle_WhenArticleNotFound_ShouldThrowNotFoundException
Handle_WhenArticleAlreadyPublished_ShouldThrowConflictException
Submit_WhenDraft_ShouldTransitionToPendingPayment
Submit_WhenNotDraft_ShouldThrowConflictException
Validate_WithValidData_ShouldNotHaveErrors
Validate_WithEmptyId_ShouldHaveError
ByIdSpec_WhenIdMatches_ShouldReturnTrue
```

### File placement
Tests mirror the source structure under `tests/Unit/Modules/`:
```
src/Modules/Content/Application/Editorial/UseCases/Admin/Commands/PublishArticle/
  AdminPublishArticleHandler.cs

tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/PublishArticle/
  AdminPublishArticleHandlerTests.cs
```

---

## Two Test Projects

### `tests/Fixtures` (test data project)
- Referenced by the unit test project
- Contains builders, factories, constants, helpers
- No test classes, no [Fact] attributes

### `tests/Unit` (unit test project)
- References the Fixtures project
- Contains all test classes
- References all production modules (Identity, Core, Content, Shared, BuildingBlocks)

---

## Global Usings

Both test projects use `ImplicitUsings: enable`. Common namespaces available globally — check for a `GlobalUsings.cs` or `_GlobalUsings.cs` if specific imports are missing.

---

## Test Categories by Layer

| Layer | Base Class | Mocks | Pattern |
|-------|-----------|-------|---------|
| Domain entity | None | None | Direct entity construction, state transitions |
| Application handler | Optional (if Mapper needed) | Repository + UnitOfWork | Constructor injection |
| Application validator | None | None | Direct validator instantiation |
| Application specification | None | None | `ToExpression().Compile()` or `IsSatisfiedBy()` |
| Application mapper | `BaseContentHandlerTest` | Optional (if repo needed) | Extension method calls, InMemory DB |
| Application factory | None | Repository + UnitOfWork | Factory as SUT |
