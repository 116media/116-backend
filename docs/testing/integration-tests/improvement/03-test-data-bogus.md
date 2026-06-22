# 03 — Test Data & Bogus Usage

Bogus is wired into the entity builders under `tests/Fixtures/Builders/Entities`,
and the recent uniqueness fix (`{prefix}{Guid:N}` instead of truncating to the
first 8 chars of a Lorem word) removed a real flakiness source. Two gaps remain.

## Gap 1 — No determinism (`Randomizer.Seed` is never set)

Every builder creates `private readonly Faker _faker = new();` with no global
seed. Test runs are **not reproducible**: a value that happens to violate a rule
(length, format) fails only on unlucky runs, and you can't reproduce it.

**Fix.** Set a fixed global seed once for the whole test run via a
`[ModuleInitializer]` in `tests/Fixtures` (or in the collection fixture):

```csharp
internal static class TestDataModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => Bogus.Randomizer.Seed = new Random(116116);
}
```

Document that a fixed seed is intentional and how to change it when reproducing a
failure. (Uniqueness still comes from `Guid.NewGuid()`, which is independent of
the Bogus seed, so seeding does not reintroduce duplicate-key collisions.)

## Gap 2 — Request payloads are ad-hoc anonymous objects

Endpoint tests build request bodies inline with hardcoded values:

```csharp
// ❌ hardcoded, no reuse, drifts from validators
var request = new { CategoryId = category.Id, Title = "Test Article Title", Slug = slug };
```

This duplicates "what a valid request looks like" across hundreds of tests and
makes it easy to accidentally send values that don't match current validation
rules.

**Fix.** Add typed **request builders** in `tests/Fixtures` that produce valid-
by-default payloads (reusing the domain constraints/constants), with overrides
for the field under test:

```csharp
var request = CreateArticleRequestBuilder.Valid(categoryId: category.Id);
var bad     = CreateArticleRequestBuilder.Valid() with { Title = "" };  // for a 400 test
```

Builders draw realistic values from Bogus (`Internet.Email`, `Lorem.Slug`,
`Commerce.ProductName`) while respecting max-length/format constraints. Tests
then assert the **echoed** request fields against the same builder instance (see
[`01-assertion-quality.md`](01-assertion-quality.md)).

## Builder hygiene checklist

- Realistic generators: emails via `Internet.Email().ToLowerInvariant()`, slugs
  via `Lorem.Slug()`, names within max length.
- Keep `Guid`-suffix uniqueness for unique columns (name/slug/resource).
- One `Faker` per builder instance (already the case) — do not reintroduce a
  shared mutable static faker.
- No `new Faker()` calls inside test methods; all randomness via builders.

Detailed specs: [`specs/test-data/01-bogus-determinism.md`](specs/test-data/01-bogus-determinism.md),
[`specs/test-data/02-builders-in-requests.md`](specs/test-data/02-builders-in-requests.md).
