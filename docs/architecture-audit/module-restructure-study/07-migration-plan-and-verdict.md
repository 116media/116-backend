# 07 — Final Verdict & Migration Plan

## The verdict, plainly

**There is no *huge* advantage in the full restructure (per-module `src/`+`tests/` + every layer as its own
project), and going all the way to ~30 projects is mostly overhead.** All four investigation streams
converged on this:

- **Layers-as-projects** ([02](02-decision-b-layers-as-projects.md)): the one benefit (compile-time
  boundary enforcement) is ~90% deliverable *today* by a few `NetArchTest` rules; the split needs an
  invasive ASP.NET-decoupling refactor, won't compile until 24 Domain→Application violations are fixed,
  relocates ~586 files to a Presentation project, and fragments all 293 vertical slices. Content is the
  *worst* candidate, not the exception.
- **Per-module tests** ([05](05-testing-strategy.md)): unit tests split cleanly, but the integration harness
  is intrinsically whole-app — splitting it ~4×s the Testcontainers cost, and 11 cross-module tests have no
  owner.
- **Build/tooling** ([04](04-build-tooling-packages-ci.md)): 84% of files/edits are in Application (one
  assembly), so the layer split barely helps incremental builds; the real wins are packaging/tooling, not
  project count.
- **Migrations** ([06](06-ef-migrations.md)): neutral — the only area with no downside.

What you actually want — organized structure, enforced boundaries, no version drift, faster feedback — is
delivered by a much smaller set of moves.

## Do these (high ROI, low risk) — at 13 projects

1. **Central Package Management** (`Directory.Packages.props`) + **`Directory.Build.props`**. Fixes the live
   `xunit.runner.visualstudio` drift, collapses 66 pin-sites to 51, and is the single best action in the
   study. Independent of any split. → [04](04-build-tooling-packages-ci.md)
2. **`NetArchTest` architecture rules** in a small `tests/Architecture` project: `Domain !→ Application`,
   `Application !→ AspNetCore`, `Domain !→ EntityFrameworkCore`, module `!→` other module implementation
   assemblies. This is the boundary enforcement the layer split was for — at zero structural cost. →
   [02 §5](02-decision-b-layers-as-projects.md)
3. **Fix the 24 Domain→Application violations** those rules flag (move localized-error throwing out of the
   domain — the anti-pattern from [03 §6](../03-content-domain.md)). Real debt regardless of structure.
4. **`.slnf` solution filters** (one per module) for IDE load. → [04](04-build-tooling-packages-ci.md)
5. **Fix the Dockerfile** Mailer omission and switch to a `COPY **/*.csproj` glob.

## Consider these (targeted, only on measured pain)

6. **Per-module *unit* test projects** — clean, cheap, gives each module its own unit-test home. Keep **one
   shared** `Integration.Tests` + `Shared.TestKit`. → [01](01-decision-a-per-module-src-tests.md),
   [05](05-testing-strategy.md)
7. **Split Content/Identity *Application* by feature area** (`Commerce/Interactions/Editorial/Catalog/Lookup`)
   — *only if* incremental build time on those two modules is a recurring, measured pain. This matches the
   vertical slices and lands ~18–22 projects, not ~30. → [02](02-decision-b-layers-as-projects.md)

## Don't do these

- ❌ Split every module into Domain/Application/Infrastructure/Presentation projects.
- ❌ A separate Presentation project (shreds the 293 vertical slices).
- ❌ Layer-split the small modules (Core, Mailer, BuildingBlocks, Contracts) — pure overhead.
- ❌ Per-module integration test projects (~4× Testcontainers cost).

## The recommended pragmatic structure

If you re-root into per-module folders (optional, cosmetic — [01](01-decision-a-per-module-src-tests.md)),
the *recommended* shape is **3 projects per big module, 1 per small module** — not the all-in tree in
[03](03-full-target-structure.md):

```
modules/
  Content/
    src/  Content.Domain/  Content.Application/  Content.Infrastructure/   (endpoints stay in Application)
    tests/  Content.Unit.Tests/
  Identity/
    src/  Identity.Domain/  Identity.Application/  Identity.Infrastructure/  Identity.Contracts/
    tests/  Identity.Unit.Tests/
  Core/     src/ Core/ (one project) + Core.Contracts/     tests/ Core.Unit.Tests/
  Mailer/   src/ Mailer/ (one project) + Mailer.Contracts/ tests/ Mailer.Unit.Tests/
shared/
  src/   Shared.Kernel/  Shared.Application/  Shared.Infrastructure/  Shared.Contracts/  BuildingBlocks/
  tests/ Integration.Tests/  Shared.TestKit/  Shared.Unit.Tests/  Architecture.Tests/
host/
  Api/
```

Key differences from the all-in tree in [03](03-full-target-structure.md): **endpoints stay in
`<M>.Application`** (no `<M>.Api`), **Core/Mailer are one project each** (not layer-split), and the
`Architecture.Tests` project carries the `NetArchTest` rules that replace the compile-time layer boundary.
The full all-in tree in [03] is kept only so you can see the maximal version — this pragmatic shape is the
recommendation. (See [11](../11-project-structure-and-packages.md) for how the Shared split and CPM were
already adopted.)

## Cost accounting (why the full split isn't worth it)

**One-time cost of the all-in split:** move ~2,000 files (Content+Identity) into per-layer trees; create
~18–22 new `.csproj`; rework 24 Domain→Application files *just to compile*; refactor ~34 `IFormFile`-carrying
commands + the authorization layer to make Application framework-clean; relocate ~586 endpoint/record/
MetaField files to Presentation and repoint Carter's assembly scanning; rebuild the 187-line hand-written
`.sln` (+~350 lines of GUIDs) and author `.slnf`s; rewrite the Dockerfile and CI paths; re-assert
`InternalsVisibleTo` per new assembly; possibly promote `internal` helpers to `public` to cross the new
seams (eroding encapsulation — the opposite of the goal).

**Ongoing cost:** ~30 vs 13 `.csproj`; every new module = 3–4 projects + sln + slnf + Dockerfile edits;
more DI wiring across seams; every vertical-slice change touches two project trees.

**Ongoing benefit:** compile-time layer boundary (≈ deliverable by a test), and faster incremental builds
**only** on the ~9% of edits that touch Infrastructure — or, better, via the feature-area split (#7), which
you can do without the layer split.

The benefit does not clear the cost. Take the five high-ROI moves; they give you the organized, enforced,
drift-free codebase you're after without the 13→30 reorganization.
