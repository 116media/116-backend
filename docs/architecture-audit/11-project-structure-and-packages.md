# 11 — Target Project Structure & Package Management

**Status: adopted decision.** Each module is split into one project per layer
(`*.Domain` / `*.Application` / `*.Infrastructure`, plus `*.Contracts` where the module is
consumed by others). The dependency rule is then enforced by the compiler, not by convention.
Package versions are managed centrally so ~20 projects cannot drift.

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
real today) and smallest in Core/Mailer (37 and 82 files), but uniformity is the point:
`AddModule`/`UseModule` and the folder layout stay identical everywhere.

---

## Target solution graph

Arrows point in the direction of the reference. Inner layers know nothing about outer layers.

```
Shared.Contracts       (leaf — CQRS interfaces, IDispatcher)
Shared.Domain          (leaf — Aggregate<T>, Entity<T>, IDomainEvent, Specification<T>; ZERO packages)
Shared.Application     (FluentValidation, Mapster — decorators, IUnitOfWork, pagination, exceptions)
Shared.Infrastructure  (Carter, EF, Npgsql, Quartz, Swashbuckle — BaseModule, interceptors, host wiring)
BuildingBlocks         (inert cross-cutting constants — policy names)

For each module X (Identity, Content, Core, Mailer):

  X.Contracts      ─► Shared.Contracts                              (leaf; only if other modules consume X)
  X.Domain         ─► Shared.Domain                                 (aim for ZERO NuGet packages)
  X.Application    ─► X.Domain, Shared.Application, <other>.Contracts
  X.Infrastructure ─► X.Application, Shared.Infrastructure          (owns the DbContext + migrations)

Api ─► every X.Infrastructure        (the composition root; the only project that sees all modules)
```

Notes specific to this codebase:
- **`Shared` splits too** — it currently drags Carter/EF/Quartz/Bogus into every domain layer
  ([01 §1.9](01-composition-root-and-shared-kernel.md)). The split above is the fix: `Shared.Domain`
  has zero package references, so a module's domain can reference it without inheriting the web
  stack.
- **`Core.Contracts` gets created** as part of this ([02 §1](02-module-boundaries.md)) — Core is
  the one module still missing it.
- **Content stays one module** — this is a *layer* split, not the feature split that
  [10 — Should the Content module be split?](10-content-module-sizing.md) rejects. `Content.Domain`
  still holds all 49 (soon ~15–20) aggregates in one `content` schema.
- The namespaces already match the folders (`_116.Content.Domain.*`, `_116.Content.Application.*`,
  `_116.Content.Infrastructure.*`), so moving files into projects is largely mechanical — `using`
  directives don't change.

### Still one deployable

Projects are compile-time boundaries. `Api.csproj` references every `X.Infrastructure`, they all
build into one process, and `docker-compose`/the Dockerfile deploy the single `Api` container
exactly as now. Nothing about runtime, hosting, or the database changes — only *what can
reference what at compile time*.

---

## Package version management (avoiding version hell across ~20 projects)

Splitting into ~20 projects makes scattered inline versions untenable. Adopt **Central Package
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
pin `Mapster` to a stable release here and move `Bogus` to the test project.

### 2. One `Directory.Build.props` for shared MSBuild properties

So `net9.0`, `Nullable`, `LangVersion`, `TreatWarningsAsErrors`, CSharpier, etc. are declared once
and inherited, not copy-pasted into ~20 `.csproj` files.

### 3. The layer split *shrinks* the version surface — it does not grow it

With the dependency rule enforced, most projects reference almost nothing:

- `*.Domain` → ideally **zero** NuGet packages (the point of a pure domain).
- `*.Application` → a handful (FluentValidation, Mapster).
- `*.Infrastructure` → the heavy set (EF, Npgsql, Cloudinary, Quartz), concentrated in the ~4–5
  infrastructure projects.

So ~20 projects do not mean 20× the packages — the packages concentrate in the infrastructure
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
  --project src/Modules/Content/Content.Infrastructure \
  --startup-project src/Api \
  --context ContentDbContext
```

Add an `IDesignTimeDbContextFactory<ContentDbContext>` in each `*.Infrastructure` project so
design-time tooling can construct the context without the full host. (This also lets migrations run
as the separate deploy step recommended in [04 §10](04-content-infrastructure.md).)

---

## Rollout order

Do it one seam at a time; each step ships green on its own.

1. **CPM + `Directory.Build.props` first** — pure mechanical move of versions/properties into the
   two root files, no code change. Establishes the guard before the churn.
2. **Split `Shared`** into `Shared.Domain` / `Shared.Application` / `Shared.Infrastructure`
   ([01 §1.9](01-composition-root-and-shared-kernel.md)). Everything depends on it, so tightening it
   first surfaces the real layer violations everywhere else as compile errors.
3. **Split one module end-to-end as the pilot — Core** (37 files, smallest, and it needs
   `Core.Contracts` created anyway). Prove the DbContext/migration/design-time-factory mechanics on
   the cheapest module.
4. **Then Identity, then Content** — the two with real leaks, so the compiler does the most work
   catching them. Content last, as the largest and the one that benefits most.
5. **Add the architecture test** ([02 §3](02-module-boundaries.md)) alongside — it now also covers
   any intra-solution rule the project graph can't express (e.g. "no `*.Application` references
   `Microsoft.AspNetCore.*`").

Projects give the compile-time guarantee; the architecture test covers the rules a project
reference can't. Together they make both the module boundaries and the layer boundaries
un-regressable.
