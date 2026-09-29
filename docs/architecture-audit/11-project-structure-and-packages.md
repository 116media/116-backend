# 11 — Target Project Structure & Package Management

> **Superseded in part by [stage 18](implementation-specs/stage-18-project-restructure.md).** The
> argument holds; six specifics do not.
>
> 1. **Naming.** The domain leaf is `Shared.Domain` (this doc's name, kept), but
>    `Shared.Application` / `Shared.Infrastructure` become `BuildingBlocks.{Application,Infrastructure}`
>    per [study 09](module-restructure-study/09-sharedkernel-vs-buildingblocks-rules.md).
>    `Shared.Contracts` does **not** survive as a leaf — its nine CQRS interfaces fold into
>    `BuildingBlocks.Application` — and `Specification<T>` lands in `BuildingBlocks.Domain`, not
>    `Shared.Domain`, because `System.Linq.Expressions` would break the zero-package rule (D3).
> 2. **Scope.** This doc argues layer projects for *every* module, the study prices that as overhead on
>    the two small ones, and the owner ruled for this doc: stage 18 D1 layers **all four** modules, each
>    plus its Contracts.
> 3. **Layout.** `src/modules/<M>/` holds `<M>/` (which carries its own `src/` and `tests/`) and
>    `<M>.Contracts/` as peers — today's `src/Modules/<M>/{<M>, <M>.Contracts}` with `src/Modules/`
>    dropped.
> 4. **Project count.** "~20 projects" below underestimated splitting every module. The shape stage 18
>    actually builds lands at **39**, from **15** today, because each module also carries its own test
>    projects. The CPM argument is unaffected — if anything it is stronger.
> 5. **`Core` is `Storage`** (D7), and **`Core.Contracts` already exists**: it shipped in Stage 14 as
>    `Storage.Contracts` (`IFileStorageService` / `FileReferenceDto`), so the "one module still
>    missing it" note below is closed.
> 6. **Rollout order.** The order below (CPM → Shared → Core pilot → Identity → Content) is
>    superseded by stage 18's checklist: CPM and the architecture tests already shipped in Stage 14,
>    the constants carve-outs run first (18.3), and **Storage is never layer-split**, so it cannot be
>    the pilot.
>
> The leak counts below ("74 places", `UserEntity` taking `UserErrors`) were fixed by Stages 7 and 15
> and now measure **0** — the live case for the split is the transitive package graph, not the imports.


**Status: adopted decision.** Each module is split into one project per layer
(`*.Domain` / `*.Application` / `*.Infrastructure`, plus `*.Contracts` where the module is
consumed by others). The dependency rule is then enforced by the compiler, not by convention.
Package versions are managed centrally so the projects cannot drift (**39** in the shape stage 18 builds, every module layered).

This is a compile-time reorganisation only. **It still builds and deploys as a single monolith**
— every project compiles into the one `Api` host process, exactly as today ([why](#still-one-deployable)).

---

## Why layer-projects, and why for every module

Folders do not enforce the dependency rule — the compiler does. Today Content's domain imports
its application layer in 74 places and query builders import `ContentDbContext`
([03 §6](03-content-domain.md), [06 §14](06-content-application.md)); Identity's `UserEntity`
takes an application-layer `UserErrors` and `VisitorPermissions` imports `Application.Shared`
([07 A4](07-identity-and-security.md)). These are exactly the leaks a project boundary makes
impossible: a `*.Domain` project that does not reference `*.Application` **cannot compile** a
domain type that uses an application type.

Applying it uniformly (not only to Content) keeps the solution consistent — one shape for every
module, no "which modules are layered?" ambiguity — and makes the module a genuinely
reusable/extractable unit. The payoff is largest in Content and Identity (where the leaks are
real today) and smallest in Storage/Mailer (37 and 82 files) — which is exactly why stage 18 D1 stops
at Content and Identity — but uniformity is the argument here:
`AddModule`/`UseModule` and the folder layout stay identical everywhere.

---

## Target solution graph

Arrows point in the direction of the reference. Inner layers know nothing about outer layers.

```
Shared.Domain                  (leaf — Aggregate<T>, Entity<T>, IDomainEvent, DomainRuleException; ZERO packages)
BuildingBlocks.Domain          (Specification<T>, IRepository<T>; System.Linq.Expressions only)
BuildingBlocks.Application     (CQRS interfaces, decorators, IUnitOfWork, pagination, exceptions, metadata)
BuildingBlocks.Infrastructure  (EF, Npgsql, Quartz, Redis — BaseModule, interceptors, outbox, cache, seeding)
BuildingBlocks.Presentation    (ASP.NET, Carter, Swashbuckle, versioning — middleware, policy names, ApiVersionUrl)

For each module X (Identity, Content, Storage, Mailer) — stage 18 layers all four:

  X.Contracts      ─► BuildingBlocks.Application                  (leaf; only if other modules consume X)
  X.Domain         ─► Shared.Domain, BuildingBlocks.Domain        (aim for ZERO NuGet packages)
  X.Application    ─► X.Domain, BuildingBlocks.Application, <other>.Contracts
  X.Infrastructure ─► X.Application, BuildingBlocks.{Infrastructure,Presentation}   (owns the DbContext + migrations)

Api ─► every X.Infrastructure        (the composition root; the only project that sees all modules)
```

Notes specific to this codebase:
- **`Shared` splits too** — it currently drags Carter/EF/Quartz/Bogus into every domain layer
  ([01 §1.9](01-composition-root-and-shared-kernel.md)). The split above is the fix: `Shared.Domain`
  has zero package references, so a module's domain can reference it without inheriting the web
  stack.
- **`Storage.Contracts` exists** — it shipped in Stage 14 ([02 §1](02-module-boundaries.md)), so all
  four modules now have their Contracts project.
- **Content stays one module** — this is a *layer* split, not the feature split that
  [10 — Should the Content module be split?](10-content-module-sizing.md) rejects. `Content.Domain`
  still holds every Content aggregate in one `content` schema — **37 aggregate roots and 12 member
  entities** after Stage 15's demotion.
- The namespaces already match the folders (`_116.Content.Domain.*`, `_116.Content.Application.*`,
  `_116.Content.Infrastructure.*`), so moving files into projects is largely mechanical — `using`
  directives don't change.

### Still one deployable

Projects are compile-time boundaries. `Api.csproj` references every `X.Infrastructure`, they all
build into one process, and `docker-compose`/the Dockerfile deploy the single `Api` container
exactly as now. Nothing about runtime, hosting, or the database changes — only *what can
reference what at compile time*.

---

## Package version management (avoiding version hell across 39 projects)

Splitting into 20-plus projects makes scattered inline versions untenable. Adopt **Central Package
Management (CPM)**.

### 1. One `Directory.Packages.props` at the backend root — every version declared once

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
    <PackageVersion Include="FluentValidation" Version="12.0.0" />
    <PackageVersion Include="Mapster" Version="7.4.2" />
    <!-- one line per package, for the whole solution -->
  </ItemGroup>
</Project>
```

Each `.csproj` then references **without a version**:

```xml
<PackageReference Include="FluentValidation" />
```

A version can drift in exactly one place. This also removes the scattered pins the audit flagged
(`Mapster 7.4.2-pre02`, `Bogus` in `Shared.csproj` — [01 §1.9](01-composition-root-and-shared-kernel.md));
`Bogus` leaves the shared graph in stage 18.2. `Mapster` does **not** move: measured, every stable
release above `7.4.2-pre02` is Mapster 10.x, a major upgrade that does not belong in a restructure, so
it is a follow-up of its own.

### 2. One `Directory.Build.props` for shared MSBuild properties

So `net9.0`, `Nullable`, `LangVersion`, `TreatWarningsAsErrors`, CSharpier, etc. are declared once
and inherited, not copy-pasted into every `.csproj`.

### 3. The layer split *shrinks* the version surface — it does not grow it

With the dependency rule enforced, most projects reference almost nothing:

- `*.Domain` → ideally **zero** NuGet packages (the point of a pure domain).
- `*.Application` → a handful (FluentValidation, Mapster).
- `*.Infrastructure` → the heavy set (EF, Npgsql, Cloudinary, Quartz), concentrated in the ~4–5
  infrastructure projects.

So 20-plus projects do not mean 20× the packages — the packages concentrate in the infrastructure
projects and the domain projects are dependency-free, which means **fewer** conflict points, not
more.

### 4. One CI guard

CPM emits `NU1507` if an inline version reappears; treat that plus `NU1605` (downgrade) and
`NU1608` (conflict) as build errors in CI, and version drift cannot come back.

---

## EF migrations after the split

The `DbContext` moves into `X.Infrastructure`, so migrations target that project with `Api` as the
startup project:

```bash
dotnet ef migrations add <Name> \
  --project src/modules/Content/Content/src/Content.Infrastructure \
  --startup-project src/host/Api \
  --context ContentDbContext
```

Optionally add an `IDesignTimeDbContextFactory<ContentDbContext>` per `*.Infrastructure` project so
design-time tooling can construct the context without the full host. **None exist today and none are
required** — `--startup-project src/host/Api` boots `Program.cs`, which registers all four contexts
([study 06 §The one real gotcha](module-restructure-study/06-ef-migrations.md)); the factories only buy
standalone per-module migration. (This also lets migrations run
as the separate deploy step recommended in [04 §10](04-content-infrastructure.md).)

---

## Rollout order

Do it one seam at a time; each step ships green on its own.

1. **CPM + `Directory.Build.props` first** — pure mechanical move of versions/properties into the
   two root files, no code change. Establishes the guard before the churn.
2. **Split `Shared`** into `Shared.Domain` + the four `BuildingBlocks.*`
   ([01 §1.9](01-composition-root-and-shared-kernel.md)). Everything depends on it, so tightening it
   first surfaces the real layer violations everywhere else as compile errors.
3. ~~**Split one module end-to-end as the pilot — Core**~~ — superseded: Storage is not layer-split
   at all (D1), and `Storage.Contracts` shipped in Stage 14. Stage 18 pilots the mechanics on the
   `Core`→`Storage` rename instead (18.6), which is a folder relocation plus one `ALTER SCHEMA`.
4. **Then Identity, then Content** — the two that *are* layered, and the two where the namespaces
   already encode the layer, so neither move changes a single `using` (stage 18 D14).
5. **Add the architecture test** ([02 §3](02-module-boundaries.md)) alongside — it now also covers
   any intra-solution rule the project graph can't express (e.g. "no `*.Application` references
   `Microsoft.AspNetCore.*`").

Projects give the compile-time guarantee; the architecture test covers the rules a project
reference can't. Together they make both the module boundaries and the layer boundaries
un-regressable.
