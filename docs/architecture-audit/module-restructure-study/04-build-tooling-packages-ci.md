# 04 — Build, Tooling, Packages & CI

The high-ROI part of this study. Independent of whether you split anything, three tooling moves capture
most of the "organized, no drift, enforced" feeling — and two are prerequisites if you ever do split.

---

## Central Package Management — do this now (best ROI in the study)

**Current state (verified):** no `Directory.Packages.props`, no `Directory.Build.props`,
no `Directory.Build.targets` — anywhere. Package versions are pinned **inline per-csproj**: **66 pin-sites
across 51 packages**. Drift is already happening at only 13 projects — `xunit.runner.visualstudio` is
`3.0.2` in `tests/Unit` vs `3.1.5` in `tests/Integration`; `EFCore.NamingConventions 9.0.0` (×3),
`Bogus 35.6.3` (×4), and the EF Core `9.0.4` family are kept in lockstep by hand across Shared/Core/
Identity/Content. At ~30 projects this becomes unmanageable.

`Directory.Packages.props` (backend root):

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.4" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
    <PackageVersion Include="FluentValidation" Version="12.0.0" />
    <PackageVersion Include="Carter" Version="8.2.1" />
    <PackageVersion Include="Mapster" Version="7.4.2" />           <!-- pin off the prerelease -->
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.5" />  <!-- unify the drift -->
    <!-- …all 51 packages, one line each… -->
  </ItemGroup>
</Project>
```

Each `.csproj` then keeps only `<PackageReference Include="…" />` (no `Version`). `PrivateAssets`/
`IncludeAssets` blocks (EF Tools) stay on the reference. `NU1507` then fails the build if an inline
version reappears; treat that plus `NU1605`/`NU1608` as CI errors. This collapses 66 pin-sites to 51
single-source lines and fixes the existing drift — with no new assemblies.

## `Directory.Build.props` — hoist the boilerplate

The `TargetFramework net9.0`, `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors` blocks are copy-pasted
into all 10 src csproj today. Hoist once:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
</Project>
```

Each `.csproj` then carries only its `RootNamespace` and references. This is where a many-project world
pays for itself on the *maintenance* axis even if build times never improve — and it is worth doing at 13
projects too.

## `.slnf` solution filters — do this now (IDE load)

None exist today. At ~30 projects, opening the full `.sln` design-time-builds every project. One filter per
module lets a developer load only their module + its transitive deps + the host. Keep the full
`116_backend.sln` as the build-everything entry point; add e.g. `Content.slnf`, `Identity.slnf`,
`Core.slnf`, `Mailer.slnf`, `Shared.slnf` (5 files) referencing the master solution. Caveat: IDEs don't
auto-maintain a filter's project list, so each new project must be added to the relevant filter by hand — a
small ongoing tax, worth it above ~20 projects.

## The Dockerfile is a real ongoing cost multiplier

`Dockerfile` hand-lists every `.csproj` COPY for restore-layer caching (it already **omits Mailer** — a
latent build break, see [01 §1.15](../01-composition-root-and-shared-kernel.md)). Going to ~30 projects
means ~30 COPY lines to maintain, and every new project silently breaks layer caching until added. Replace
the hand-list with `COPY **/*.csproj` (glob) + the `Directory.*.props` during any restructure — and fix
the Mailer omission regardless.

## CI — affected-project detection is premature here

CI today does whole-repo `restore` + `build -c Release`, then `dotnet test tests/Unit` and
`dotnet test tests/Integration` as two parallel jobs; a coverage job already git-diffs changed `src/` files
per PR. Per-module selectivity is **not** worth building yet:

- The bottleneck is the **integration tests** (Testcontainers Postgres + Respawn), which dominate
  wall-clock and don't shrink by building fewer projects.
- Integration tests reference `Api`, which references every module, so any "affected" calculation collapses
  to "rebuild everything" the moment a shared or Api file changes — most PRs.
- `dotnet build Content.slnf` in CI is a reasonable cheap win *once filters exist* (build only the changed
  module's graph for the build job); full `dotnet-affected`/Nx-style tooling is over-engineering for ~30
  projects and one deployable.

Higher-leverage selectivity that needs no split: `dotnet test --filter` by module namespace for
changed-module-only runs, and per-module Codecov flags.

## Bottom line

**Adopt CPM + `Directory.Build.props` + `.slnf` now, at 13 projects.** They are low-risk, fix real drift,
improve IDE load, and are the prerequisites for any later split. They deliver most of the organizational
benefit the restructure was chasing — without the 13→30 churn.
