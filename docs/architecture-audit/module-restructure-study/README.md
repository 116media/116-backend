# Module Restructure Study

A deep, evidence-backed evaluation of two proposed structural changes to the backend:

- **(A)** each module gets its own top-level folder with its own `src/` and `tests/`;
- **(B)** each architectural layer becomes its own `.csproj` (Clean-Architecture layer-as-project),

taking the solution from **13 projects to ~30**.

Four agents read the entire codebase (layer/slice structure, EF migrations, the test harness, build/
tooling). The full target tree is **generated from the filesystem** — every one of 3,800+ files mapped to
its new home, no ellipsis.

## Read in this order

| # | Doc | What it answers |
|---|-----|-----------------|
| 00 | [Overview](00-overview.md) | The question, the method, the TL;DR verdict table |
| 01 | [Decision A — per-module src/ + tests/](01-decision-a-per-module-src-tests.md) | Physical module autonomy — modest; unit tests yes, integration no |
| 02 | [Decision B — layers as projects](02-decision-b-layers-as-projects.md) | The Clean-Arch split — not worth it; use NetArchTest instead |
| 03 | [Full target structure](03-full-target-structure.md) | The complete no-ellipsis tree of the all-in layout |
| 04 | [Build, tooling, packages, CI](04-build-tooling-packages-ci.md) | CPM + Directory.Build.props + .slnf — the high-ROI wins |
| 05 | [Testing strategy](05-testing-strategy.md) | Per-module unit + one shared integration; the 4× Testcontainers trap |
| 06 | [EF migrations](06-ef-migrations.md) | Neutral→easier; four design-time factories |
| 07 | [Migration plan & final verdict](07-migration-plan-and-verdict.md) | What to do, what not to do, cost accounting |
| 08 | [Shared foundation: SharedKernel + BuildingBlocks](08-shared-foundation-structure.md) | The two-project split, file-by-file from reading all 90 shared files + a module sweep: `SharedKernel` vs `BuildingBlocks`, plus generics hoisted in and constants sent home |
| 09 | [SharedKernel vs BuildingBlocks: the rule + final layered structure](09-sharedkernel-vs-buildingblocks-rules.md) | The rule (domain vs technical), decision test, and the final structure — `SharedKernel` (1 domain project) + `BuildingBlocks` split into `.Domain/.Application/.Infrastructure/.Presentation` — adapted file-by-file to this codebase |
| 10 | [Entity / Behavior partial split](10-entity-behavior-partial-split.md) | Convention: each entity split into `Entities/<Name>.cs` (state) + `Behaviors/<Name>.cs` (behavior) as partial classes — the mirrored-folder layout and the same-namespace rule |

## One-line answer

**No huge advantage in the full 13→30 restructure.** Adopt Central Package Management,
`Directory.Build.props`, `.slnf` filters, and a few `NetArchTest` rules (which give the boundary
enforcement the layer split was for) — that captures the organized, enforced, drift-free result you want at
~13 projects. Split big-module *Application* by feature area only if incremental build time is measured
pain. Details and rollout in [07](07-migration-plan-and-verdict.md).
