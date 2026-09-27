# 05 — Testing Strategy Under the Restructure

> **Overruled in part by [stage 18](../implementation-specs/stage-18-project-restructure.md) D8–D10.**
> Per-module integration test projects **are** being built — one per module — on the owner's decision.
> The 4× Testcontainers cost this doc prices is not inherent: `TestPostgresContainer` already leases
> each fixture a private database from one migrated template, and a `pg_advisory_lock` on the template
> migration lets a single container serve every assembly (D9). **Measured in 18.8:** `.WithReuse(true)`
> is not the mechanism — it attaches but then times out re-checking readiness on the running container —
> so the container is coordinated by a fixed name and port with cleanup disabled, and Redis stays
> per-assembly because the between-test flush would clobber a shared keyspace.
> The two shared libraries are **`TestData`** (builders, factories, constants — today's misnamed `tests/Fixtures`)
> and **`Fixtures`** (the xUnit fixtures and containers — today's `tests/Integration/Common`), per stage 18 D10.
> **Measured in 18.7:** the *collections* do not go in `Fixtures` with them. xUnit resolves a
> `[CollectionDefinition]` only inside the assembly that uses it, and the same holds for
> `[assembly: AssemblyFixture]`, so every suite declares its own collections and arms the container fixture
> itself; only the fixture classes are shared. Also measured: `TestConstants` is one partial class imported
> statically by every integration suite, so it stays whole in `TestData` instead of splitting per module.


**Verdict: split *unit* tests per module (clean, cheap); keep *integration* tests in one shared project
(the harness is intrinsically whole-app); a shared `Fixtures` is unavoidable.** Per-module integration test
projects would ~4× the Testcontainers cost for no CI win — the decisive finding.

---

## Current layout (already foldered by module)

Three test projects. Module-ownable files vs structurally-shared files:

| Bucket | Files | Ownable by a module? |
|---|---|---|
| `tests/Unit/Modules/{Content,Identity,Mailer,Core}` | 497 / 223 / 25 / 14 | **yes** |
| `tests/Integration/Modules/{…}` | 246 / 82 / 10 / 1 | in principle, but see harness below |
| `tests/Unit/Common` (mocks for all modules, `BaseHandlerTest`) | 55 | no — shared |
| `tests/Unit/Shared` + `BuildingBlocks` | 58 | no — shared |
| `tests/Integration/Common` (Base/Fixtures/Seeders/Stubs — the whole harness) | 26 | no — shared |
| `tests/Integration/Workflows` (**cross-module** flows) | 11 | **no — by definition** |
| `tests/Fixtures` (Builders/Factories/Constants/Routes) | 193 | no — cross-module shared data |

~1,098 module-ownable files vs ~342 structurally-shared.

## Unit tests split cleanly (do it)

They use mocks only, no container, no host, and are already grouped by module. Move
`Unit/Modules/<M>` → `src/modules/<M>/<M>/tests/<M>.Unit.Tests`, each referencing the shared `Fixtures`
(mocks + fixtures). Clean and low-risk. `Unit/Common` (shared mocks/handler bases) and `Unit/Shared`
stay shared.

## Integration tests **cannot** be split — the harness is whole-app

The integration suite is one tightly-coupled graph:

- **one static `PostgreSqlContainer`** for the whole assembly; `MigrateTemplateAsync` migrates **all four**
  DbContexts (Identity, Core, Content, Mailer) into one template DB, then `CREATE DATABASE … TEMPLATE`
  clones a cheap per-fixture copy — **one container start + one migration pass per assembly**, bound to the
  xUnit `[assembly: AssemblyFixture]`.
- **one `WebApplicationFactory<Program>`** booting the entire modular monolith; it rewires all four
  contexts and stubs Cloudinary/YouTube/Odesli/SMTP.
- `[CollectionDefinition("Database")]` + Respawn resets schemas `["identity","core","content","mailer"]`
  **together**; `BaseApiTest` seeds Identity users for **every** test regardless of module.
- **11 cross-module workflow tests** read two modules' DbContexts in one test (payment stamps a Content
  article *and* writes a Mailer outbox row; asset cleanup spans Content+Core+Identity). These have **no
  module owner**.

### The decisive cost — Testcontainers 4×

The container lifetime is **assembly-scoped**. `dotnet test` runs each test assembly in its own process; a
`static` container + assembly fixture do not cross the process boundary. Split integration tests into N
per-module assemblies and you get **N container starts + N full four-context migration passes** — because
each module's `ApiFixture` still boots the whole `Program` and still migrates all four schemas (the app
under test *is* the whole app). That turns the single most expensive part of the suite, paid **once** today,
into ~**4×** — the exact opposite of a CI win, for a harness deliberately engineered (tmpfs, template
cloning) to be shared.

### A hidden blocker: `TestConstants` is one partial class

`TestConstants` is a single partial class split across `Constants/{Content,Identity,Core,Shared}/…`, used as
`TestConstants.Commerce.*`, `TestConstants.Auth.*`, etc. **A partial class cannot span assemblies** — so
splitting Fixtures per module breaks the type until it's refactored into separate classes. `BaseApiTest`
has the same cross-module coupling (Content tests already depend on Identity's `UserFactory`).

## Recommended test layout

```
src/modules/<M>/<M>/tests/<M>.Unit.Tests        (per module — mocks only)
src/shared/tests/Integration.Tests          (ONE project: harness + all Modules/<M> integration + Workflows)
src/shared/tests/Fixtures                   (builders, factories, constants, mocks, the Api/Postgres boot)
src/shared/tests/Shared.Unit.Tests          (Shared kernel + BuildingBlocks unit tests)
```

> **What is actually being built** ([stage 18](../implementation-specs/stage-18-project-restructure.md) 18.7,
> drawn in full in [03](03-full-target-structure.md)): `src/modules/<M>/<M>/tests/{<M>.TestData,<M>.Unit.Tests,<M>.Integration.Tests}`,
> `src/shared/tests/{Shared.Unit.Tests,Shared.Integration.Tests}`, and top-level
> `tests/{TestData,Fixtures,EndToEnd.Tests,Architecture.Tests}` — `Fixtures` (the boot library) is
> separate from `TestData` so Testcontainers stays out of every unit-test graph, and the whole-app
> flows land in `EndToEnd.Tests`.

The harness **must** reference the `Api` host and all four module infra projects —
because `ApiFixture` boots the whole app. This project is inherently whole-app; it moves the coupling into
one honest place rather than pretending each module owns its integration tests.

## If the goal is faster/selective CI

Don't split — the wins are already available: `dotnet test --filter "FullyQualifiedName~Modules.Content"`
for changed-module-only unit runs, and per-module Codecov flags. The integration suite's cost is the
container + migration, which selectivity by project can't reduce without the 4× regression above.
