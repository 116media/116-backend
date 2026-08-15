# Module Restructure Study — Overview

## The question

Is there a **huge advantage** in restructuring so that:

- **(A)** each module gets its own top-level folder with its own `src/` and `tests/` (physical module
  autonomy), and
- **(B)** each architectural layer within a module (Domain / Application / Infrastructure /
  Presentation) becomes its own `.csproj` (Clean-Architecture layer-as-project),

taking the solution from **13 projects today to ~30 projects**?

## How this was investigated

Four parallel agents read the real codebase: the layer/slice structure and Clean-Architecture
correctness; EF Core migrations and DbContext ownership; the test suite and its Testcontainers
harness; and build/tooling/CI/packaging. Every claim below is backed by file evidence in the
per-topic docs. The complete target tree in [03](03-full-target-structure.md) is **generated from the
filesystem** — every one of the 3,800+ files mapped to its new home, no ellipsis.

## TL;DR verdict

**No — not a huge advantage as literally scoped, and going all the way to ~30 projects is mostly
overhead.** But three cheap, high-ROI moves capture most of the *feeling* you're after (organized,
enforced boundaries, no version drift) without the churn, and two narrow splits give a real but
bounded win.

| Move | Verdict | Why |
|---|---|---|
| **Central Package Management + `Directory.Build.props`** | **Do it now** — best ROI in the study | 66 inline version pins across 51 packages, already drifting (`xunit.runner.visualstudio` 3.0.2 vs 3.1.5). Independent of any split. ~13 projects unchanged. |
| **`.slnf` solution filters (one per module)** | **Do it now** | Faster IDE load; prerequisite plumbing if you later split. Works against the current solution. |
| **Per-module `src/` folder** | Low value, low cost | Modules are already isolated as projects; this just relocates folders. Cosmetic autonomy. |
| **Per-module `tests/` — unit tests** | Worth it | Unit tests are already foldered per module, use mocks only, split cleanly. |
| **Per-module `tests/` — integration tests** | **Don't** | The harness is intrinsically whole-app (one container, one `WebApplicationFactory<Program>`, migrates all four schemas); 11 cross-module workflow tests have no owner; splitting ⇒ ~4× Testcontainers cost. Keep one shared integration project. |
| **Layer-as-project (Domain/App/Infra/Presentation) for every module** | **Don't (as scoped)** | 84% of files and most edits are in Application, which stays one assembly ⇒ little incremental-build win; a separate Presentation project shreds the vertical slices; small modules are pure overhead. |
| **Split big modules' Application by *feature area*** | Consider, Content/Identity only | This — not layer-splitting — is the split that actually speeds incremental builds, and it matches the existing slices. Lands ~18–22 projects, not ~30. |
| **Compiler-enforced layer boundary** | Use an architecture test instead | A `NetArchTest` rule gives the same "Domain can't reference Infrastructure" guarantee at ~0 project overhead. |

## The two decisions, separated

The request bundles two independent decisions. They have different answers:

- **[01 — Decision A: per-module `src/` + `tests/`](01-decision-a-per-module-src-tests.md)** — physical
  autonomy. Modest: unit tests split cleanly, integration cannot, `src/` relocation is cosmetic.
- **[02 — Decision B: layers as projects](02-decision-b-layers-as-projects.md)** — the Clean-Arch
  split. Mostly overhead as scoped; the real win is a feature-area split of the two big modules only.

## Supporting analysis

- **[03 — Full target structure](03-full-target-structure.md)** — the complete, no-ellipsis tree of
  the all-in layout (so you can see it), with the recommended-pragmatic deltas called out.
- **[04 — Build, tooling, packages, CI](04-build-tooling-packages-ci.md)** — CPM, `Directory.Build.props`,
  `.slnf`, the Dockerfile cost, why affected-project CI is premature here.
- **[05 — Testing strategy](05-testing-strategy.md)** — per-module unit + one shared integration +
  shared TestKit; the Testcontainers 4× regression quantified.
- **[06 — EF Core migrations](06-ef-migrations.md)** — neutral, trending easier; four design-time
  factories needed for true module independence.
- **[07 — Migration plan & final verdict](07-migration-plan-and-verdict.md)** — the recommended
  pragmatic target, sequenced, with cost accounting.

## Relationship to the rest of the audit

This study is about **physical project/folder structure**. It composes with — but is distinct from —
the *logical* structure decisions already recorded:
[11 (layer projects + CPM, adopted)](../11-project-structure-and-packages.md),
[12 (Shared kernel vs BuildingBlocks)](../12-shared-kernel-and-buildingblocks.md),
[13 (Core→Storage, Settings module)](../13-core-storage-and-settings-module.md). Where they overlap,
this study supplies the file-level evidence and the honest cost/benefit that says *how far* to take
the split.
