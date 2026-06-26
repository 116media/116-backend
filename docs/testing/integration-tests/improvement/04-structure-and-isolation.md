# 04 — Structure, Fixtures & Isolation

## Directory hygiene

| Item | State | Action |
| --- | --- | --- |
| `Workflows/.gitkeep` | dir has 4 files | **remove** (done in Phase 1) |
| `Modules/Identity/Infrastructure/Mappers/.gitkeep` | dir has 3 files | **remove** (done in Phase 1) |
| `Common/Constants/` | empty, redundant with `tests/Fixtures/Constants/` | **delete dir** (done in Phase 1) |
| `Modules/Core/Infrastructure/Services/.gitkeep` | empty | **keep** — placeholder for the missing FileService tests (gap below) |

`.gitkeep` should only ever exist in an otherwise-empty directory that must be
tracked. Once real files land, the `.gitkeep` is noise.

## Known coverage gap: Core `FileService`

`Modules/Core/Infrastructure/Services/` is empty. `FileService` is only partly
exercised (via `AdminDeleteVideoHandler` / `AdminDeleteShortVideoHandler` delete
paths). Its upload/lookup paths and the `File*Specification`s have no direct
tests. Tracked as a TODO in
[`specs/infrastructure/03-fixtures-cleanup.md`](specs/infrastructure/03-fixtures-cleanup.md);
the `.gitkeep` stays until those tests exist.

## Fixtures (what they do, and the smells)

- `Common/Fixtures/PostgresFixture.cs` — single shared Testcontainer; applies the
  3 module migrations; builds a Respawn `Respawner` over schemas
  `["identity","core","content"]`. **Good** (fast, one container).
- `Common/Fixtures/ApiFixture.cs` — `WebApplicationFactory<Program>` that swaps
  DbContexts to the container, stubs `ICloudinaryService` /
  `IYoutubeThumbnailService`, overrides JWT validation, and **disables rate
  limiting**. Smell: `DisableRateLimiting()` works by reflection + matching
  `IConfigureOptions` type names — fragile across DI/runtime upgrades. Replace
  with a strongly-typed `services.Configure<RateLimiterOptions>(...)` that
  registers no-op policies for each named policy.
- `Common/Fixtures/DatabaseCollection.cs` — `[Collection("Database")]`, forces
  serial execution.
- `Common/Base/BaseApiTest.cs` — per-test `Db.ResetAsync()` (Respawn truncate) →
  seed fixed SuperAdmin/Admin/Visitor → `SeedAsync()`. `Common/Base/BaseRepositoryTest.cs`
  for direct-repo tests.
- `Common/Extensions/HttpClientExtensions.cs` — in-memory JWT minting
  (`AuthenticateAsSuperAdmin/Admin/Visitor`, `AuthenticateAs(userId, role, sessionId)`).
- `Common/Seeders/TestDataSeeder.cs` — wraps prod seeders; **underused**.
- `Common/Stubs/StubCloudinaryService.cs`, `StubYoutubeThumbnailService.cs` —
  return deterministic fakes (Cloudinary returns success URLs, so upload handlers
  ARE reachable).

## Isolation: Respawn truncate, no per-test transaction

Each test truncates the shared DB and reseeds the three fixed users. There is
**no transaction-per-test rollback**. Consequences:

- Tests must not assume a pristine DB beyond the fixed users.
- Several repository/list tests use **count-based assertions**
  (`totalCount.Should().Be(baseCount + 5)`) that are fragile under any shared
  state. Replace with **unique-key lookups** (seed entities with a unique
  name/slug, then assert that specific entity is present/filtered), which is
  isolation-independent.

Options (see [`specs/infrastructure/02-isolation.md`](specs/infrastructure/02-isolation.md)):
1. Keep Respawn truncate (current) + switch count assertions to unique-key — low
   risk, recommended baseline.
2. Add per-test transaction + rollback — stronger isolation but interacts with
   `WebApplicationFactory`'s own scopes; document trade-offs before adopting.

## Seeding boilerplate

~200 tests repeat:

```csharp
await using var seedContext = CreateDbContext<ContentDbContext>();
var customer = CustomerFactory.Create();
seedContext.Customers.Add(customer);
await seedContext.SaveChangesAsync();
```

Add a base-class helper to collapse this:

```csharp
protected async Task SeedAsync<TDbContext>(Func<TDbContext, Task> seed)
    where TDbContext : DbContext
{
    await using var ctx = CreateDbContext<TDbContext>();
    await seed(ctx);
    await ctx.SaveChangesAsync();
}
```

Spec: [`specs/infrastructure/01-seeding-helpers.md`](specs/infrastructure/01-seeding-helpers.md).

## Workflows & Shared

- `Workflows/` (4 e2e flows: auth, content-publication, interaction, order-
  lifecycle) — good coverage; upgrade to typed/body assertions and route helpers
  like everything else.
- `Shared/` (exception handler, decorators, EF interceptors) — decorator tests
  are thin smoke tests; note in the shared spec.
