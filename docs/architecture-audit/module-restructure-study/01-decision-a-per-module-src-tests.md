# 01 — Decision A: Per-Module `src/` and `tests/`

The proposal: give each module its own top-level folder containing its own `src/` and `tests/`, e.g.
`modules/Identity/src/...` and `modules/Identity/tests/...`, instead of the current single `src/Modules/…`
+ shared root `tests/`.

**Verdict: worth it only in its light form.** Re-rooting `src/` is cheap and neutral; splitting *unit*
tests per module is clean; splitting *integration* tests per module is a real regression. Net priority:
low.

---

## What already exists (so this is half-done)

Modules already live in their own folders — `src/Modules/{Identity,Content,Core,Mailer}/` — each its own
`.csproj`, and the cross-module boundaries that matter are **already enforced by dedicated `.Contracts`
projects** (`Content.csproj` references `Identity.Contracts`, `Mailer.Contracts`, `Shared.Contracts`,
never the implementation assemblies). The valuable part of "module autonomy" — a contract seam between
modules — is present. Moving `src/Modules/Identity/Identity/**` to `modules/Identity/src/**` **relocates
folders without changing the build graph**. It buys clearer ownership and enables per-module `.slnf`
filters ([04](04-build-tooling-packages-ci.md)); it does not buy isolation you don't already have.

## The `src/` re-rooting: cheap, cosmetic, fine

- Cost: a folder move + updating `.csproj`/`.sln` paths + the `Dockerfile` COPY block + the CLAUDE.md
  migration commands. Mechanical.
- Benefit: everything for a module (its layers, its tests, its README, its `.slnf`) sits under one path.
  Nice, not transformational.
- Neutral on build, deploy, and runtime — it is still one host, one deployable.

## The `tests/` split: unit yes, integration no

Today there are exactly three test projects, already foldered by module inside them:

| | Content | Identity | Mailer | Core | Shared/Common |
|---|---|---|---|---|---|
| Unit (`tests/Unit`) | 497 | 223 | 25 | 14 | 113 (Shared/BuildingBlocks/Common) |
| Integration (`tests/Integration`) | 246 | 82 | 10 | 1 | 46 (Common/Shared/Workflows) |
| Fixtures (shared) | — | — | — | — | 193 |

### Unit tests split cleanly (do it)

Unit tests are already grouped under `Unit/Modules/<M>`, use **mocks only** (no container, no host), and
have no cross-module dependencies beyond the shared mock/`Fixtures` helpers. Moving them to
`modules/<M>/tests/<M>.Unit.Tests` is a clean, low-risk relocation that genuinely gives each module its
own unit-test project. This is the one part of Decision A with real, uncomplicated value.

### Integration tests **cannot** split — the harness is whole-app

The integration suite is one tightly-coupled, all-modules graph (full detail in
[05](05-testing-strategy.md)):

- **one** static `PostgreSqlContainer` for the whole assembly; `MigrateTemplateAsync` migrates **all four**
  module DbContexts into one template DB, then clones per-fixture copies;
- **one** `WebApplicationFactory<Program>` booting the entire monolith; Respawn resets all four schemas
  together; `BaseApiTest` seeds Identity users for every test regardless of module;
- **11 cross-module workflow tests** (`tests/Integration/Workflows`) that read two modules' DbContexts in
  one test (e.g. payment stamps a Content article *and* writes a Mailer outbox row) — these have **no
  module owner** by definition.

Split integration tests into per-module assemblies and each assembly pays the full container-start +
all-four-schema migration cost that is paid **once** today — roughly **4× the most expensive part of the
suite** — while the cross-module tests still force a shared integration project. So "each module its own
`tests/`" is only ~half-true: it applies to unit tests, not integration.

## Recommendation

1. **If you re-root:** move each module to `modules/<M>/{src,tests}`, put **unit** tests under
   `modules/<M>/tests/<M>.Unit.Tests`, but keep **one shared** `Integration.Tests` project and a shared
   `Shared.TestKit` (builders/factories/harness) — see the tree in [03](03-full-target-structure.md) and
   the rationale in [05](05-testing-strategy.md).
2. **If you don't:** you lose very little. The contract seams — the part that actually enforces module
   autonomy — already exist.

Priority: **low**. Do it for tidiness alongside the higher-value CPM/`.slnf` work, or skip it. It is not a
source of the "huge advantage" the study set out to find.
