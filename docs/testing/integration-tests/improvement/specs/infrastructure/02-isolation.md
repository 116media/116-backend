# Infra Spec 02 — Isolation & count-assertion fragility

## Problem
The shared Postgres DB is truncated per test (Respawn) with **no transaction
rollback**. Some tests assert on **counts** (`baseCount + N`), which is fragile
under any shared/residual state and couples unrelated tests.

## Before
```csharp
var (_, baseCount) = await repo.GetAllWithPaginationAsync(page: 1, pageSize: 100);
// seed 5 roles
var (result, totalCount) = await repo.GetAllWithPaginationAsync(page: 1, pageSize: 3);
totalCount.Should().Be(baseCount + 5);   // fragile
```

## After — assert specific seeded entities by unique key
```csharp
var marker = $"iso-{Guid.NewGuid():N}";
// seed roles whose names start with `marker`
var (result, _) = await repo.GetAllWithPaginationAsync(page: 1, pageSize: 100);
result.Where(r => r.Name.StartsWith(marker)).Should().HaveCount(5);
```

## Decision: transaction-per-test?
Two options — pick #1 as baseline:
1. **Keep Respawn truncate + unique-key assertions** (recommended). Low risk,
   no interaction with `WebApplicationFactory` scopes.
2. **Per-test transaction + rollback.** Stronger isolation but the API runs in
   its own DI scope/connection under `WebApplicationFactory`, so a test-owned
   transaction won't wrap the server's writes cleanly. Only pursue with a
   documented connection-sharing strategy.

## TODO checklist
- [ ] Replace count-based assertions with unique-key assertions. Known sites:
  - [ ] `Modules/Identity/Infrastructure/Repositories/RoleRepositoryTests.cs`
  - [ ] other `*RepositoryTests.cs` using `baseCount`/`+N` (sweep `grep -rn 'baseCount\|+ [0-9]\+)' tests/Integration`)
- [ ] Document the chosen isolation model in `../04-structure-and-isolation.md`.

## Acceptance
- No assertion depends on the absolute row count of the shared DB.
- `grep -rn 'baseCount' tests/Integration` → 0.
