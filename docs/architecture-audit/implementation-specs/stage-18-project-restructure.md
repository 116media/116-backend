# Stage 18 — Project restructure (SharedKernel, BuildingBlocks, layer projects)

Closes **[01 §1.9]**, **[02 §7]**, **[02 §10]**, **[03 §1]**, and executes the
[module-restructure-study](../module-restructure-study/README.md)'s **pragmatic verdict** —
not the all-in 36-project tree, which the study itself rejects on cost accounting
(`07-migration-plan-and-verdict.md`: "The benefit does not clear the cost").

**Deliberately second-to-last.** Every earlier stage would conflict with these file moves;
Stage 14's CPM + `Architecture.Tests` must exist first so the project growth is
version-coherent and boundary-enforced from the first commit.

> Draft — finalized against the tree Stage 17 lands on.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Which shape | all-in (36 projects, per-module `.Api`), or the study's pragmatic shape | **Pragmatic, exactly as the study's verdict:** layer projects for the two big modules only; **endpoints stay in `<M>.Application`** (a separate presentation project shreds the vertical slices); Core and Mailer stay one project each + their Contracts; **one** shared integration test project (the harness is whole-app; per-module integration projects would 4× the Testcontainers cost). |
| D2 | `Shared` split | keep one Shared, or Kernel + layered shared projects | **Split per study 08/09:** `Shared.Kernel` (pure domain: `Aggregate<T>`, `Entity<T>`, `IDomainEvent` — references nothing), `Shared.Application`, `Shared.Infrastructure`, `Shared.Contracts`, `BuildingBlocks`. This is what finally takes the web stack and Bogus out of every domain layer's transitive graph `[01 §1.9]`; single-module constants leave `BuildingBlocks` for their module `[02 §10]`. |
| D3 | Visibility | leave `public`, or `internal` default | **`internal` by default** per new assembly, `InternalsVisibleTo` only for the module's own unit-test assembly `[02 §7]`. Where a seam genuinely needs crossing, that's a Contracts type, not a `public` escape hatch. The Stage 14 boundary allowlist ratchets to zero here. |
| D4 | Entity files | leave as-is, or the study-10 partial split | **Partial split** for the large entities only (the study's threshold: >300 lines): `Entities/ArticleEntity.cs` (state) + `Behaviors/ArticleEntity.Editorial.cs` etc. Junction/child entities (`ArticleTagEntity`, `ArticleImageEntity`, `RolePermissionEntity`, …) demote from `Aggregate<Guid>` to `Entity<Guid>` — they raise no events and are never loaded alone `[03 §1]`. |
| D5 | Migrations | regenerate, or relocate | **Relocate only**, per study 06: the `Migrations/` folders move with their Infrastructure project, `MigrationsAssembly` is pointed explicitly, and `dotnet ef migrations list` must show the identical list per context before and after. A regenerated migration on a live schema is the failure mode this rule exists to prevent. |
| D6 | Mechanics | big-bang, or per-module series | **Per-module commit series, solution green after every series.** Pre-compute the full old-path → new-path map before touching anything (the Stage 7 lesson). `git mv` so history follows. |

---

## Checklist

- [ ] 18.1 — Move map committed (every file, old → new, per series)
- [ ] 18.2 — `shared/`: Kernel/Application/Infrastructure/Contracts/BuildingBlocks split; domain graphs lose web+Bogus
- [ ] 18.3 — Identity → `Identity.Domain/Application/Infrastructure` (+ existing Contracts)
- [ ] 18.4 — Content → `Content.Domain/Application/Infrastructure`
- [ ] 18.5 — Core, Mailer re-rooted unsplit (+ Contracts); `Core` folder renamed `Storage` if Stage 14's rename deferred it
- [ ] 18.6 — Entity/behavior partials for the >300-line entities; junction entities → `Entity<Guid>`
- [ ] 18.7 — `internal` sweep + `InternalsVisibleTo`; `Architecture.Tests` allowlist → 0
- [ ] 18.8 — sln/slnf, Dockerfile, CI paths, coverage configs updated
- [ ] 18.9 — Verify (build 0/0, csharpier, unit, integration, architecture; migrations lists identical)

---

## The target tree (the study's verdict, verbatim)

```text
modules/
  Content/
    src/   Content.Domain/  Content.Application/  Content.Infrastructure/   (endpoints stay in Application)
    tests/ Content.Unit.Tests/
  Identity/
    src/   Identity.Domain/  Identity.Application/  Identity.Infrastructure/  Identity.Contracts/
    tests/ Identity.Unit.Tests/
  Core/    src/ Core/ (one project) + Core.Contracts/      tests/ Core.Unit.Tests/
  Mailer/  src/ Mailer/ (one project) + Mailer.Contracts/  tests/ Mailer.Unit.Tests/
shared/
  src/   Shared.Kernel/  Shared.Application/  Shared.Infrastructure/  Shared.Contracts/  BuildingBlocks/
  tests/ Integration.Tests/  Shared.TestKit/  Shared.Unit.Tests/  Architecture.Tests/
host/
  Api/
```

Deterministic mapping rules (from study 03, adjusted to the pragmatic shape):

- `src/Modules/<M>/<M>/Domain/**` → `modules/<M>/src/<M>.Domain/**`
- `src/Modules/<M>/<M>/Application/**` → `modules/<M>/src/<M>.Application/**` (endpoints included)
- `src/Modules/<M>/<M>/Infrastructure/**` → `modules/<M>/src/<M>.Infrastructure/**` (Migrations ride along)
- `src/Shared/Shared/Domain/**` → `shared/src/Shared.Kernel/**`
- `tests/Integration/**` → `shared/tests/Integration.Tests/**` (one project, whole-app harness)
- `tests/Fixtures/**` + `tests/Unit/Common/**` → `shared/tests/Shared.TestKit/**`

## Project shape

Representative — `Content.Domain.csproj`, the point of the whole exercise (nothing outward):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="../../../../shared/src/Shared.Kernel/Shared.Kernel.csproj" />
  </ItemGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="Content.Unit.Tests" />
  </ItemGroup>
</Project>
```

No package references (CPM would supply versions if any existed — none should), no
`Microsoft.AspNetCore.*`, no Bogus. The architecture test that proves it:

```csharp
[Fact]
public void DomainProjectsReferenceOnlyTheKernel()
{
    foreach (Assembly domain in new[] { ContentDomain, IdentityDomain })
    {
        TestResult result = Types
            .InAssembly(domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore",
                "Bogus",
                "_116.Shared.Application",
                "_116.Shared.Infrastructure"
            )
            .GetResult();

        result.FailingTypes.Should().BeEmpty();
    }
}
```

## Entity partials and demotions

`ArticleEntity` (state and identity stay in one file; the behaviour clusters split per
study 10):

```text
Content.Domain/
  Entities/ArticleEntity.cs              // properties, ctor, Create factories
  Behaviors/ArticleEntity.Editorial.cs   // Submit/Approve/Publish/Reject/Archive
  Behaviors/ArticleEntity.Promotion.cs   // StampSocialBoost/ForceUnpromote/…
```

```csharp
// Entities/ArticleEntity.cs
public partial class ArticleEntity : Aggregate<Guid> { /* state */ }

// Behaviors/ArticleEntity.Editorial.cs
public partial class ArticleEntity { /* transitions */ }
```

Junction demotion `[03 §1]` — `ArticleTagEntity : Aggregate<Guid>` becomes
`ArticleTagEntity : Entity<Guid>`; no event plumbing, no `DomainEvents` allocation per row
materialized. Repeat for every join/child row type the census in 18.1 lists.

## Migration relocation (study 06)

Per context, in the same commit as its Infrastructure move:

```csharp
options.UseNpgsql(connectionString, npgsql =>
{
    npgsql.MigrationsAssembly("Content.Infrastructure");
    // …Stage 10's retry/timeout settings unchanged…
});
```

Gate for each of the four contexts, before and after:

```bash
dotnet ef migrations list --project modules/Content/src/Content.Infrastructure --startup-project host/Api
```

Identical output or the series does not merge.

---

## Tests

No new behaviour — the suites *are* the test. Additional gates: architecture tests (allowlist
empty), `dotnet ef migrations list` diff per context, and `docker build` (the Dockerfile is
rewritten for the new paths — the Stage 10 Mailer fix must survive).

Nothing semantic ships in this PR. Any behaviour change discovered mid-move gets its own commit
**before** the move commit that exposed it.

---

## Rollout

Development-freeze coordination: this is the one stage where parallel feature branches all
conflict. Land it between releases; rebase cost for any open branch is the move map, which is
why 18.1 commits it as a file others can script against.

---

## Verification

1. Build/format/unit/integration/architecture green; 0 warnings.
2. `dotnet ef migrations list` × 4 contexts — identical to pre-move capture.
3. `grep -rn "Microsoft.AspNetCore" modules/*/src/*.Domain` → empty.
4. `docker build .` succeeds; CI paths green.
5. `git log --follow` on a sampled moved file shows pre-move history.

---

**PR:** `refactor(structure): shared kernel split, layer projects and entity partials`
