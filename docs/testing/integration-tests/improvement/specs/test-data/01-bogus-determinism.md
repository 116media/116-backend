# Test-Data Spec 01 — Bogus determinism

## Problem
No global `Randomizer.Seed`. Bogus produces different values each run, so a value
that violates a constraint fails intermittently and can't be reproduced.

## Before
```csharp
internal class UserBuilder
{
    private readonly Faker _faker = new();   // unseeded, per builder
    // ...
}
```

## After
Set a fixed seed once for the whole test assembly via a module initializer in
`tests/Fixtures`:

```csharp
// tests/Fixtures/TestDataModuleInitializer.cs
using System.Runtime.CompilerServices;
using Bogus;

namespace _116.Tests.Fixtures;

internal static class TestDataModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => Randomizer.Seed = new Random(116116);
}
```

Uniqueness still comes from `Guid.NewGuid()` (independent of the Bogus seed), so a
fixed seed does NOT reintroduce duplicate-key collisions.

## TODO checklist
- [ ] Add `TestDataModuleInitializer` to `tests/Fixtures`.
- [ ] Document the seed value + "how to reproduce a failure" in `03-test-data-bogus.md`.
- [ ] Confirm both `tests/Unit` and `tests/Integration` pick it up (Fixtures is referenced by both).

## Acceptance
- Two consecutive runs produce identical generated values for the same test order.
- No new duplicate-key failures.
