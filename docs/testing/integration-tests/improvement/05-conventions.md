# 05 — Integration Test Conventions

The standard every endpoint test must follow after the overhaul. New tests that
violate these should not be merged.

## Structure

- One test class per endpoint, mirroring the src path:
  `Modules/<Module>/Application/<Area>/UseCases/<Scope>/<Commands|Queries>/<UseCase>/V1/<Name>EndpointV1Tests.cs`.
- Class name = `<Name>EndpointV1Tests`; inherit `BaseApiTest` (HTTP) or
  `BaseRepositoryTest` (direct repo).
- Use **primary constructors**: `public XTests(PostgresFixture db) : BaseApiTest(db)`.
- Multiline XML doc on the class (per repo convention).

## URLs

- Never write `/api/...` or a literal route segment. Use `ApiRoutes.*` for bases
  and `Routes.*` / `*RouteConstants.*` for sub-resources and actions
  (see [`02-route-constants.md`](02-route-constants.md)).

## Test data

- Build request payloads with the typed **request builders**, not anonymous
  objects with hardcoded values.
- Seed entities with `*Factory` / `*Builder`; give unique columns a
  `Guid`-suffixed value.
- Randomness only through builders (global Bogus seed is fixed for reproducibility).

## Assertions (mandatory minimum)

Every test asserts status **and** at least the relevant subset of:

- Typed body via `response.ReadAsAsync<TResponse>()` (real src response record).
- Echoed request fields + returned id on create/update.
- Pagination metadata + presence of the seeded item on lists; filter tests
  assert every item matches the filter.
- `await response.ShouldBeProblem(status, code?)` on every error path.
- DB side-effect re-query for every mutation (create/update/delete/soft-delete).
- Content-Type for non-JSON responses (exports).

Status-only assertions are allowed **only** for pure authz gates (401/403) where
there is no body — and even then prefer asserting the ProblemDetails.

## Seeding

- Prefer `SeedAsync<TDbContext>(ctx => …)` over inline
  `CreateDbContext` + `Add` + `SaveChanges` boilerplate.

## Naming

- Method name states scenario + expected outcome:
  `Action_WithCondition_ReturnsXxx` (e.g.
  `CreateCategory_WithDuplicateSlug_ReturnsConflictProblem`).

## Anti-patterns to avoid

- `JsonDocument.Parse(...).GetProperty(...)` for body assertions — use typed
  deserialization.
- Count-based assertions that depend on shared DB state — assert specific seeded
  entities by unique key.
- `[Fact]` that only checks `StatusCode` for a use case that returns a body.
