# Stage 18 — Project restructure (shared foundation, layer projects, per-module tests)

Closes **[01 §1.9]**, **[02 §7]**, **[02 §10]**, **[03 §1]**, **[13]**, and executes the
[module-restructure-study](../module-restructure-study/README.md)'s **pragmatic verdict** — not the
all-in tree in `03-full-target-structure.md`, which the study itself rejects on cost accounting
(`07-migration-plan-and-verdict.md`: "The benefit does not clear the cost").

**Deliberately second-to-last.** Every earlier stage conflicts with these file moves; Stage 14's CPM
and `Architecture.Tests` must exist first so the project growth is version-coherent and
boundary-enforced from the first commit.

**Prerequisites, verified against the tree:** `Directory.Packages.props` and `Directory.Build.props` exist;
`tests/Architecture` exists; Stage 15 is complete; all four modules report **0** `Domain → own Application`
imports; and `[15 §15.2]` — the `WebEncoders` call that would stop compiling the moment `Shared.Domain`
drops its packages — is already fixed (`NewsletterSubscriberEntity` uses `Base64Url.EncodeToString`).
Nothing blocks the start.

---

## Reconciliation: the sources disagreed, this is the ruling

Three documents specify the shared foundation and **none of them agree**. This stage is the single
source of truth; the three are annotated to point here.

| Source | Proposed | Ruling |
| --- | --- | --- |
| [11 §Target solution graph](../11-project-structure-and-packages.md) | `Shared.Domain` / `Shared.Application` / `Shared.Infrastructure` / `Shared.Contracts` / `BuildingBlocks` | **Naming adopted for the domain leaf** (`Shared.Domain`); its `Shared.Application`/`Shared.Infrastructure` become `BuildingBlocks.*` per study 09 |
| [12 §What each should be](../12-shared-kernel-and-buildingblocks.md) | `Shared.Kernel` + layered `Shared.*`; `BuildingBlocks` → constants leaf | **Reasoning adopted, name rejected** — by doc 12's own header, a DDD Shared Kernel is domain model co-owned by ≥2 contexts, which these base types are not, so `Shared.Domain` is the honest name |
| [study 09](../module-restructure-study/09-sharedkernel-vs-buildingblocks-rules.md) | `SharedKernel` (1) + `BuildingBlocks.{Domain,Application,Infrastructure,Presentation}` (4) | **Structure adopted** — technical concerns genuinely exist at every layer; only the `SharedKernel` name is replaced |

**Ruling:** `Shared.Domain` + `BuildingBlocks.{Domain,Application,Infrastructure,Presentation}`.
Five projects, study 09's shape, doc 11's name, doc 12's argument. Study 09's open question —
"Specification → `BuildingBlocks.Domain` … that one line in 08 should be reconciled" — is settled
here in favour of 09: a specification is a technical pattern, not a business concept.

## What has changed under the sources since they were written

Measured against the tree, not assumed:

- **Doc 11's central argument is spent.** It justifies layer projects from live leaks ("Content's
  domain imports its application layer in 74 places"; `UserEntity` taking `UserErrors`). Today
  **all four modules report 0** `Domain → own Application` imports, and `LayerDependencyTests`
  already guards it. The remaining honest case for the split is the *transitive package graph*:
  `Content.csproj → Shared.csproj` still drags Carter, EF Core, Quartz and Swashbuckle into the
  domain's graph. That is the reason to cite, and it is narrower than doc 11's.
- **Doc 11 wants every module layer-split**, "uniformity is the point". The study calls that pure
  overhead for Core/Mailer (37 and 82 files). **This stage follows the study** and says so, rather
  than claiming both.
- **Study 09's dead-package claim is ⅓ right.** `Bogus` is genuinely unused in `Shared/` (0 files);
  `Mapster` (3) and `StackExchange.Redis` (1) are live. Drop Bogus only.
- **Project count.** The study's cost accounting argues against "13 → 30". Today is **15**. This
  stage lands **39**, counted off the solution: 6 Content + 7 Identity + 7 Storage + 7 Mailer +
  5 `src/shared/src` + 2 `src/shared/tests` + 4 top-level `tests/` + `Api`. Twelve of those 24 new
  projects are test/fixture projects, which is where D8 spends, and four more are the Storage and Mailer
  layers D1 added on the owner's decision. The increase is the decision being taken knowingly, not a
  side effect.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Which module shape | all-in (per-module `.Api`, every module layered), or the study's pragmatic shape | **Every module layered — `<M>.Domain` / `<M>.Application` / `<M>.Infrastructure` plus its Contracts — on the owner's decision, which reinstates doc 11's uniformity and overrides the study.** The earlier ruling here kept Storage and Mailer as one project each on the study's pricing of layer projects as overhead for modules of 37 and 82 files; the owner's call is that a module's shape should not depend on its current size, so all four are identical and there is one rule to learn instead of two. **Endpoints still stay in `<M>.Application`** (a separate presentation project shreds the 293 vertical slices) — that half of D1 is unchanged. Measured cost of the reinstatement, taken in 18.6: 6 project files, and two consequences the compiler surfaced — `Storage.Domain` and `Mailer.Domain` reference their own `.Contracts` (their entities use the enums it publishes, `EnumStoredFileKind` and `EnumNotificationType`, which exist once and are read by other modules), and `Storage.Contracts` grants `InternalsVisibleTo` to `Storage.Infrastructure` rather than to the module (`StoredFile`'s constructor is `internal`, and the service that calls it is now in the outermost layer). |
| D2 | Shared foundation | doc 11's `Shared.*`, doc 12's `Shared.Kernel`, or study 09's `SharedKernel` + 4 × `BuildingBlocks` | **`Shared.Domain` + `BuildingBlocks.{Domain,Application,Infrastructure,Presentation}`** — see the reconciliation table. `Shared.Domain` carries `Aggregate<T>`, `Entity<T>`, `IAggregate`, `IDomainEvent`, `EnumAuditActor` and **zero package references**; that zero is what finally takes the web stack out of every module domain's transitive graph `[01 §1.9]`. `Shared.Contracts`' nine CQRS interfaces fold into `BuildingBlocks.Application`. |
| D3 | `Specification<T>` / `IRepository<T>` | `Shared.Domain` (study 08), or `BuildingBlocks.Domain` (study 09) | **`BuildingBlocks.Domain`.** They are technical patterns over the domain, not business concepts, and `Specification<T>` needs `System.Linq.Expressions` — keeping it out preserves `Shared.Domain`'s zero-package rule. Settles the conflict study 09 flagged and 08 left open. |
| D4 | Today's `BuildingBlocks` constants | keep as one leaf, or send single-module constants home | **Send them home `[02 §10]` / `[12]`, minus the two members that are genuinely global.** `RoleConstants`, `SessionConstants`, `PermissionConstants`, `JwtClaimsConstants` and the rest of `UserConstants` → `Identity/Domain/Constants/`; `FileConstants` → `Storage/Domain/Constants/`. **Two exceptions, measured against the tree** (both would otherwise make a module reference another module's Domain, which `ModuleBoundaryTests` forbids): `UserConstants.{DefaultLocale,SupportedLocales}` is read by `LocalizationExtension`, Mailer (4 sites) and Content (1), so the locale trio (`DefaultLocale`, `SupportedLocales`, `MaxLocaleLength`) **stays global** in `Shared.Domain/Constants/LocaleConstants.cs` — **measured in 18.4: `BuildingBlocks.Presentation` was the wrong home.** `UserEntity.PreferredLocale` initialises to `DefaultLocale`, and that is a module `Domain` file, which reaches only `Shared.Domain` and `BuildingBlocks.Domain`. The trio is read from four places that share no other layer (a module's Domain, Application and Infrastructure, and the shared Presentation extension), so it belongs in the zero-package project every one of them sees, beside `EnumAuditActor` and `DomainRuleException` — inert domain vocabulary, which is what it is — an extraction in place, not a move: all three already sit in `BuildingBlocks`, and only the enclosing class name (`UserConstants`) makes them read as an Identity concern, which is the trap D4 walks into; and `FileConstants` is read by two other modules, so **five** of its members move to **`Storage.Contracts/Domain/Constants/FileUploadLimits.cs`**, which both already reference: `{MaxVideoFileSizeBytes,AllowedVideoExtensions}` for Content's `EditorialValidation`, and — **measured in 18.6, D4 as first written missed these** — `{MaxAvatarFileSizeBytes,AllowedAvatarMimeTypes,AllowedAvatarExtensions}` for Identity's `Application/Auth/Validators/FileValidation.cs`. The avatar rules are the same case as the video rules and the same fix; the remaining ten members (persistence lengths, the raw-upload trio, `AllowedVideoMimeTypes`) are read only by Storage and stay in its Domain — which is what `[02 §10]` said in the first place ("expose only the values validators need via `Core.Contracts`"); D4 lost the clause when it summarised. Only genuinely global inert vocabulary stays otherwise — rate-limit policy names, authorization policy names, `ApiVersionUrl` — and that is what becomes `BuildingBlocks.Presentation`. |
| D5 | Visibility | leave `public`, or `internal` default | **`internal` by default** per new assembly, `InternalsVisibleTo` only for that module's own test assemblies `[02 §7]`. Measured in 18.10: 391 of 3,164 types, and **no `InternalsVisibleTo` needed** — the criterion is "named by no other project", so a type a test names never qualifies. Where a seam genuinely needs crossing, that is a Contracts type, not a `public` escape hatch. Stage 14's boundary allowlist ratchets to zero here. **Note the test-data consequence:** `<M>.TestData` constructs entities, so the grant must name it too, or every builder stops compiling the moment the sweep lands. |
| D6 | Entity files | leave as-is, or the study-10 partial split | **Partial split** above 300 lines — measured today: `Lyrics` 805, `Article` 796, `Video` 729, `User` 488, `Category` 452, `Artist` 447, `ContentOrder` 412. State stays in `Entities/X.cs`; behaviour clusters move to `Behaviors/X.Editorial.cs` etc. The junction/child demotion `[03 §1]` **already shipped in Stage 15 Part C** — Content measures 37 aggregates / 12 entities, its target — so this stage only owns the partials. No `Behaviors/` folder exists yet. |
| D7 | The `Core` rename | defer again, or land it here | **Land it, schema and all.** Stage 14 already did the hard part (contract extraction, avatar → Identity, `SlugHelper` → Content), leaving the name: `Storage.Contracts`, ~45 mechanical renames, and one migration doing `ALTER SCHEMA core RENAME TO storage` carrying `__EFMigrationsHistory` with it. That migration is the only irreversible step in this stage and the reason the rename belongs in a file-move PR `[13]`. `IImageColorService` **stays** — it writes `core.files`' own colour columns at upload, so it is storage metadata, not a leak (Stage 14 D3). **Mailer does not move into Storage**; notification preferences go to a future `Settings` module `[13]`. |
| D8 | Test layout | one shared unit + one shared integration project, or per module | **Per module, owner decision, overriding the study.** Each module gets `<M>.TestData` (a library), `<M>.Unit.Tests` and `<M>.Integration.Tests`. The study rejected per-module integration projects at "4× the Testcontainers cost"; that cost is not inherent — see D9. Measured split of today's 197 test-data files: **122 Content, 53 Identity, 5 Core, 0 Mailer, ~10 genuinely cross-module**. **Every module gets a `<M>.TestData`, Mailer included, on the owner's decision** — the zero is a gap in Mailer's tests, not a reason to skip the project: its tests built entities inline (32 `NewsletterSubscriberEntity.Subscribe`, 11 `NotificationEntity.Create`, 4 `OutboxEmailEntity.Enqueue`), so 18.7 writes the three missing builders in the same shape as `FileBuilder` and routes those call sites through them. The entity guard tests keep calling the factories directly, because they exist to pass values a builder would never produce. Corrected in 18.7: the 30 `TestConstants` files are **not** part of that split — they are parts of one partial class that every integration suite imports statically, so they stay whole in `tests/TestData`. The builders, factories and mocks split as measured. |
| D9 | The integration harness | accept N containers, or make one serve every assembly | **One container, N assemblies.** `TestPostgresContainer` already starts one container per assembly and leases each fixture a private database cloned from a migrated template — the multiplication is only that it is a `static` singleton with no cross-process reuse. Three changes remove it: `.WithReuse(true)` with a stable label so every assembly attaches to one container; a `pg_advisory_lock` replacing the in-process `SemaphoreSlim` so exactly one process migrates the template; and `LeaseDatabaseAsync` names prefixed by assembly so two modules' identically-named fixtures cannot collide. Where reuse is unavailable (most CI), it degrades to one container per assembly — correct, just slower. |
| D10 | Test library naming | keep `Fixtures` for the data library and invent a name for the boot library, or use the words for what they mean | **`TestData` and `Fixtures` — the repo had the word backwards.** Today's `tests/Fixtures` (`_116.Tests.Fixtures`) holds 111 builders, 46 factories, 30 constants files and 9 helpers: that is **test data**, and nothing in it is a fixture in the xUnit sense. The actual xUnit fixtures — `ApiFixture`, `PostgresFixture`, `TestPostgresContainer`, `TestRedisContainer`, the `*Collection` classes — live in `tests/Integration/Common/Fixtures/`, beside the base classes, seeders and stubs that surround them. So the data library becomes **`tests/TestData`** (`_116.Tests.TestData`, per module `<M>.TestData`) and the boot library becomes **`tests/Fixtures`** (`_116.Tests.Fixtures`) — no invented word, and the folder that holds the fixtures is the one called Fixtures. The cost is a scripted rename of the `_116.Tests.Fixtures` namespace across ~1,060 `using` lines; D14 already accepts a 2,395-file pass of the same kind, the one-commit swap (D13) keeps `git log --follow` intact regardless, and the earlier objection ("churn for nothing") was preserving a misnomer. |
| D11 | Migrations | regenerate, or relocate | **Relocate only**, per study 06: `Migrations/` folders move with their Infrastructure project, `MigrationsAssembly` is pointed explicitly, and `dotnet ef migrations list` must show the identical list per context before and after. A regenerated migration on a live schema is the failure mode this rule exists to prevent. |
| D12 | Feature-area split | do it here, or defer | **Deferred, explicitly.** Study item #7 (splitting Content/Identity `Application` by `Commerce`/`Editorial`/…) is conditional on *measured, recurring* incremental-build pain on those two modules. No such measurement exists. Recorded so it is not re-opened as an oversight. |
| D13 | Mechanics | big-bang in place, per-module series in place, or a parallel tree | **A parallel tree: `src.refactor/` and `tests.refactor/` at the repository root, beside the untouched `src/` and `tests/`.** The new solution (`116_backend.refactor.sln`) grows green series by series while the old solution is never touched, so both can be built, tested and compared side by side at the end. Pre-compute the full old-path → new-path map before touching anything (the Stage 7 lesson) — with two trees on disk it is also the only record of where each file came from. **Three rules the model depends on.** (1) *The swap is one commit*: `git rm -r src tests` and `git mv src.refactor src && git mv tests.refactor tests` together, so rename detection links every old path to its new one and `git log --follow` survives; split across two commits, history is lost for good. Before the swap the old tree is renamed `src.old/` / `tests.old/` and kept until the owner's explicit go-ahead to delete. (2) *Two solutions, one Dockerfile*: the `.sln`, Dockerfile and CI keep building `src/` until the swap; `src.refactor/` carries its own `.sln`, and the plumbing switch (18.11) is part of the swap, not of the series. (3) *`src/` is frozen*: any fix that must land in `src/` while `src.refactor/` exists is applied twice by hand, using the move map to find the twin — the coordination freeze in Rollout is a hard rule, not advice. |
| D14 | Namespaces | follow the new project names, or preserve today's | **Follow the project, and note that this costs nothing for the modules and 2,395 files for `Shared`.** Measured: module layer namespaces *already* encode the layer (`_116.Content.Domain.*` / `.Application.*` / `.Infrastructure.*`), so each new layer project sets `<RootNamespace>_116.<M></RootNamespace>` and **not one `using` changes** across the 3,554 files that name a module namespace. `Shared` is the opposite: its namespaces do not match the projects they land in, and leaving `_116.Shared.Application.Decorators` inside `BuildingBlocks.Application.csproj` would make `LayerDependencyTests` and `ModuleBoundaryTests` — which match on namespace — assert something untrue, i.e. break the one thing this stage exists to fix. So the shared namespaces are rewritten with the move. |

---

## Checklist

- [x] 18.1 — Move map committed (every file, old → new, per series), **generated from the tree, not hand-written**:
      `scripts/stage18-move-map.py` emits `implementation-specs/stage-18-move-map.tsv` (columns `series`, `kind`,
      `old_path`, `new_path`, `refactor_path`); the pre-move baseline 18.12 compares against lives in
      `implementation-specs/stage-18-baseline/` (migration list per context, test counts, project count) — the
      counts are `dotnet test -c Release --no-build` **run totals** (8523 unit / 2152 integration / 6
      architecture at `55f8c1ca0`), since that is what 18.12 compares against; the
      parallel tree is created empty (`src.refactor/`, `tests.refactor/`, `116_backend.refactor.sln`) and
      `Directory.Build.props` / `Directory.Packages.props` are confirmed to reach it unchanged (`net9.0`,
      `Nullable`, CPM all inherited by a probe project)
- [x] 18.2 — `shared/`: `Shared.Domain` + `BuildingBlocks.{Domain,Application,Infrastructure,Presentation}`;
      `Shared.Contracts` folded into `BuildingBlocks.Application`; `Bogus` absent from the new shared graph
      (its `PackageVersion` line leaves with `Shared.csproj` in 18.13). **Measured:** 151 files copied into
      the 5 projects (8 / 6 / 55 / 21 / 60 `.cs`) by `scripts/stage18-copy-series.py --clean 18.2`;
      `116_backend.refactor.sln` builds 0 warnings / 0 errors; `Shared.Domain` and `BuildingBlocks.Domain`
      carry 0 `PackageReference`; no `_116.Shared.Application|Infrastructure|Contracts` or
      `_116.BuildingBlocks.Constants|Utils` namespace survives in `src.refactor/`. The compiler corrected
      three placements, now folded into the mapping rules below: `IAccountRateLimiter` stays in Application,
      `Configurations/` is Application, `HttpCurrentActor` is Presentation with its `BaseModule` registration
      carved out into `AddHttpCurrentActor()`. New pins in `Directory.Packages.props`:
      `Microsoft.Extensions.{Hosting,Logging,Localization}.Abstractions`, `Microsoft.AspNetCore.HttpOverrides`;
      `Microsoft.AspNetCore.Http.Abstractions` 2.3.0 → 2.3.13 (HttpOverrides requires it; the old solution
      still builds 0 / 0). `BuildingBlocks.Presentation` also takes `Quartz.Serialization.SystemTextJson`
      (`QuartzExtension.UseSystemTextJsonSerializer`). **Not done:** the `Mapster` pin stays on
      `7.4.2-pre02` — the only stable releases above it are Mapster 10.x, a major upgrade that has no place
      in a restructure; it is a follow-up outside Stage 18, not the one-liner `[11 §CPM]` assumed
- [x] 18.3 — Single-module constants leave `BuildingBlocks` for Identity and Storage `[02 §10]`; the
      two cross-module carve-outs stay reachable — locale trio → `Shared.Domain`,
      video and avatar upload limits → `Storage.Contracts` (D4). Do this **before** 18.4/18.5 so the two
      carve-outs are already in place when Identity and Storage split. `LocaleConstants.cs` already exists in
      `Shared.Domain/Constants/` since 18.2 (`LocalizationExtension` needs it to compile);
      `FileUploadLimits` lands with `Storage.Contracts`. **Measured:** the series has no standalone step — its
      destinations are created by the series that build the projects, so it rides along: `LocaleConstants` with
      18.2, the five Identity constants files with 18.4, and `FileConstants` + `FileUploadLimits` + the two
      Cloudinary files with 18.6. Verified after all three: no `UserConstants` or `FileConstants` reference
      survives in Content, Mailer or the shared kernel, and no module reaches another module's Domain — every
      cross-module project edge goes through a `.Contracts` project, the host's four being the only exceptions
      by design.
- [x] 18.4 — Identity → `Identity.Domain/Application/Infrastructure` (+ existing Contracts). **Measured:** 624 files
      (53 / 485 / 70 `.cs`), 0 / 0. `Identity.Application` needs one package of its own,
      `Microsoft.AspNetCore.Authentication.JwtBearer`, because `AuthorizationExtensions` configures
      `JwtBearerOptions`; the other six stay with `Identity.Infrastructure`. `IdentityModule.cs` moves to
      `_116.Identity.Infrastructure` — the one namespace the module split changes, since D14 ties a namespace
      to its project and the module class registers the DbContext
- [x] 18.5 — Content → `Content.Domain/Application/Infrastructure`. **Measured:** 1,637 files
      (107 / 1,302 / 156 `.cs`), 0 / 0, no packages of its own at any layer — everything it uses arrives
      through `BuildingBlocks.Presentation`. `ContentModule.cs` → `_116.Content.Infrastructure`, as Identity's
- [x] 18.6 — `Core` renamed `Storage`, **schema included**; Storage and Mailer layered like the other two (+ Contracts) `[13]`. **Measured:** 227 files into `Storage.{Domain,Application,Infrastructure}` (+ `Storage.Contracts`) and `Mailer.{Domain,Application,Infrastructure}` (+ `Mailer.Contracts`), building 0 / 0 with `Identity.Contracts` copied early (a leaf with no references, which Mailer references). Packages land where the imports are: `CloudinaryDotNet`, `Polly.Core` and `SixLabors.ImageSharp` in `Storage.Infrastructure`, `MailKit` in `Mailer.Infrastructure`, nothing at the other layers. The rename is **24 identifiers**, computed rather than prefix-matched: `Core*` names *declared in the module* (plus its test classes and the four test aliases pointing into its namespace), which is what keeps EF Core's own `CoreEventId` and Identity's `CoreRoleCannotBeDeleted` / `CoreRoleName` / `CoreUserRole` out of the set. Also renamed: the schema (`StorageConstants.SchemaName` → `"storage"`), `ModuleName` → `"Storage"`, the six `core.file.*` rule codes → `storage.file.*` (measured: neither the dashboard nor the frontend matches on any rule code, so no client contract breaks) and every `core.files` reference in prose. Untouched, deliberately: the `Migrations/` folder (history, including its `schema: "core"` and raw SQL — the schema move is a new migration, not an edit to old ones) and the phrase "Entity Framework Core". The host (`src/Api` → `src/host/Api`) references every module, so it is copied after 18.4/18.5; it takes `DotNetEnv` as a package of its own, which it used to get transitively through `Shared.csproj` and only `Program.cs` uses, and gains the one line `builder.Services.AddHttpCurrentActor();`. `RenameCoreSchemaToStorage` is generated against the host as startup project: `dotnet ef migrations list` is then **identical to the baseline for Identity (11), Content (33) and Mailer (4), and the old Core list plus one for Storage (10)**, and `has-pending-model-changes` reports none for all four
- [x] 18.7 — Tests re-rooted: `<M>.TestData` / `<M>.Unit.Tests` / `<M>.Integration.Tests` per module; `Shared.Integration.Tests` under `shared/`; `tests/{TestData,Fixtures,EndToEnd.Tests,Architecture.Tests}` top-level; the `_116.Tests.Fixtures` → `_116.Tests.TestData` namespace rename rides in the same series (D10)
      **Measured — two things per-module test assemblies force, neither of them optional:**
      (1) **Every suite declares its own `[CollectionDefinition]`.** xunit resolves a collection definition only
      inside the assembly that uses it (analyser `xUnit1041`, 784 hits on the first attempt: *"if it comes from a
      collection definition, ensure the definition is in the same assembly as the test"*), so the nine one-line
      definitions cannot live in the shared boot library — they dissolve into a `Collections.cs` per suite naming
      exactly the collections it joins, while the `*PostgresFixture` classes they point at stay in `tests/Fixtures`.
      Measured per suite: Content and Identity integration → `Database`, `Seeding`; Mailer → `Database`, `Resend`;
      Storage → `Database`; Shared integration → `AccountRateLimiting`, `Cors`, `Database`, `OtpPepperless`,
      `RateLimiting`; EndToEnd → `Database`, `UnreachableDatabase`; the three unit suites that need it →
      `EnvironmentVariable`. Left as it was: `VisitorRoleSeederCollection`, already declared in the file that uses it.
      (2) **Every integration suite declares `[assembly: AssemblyFixture(typeof(TestContainersFixture))]`** for the
      same reason — an assembly-level attribute in a referenced library is never scanned — so the container lifetime
      is armed per test assembly (`AssemblyFixtures.cs` × 6) against the one fixture class in the boot library.
      Also measured: the old integration suite's `GlobalUsings.cs` carried `global using static
      …Constants.TestConstants` plus two aliases, which is why **`TestConstants` stays one partial class in
      `tests/TestData`** rather than splitting per module as D8's file census implies — 30 files are parts of one
      class, a class cannot span assemblies, and splitting the static import would make nested names such as
      `Role` ambiguous. The builders, factories and mocks do split per module, which is what D8 was about.
- [x] 18.8 — Fixtures: one shared container, advisory-lock template migration, assembly-prefixed database leases.
      **Measured — D9's mechanism needed replacing, its outcome did not.** `.WithReuse(true)` does attach a second
      process to the running container, but then times out re-running its readiness checks against it
      (`TimeoutException` out of `DockerContainer.CheckReadinessAsync`, 38 of 47 tests in the second assembly), and
      a custom wait strategy does not help. The coordination is ours instead: a fixed container name
      (`116_integration_postgres`) and host port (`54329`, overridable with `_116_TESTS_POSTGRES_PORT`), each
      process connecting first and starting the container only if nothing answers — whoever loses that race waits
      for the winner's server. Two further findings: **cleanup must be off** (`WithCleanUp(false)`), because the
      resource reaper deletes the container when the process that created it exits, which took the server away
      from the assemblies still running and surfaced as `connection reset by peer` mid-suite; and **Redis is
      deliberately not shared**, because `BaseApiTest` calls `FlushDatabaseAsync` between tests and a shared
      keyspace would let one assembly evict another's cache — it is also the cheap container, so the saving was
      never there. The template migration is serialized with `pg_advisory_lock` plus a `template_ready` marker
      table (the template database is created empty by the container, so its existence proves nothing), leases
      drop-then-create so a surviving container serves a second run, and each assembly drops its own databases on
      the way out. Lease names are prefixed with the assembly's **first segment**, not its full name: PostgreSQL
      truncates identifiers at 63 bytes silently, and `test_116_identity_integration_tests_accountratelimited…`
      overruns it. Opt out of all of it with `_116_TESTS_NO_CONTAINER_REUSE=1`, which gives one disposable
      container per assembly.
- [x] 18.9 — Entity/behavior partials above 300 lines (the demotion to `Entity<Guid>` shipped in Stage 15 — Content is 37 aggregates / 12 entities already).
      **Measured:** the same seven files D6 names, split by `scripts/stage18-split-entities.py` into **19 behaviour
      partials** — `Lyrics` 4, `Article` 3, `Video` 3, `User` 3, `Category` 2, `Artist` 2, `ContentOrder` 2 — with
      state, the private EF constructor and the factories left in `Entities/<X>.cs`. Every file is now under 300
      lines; `Lyrics` needed a fourth cluster (`Creation`) because 30 documented properties keep its state file at
      351 lines on their own, so its factories moved too. Clusters are named for the concern, not the layer:
      `Editorial` / `Publication` / `Promotion` for the three content aggregates, `Catalogue` / `Pricing` for
      Category, `Profile` / `SocialLinks` for Artist, `Composition` / `Payment` for ContentOrder, and
      `Credentials` / `Profile` / `Access` for User. **`Behaviors/` is a file grouping, not a namespace:** a
      partial class has one namespace by definition, so these files declare `…Domain.Entities` while sitting in
      `Behaviors/`. It is the one place in the tree where namespace and folder deliberately differ.
- [x] 18.10 — `internal` sweep + `InternalsVisibleTo` (unit tests **and** test data); `Architecture.Tests` allowlist → 0.
      The allowlist was already empty. The sweep is computed rather than listed, by
      `scripts/stage18-internal-sweep.py`: **of 3,164 public types in `src/`, 791 are named by no file in any
      other project, and 391 survive the accessibility closure** — a type can only be internal if nothing still
      public exposes it, which is a fixed point, not a single pass (`PublicVideoDetailDto` is named only inside
      its own project but appears in a public `Result`'s signature). **No `InternalsVisibleTo` follows from this
      sweep at all**, which is the useful property of the criterion: anything a test names is not a candidate.
      Three exclusions, each measured from a failure:
      (1) **Scrutor skips non-public classes** — the handler scan is opted in with `publicOnly: false`; Carter
      (`GetTypes()`), FluentValidation (`includeInternalTypes: true`) and EF's `ApplyConfigurationsFromAssembly`
      already see internals.
      (2) **Static classes holding extension methods stay public** — an extension method is called through its
      receiver, so the projects that depend on it never name the class; the analysis read `HttpContextServiceExtension`
      and `CurrentActorExtension` as unused and broke 20 call sites.
      (3) **`IEntityTypeConfiguration` implementations stay public** — EF's scan silently skips an internal one,
      the model quietly loses that configuration, and the next migration check reports pending changes. A seeding
      integration test caught it; the build could not.
      All 10,681 tests pass with the sweep applied.
- [x] 18.11 — **The swap, one commit** (D13): `src` → `src.old`, `tests` → `tests.old`, `src.refactor` → `src`,
      `tests.refactor` → `tests`, `116_backend.refactor.sln` → `116_backend.sln`; in that same commit the Dockerfile
      switches to a `COPY **/*.csproj` glob (13 hardcoded paths today, every one breaks at the swap), the `tests.yml` /
      `build.yml` paths move off `tests/Unit` / `tests/Integration` / `tests/Architecture`, the two
      `coverage*.runsettings` assembly filters follow, and one `.slnf` per module is added. `src.old/` and
      `tests.old/` stay, excluded from the solution, CSharpier and coverage, until the owner's go-ahead to delete.
      **Measured:** git detects **4,257 renames** across the swap, so `git log --follow` survives it; the single
      solution holds **39 projects** and no `.old` one; `dotnet csharpier check .` passes with `src.old/` and
      `tests.old/` added to `.csharpierignore`; `docker build .` succeeds; and every suite passes through the
      commands CI now runs (`dotnet test tests/unit.slnf`, `tests/integration.slnf`, `tests/Architecture.Tests`).
      Three of the four items needed more than a path edit:
      **(a) The Dockerfile does not get a `**/*.csproj` glob.** Docker flattens such a glob unless the labs
      `COPY --parents` is available, which CI cannot assume, so the 13 project lines become one `COPY src/ ./src/`
      and the restore narrows from the solution to `src/host/Api/Api.csproj` — which also means the test projects
      no longer need copying into the image at all.
      **(b) CI runs solution filters, not directories.** `tests/Unit` and `tests/Integration` no longer exist as
      single projects, and naming six suite directories would mean editing the workflow for every new module, so
      `scripts/stage18-solution-filters.py` generates `tests/unit.slnf` and `tests/integration.slnf` from the
      solution alongside the four module filters. Note that a `.slnf`'s `solution.path` is relative to the filter
      while its project paths are relative to the **solution**.
      **(c) The coverage filters needed more than a path.** `[*Tests*]*` caught the old `_116.Tests.Fixtures`
      assembly by accident of its name; `Fixtures`, `TestData` and `<M>.TestData` are not matched, so they join the
      exclude list. `<IncludeDirectory>../src/**/*.cs</IncludeDirectory>` goes entirely: the suites no longer sit at
      one depth, so no single relative path names `src/` for both `tests/` and `src/modules/<M>/<M>/tests/`.
      The `scripts/stage18-*.py` pipeline is historical from here: it read the pre-swap tree, which is now
      `src.old/`, and `stage-18-move-map.tsv` remains the record of what moved where
- [~] 18.12 — Verify (below) — items 1–6 run against **both** trees before the swap, 7–9 after it.
      **All pass**, with one correction to D7's own premise (the migration history is in `public`, not in the
      module's schema — see that section). Item 8, `git log --follow` on a sampled moved file, is the only one
      still open: it cannot be checked until the swap commit exists. Staged and ready, git already reports
      4,257 renames, which is what item 8 will read.
- [ ] 18.13 — Delete `src.old/` and `tests.old/` on the owner's explicit go-ahead, once the new suites have been green in CI

---

## The target tree

```text
src/
  modules/
    Content/
      Content/
        src/    Content.Domain/  Content.Application/  Content.Infrastructure/
        tests/  Content.TestData/  Content.Unit.Tests/  Content.Integration.Tests/
    Identity/
      Identity/
        src/    Identity.Domain/  Identity.Application/  Identity.Infrastructure/
        tests/  Identity.TestData/  Identity.Unit.Tests/  Identity.Integration.Tests/
      Identity.Contracts/
    Storage/
      Storage/
        src/    Storage.Domain/  Storage.Application/  Storage.Infrastructure/
        tests/  Storage.TestData/  Storage.Unit.Tests/  Storage.Integration.Tests/
      Storage.Contracts/
    Mailer/
      Mailer/
        src/    Mailer.Domain/  Mailer.Application/  Mailer.Infrastructure/
        tests/  Mailer.TestData/  Mailer.Unit.Tests/  Mailer.Integration.Tests/
      Mailer.Contracts/
  shared/
    src/    Shared.Domain/  BuildingBlocks.Domain/  BuildingBlocks.Application/
            BuildingBlocks.Infrastructure/  BuildingBlocks.Presentation/
    tests/  Shared.Unit.Tests/  Shared.Integration.Tests/
  host/
    Api/
tests/
  TestData/            library — the ~10 module-agnostic builders/helpers
  Fixtures/            library — ApiFixture, the Postgres/Redis containers, collections, base classes, seeders, stubs (47 files)
  EndToEnd.Tests/      whole-app flows: the 23 Workflows + the 3 Api surface tests
  Architecture.Tests/  solution-wide boundary rules
```

The repository root keeps exactly two code folders, **`src/`** (`modules/`, `shared/`, `host/`) and
**`tests/`** (the solution-wide test libraries and projects), as today. Endpoints stay in
`<M>.Application`. **A module folder wraps two peers, `<M>/` and `<M>.Contracts/`**,
and `<M>/` carries its own `src/` and `tests/`. That is today's `src/Modules/<M>/{<M>, <M>.Contracts}`
with `src/Modules/` dropped, which keeps the published seam out of the module's layer list and makes
the Storage and Mailer copies now split the same way (D1, owner decision). Content has no Contracts project
(nothing consumes it).

Four notes on the test layout, each of which took a wrong turn first:

- **`Fixtures` is its own library, not part of `TestData`.** All five integration projects need it,
  and it drags Testcontainers, Respawn and `WebApplicationFactory` with it. Folding it into
  `TestData` would put that graph inside every *unit* test project too — the same mistake
  `Shared.csproj` makes today with EF. (D10: this library is the one that actually holds the xUnit
  fixtures — `ApiFixture`, `PostgresFixture`, the collections — which is why it gets the name.)
- **`Shared.Integration.Tests` lives under `shared/`, not in the top-level bucket.** The 18 files
  there (outbox, interceptors, middleware, CORS, rate limiting, decorators) exercise shared
  infrastructure through HTTP; they belong to the thing they test.
- **`EndToEnd.Tests` stays top-level, not under `host/`.** Colocating it with the host was the first
  answer and the argument is real — those flows do test what the host composes. But all 23 reference
  fixtures from three different modules, so putting them under `host/` makes the deployable's folder
  depend sideways on four test libraries. `src/host/` stays a leaf, and study 03 was corrected to match.
- **Mailer has no `Mailer.TestData`** because it has no builders or factories today (measured: 0).
  The project appears when the first one does.

### Reference direction

```text
Shared.Domain                    (0 packages)
    ▲
BuildingBlocks.Domain            (Specification<T>, IRepository<T>; System.Linq.Expressions only)
    ▲
BuildingBlocks.Application       (CQRS interfaces, decorators, pagination, UoW, exceptions, metadata)
    ▲                       ▲
BuildingBlocks.Infrastructure ◄─ BuildingBlocks.Presentation
(EF, Npgsql, Quartz, Redis,      (ASP.NET, Carter, Swashbuckle, Asp.Versioning;
 BaseModule, interceptors,        middleware, exception→ProblemDetails strategies,
 outbox, cache, seeding,          rate-limit policies, HttpCurrentActor, policy
 IScheduledJob)                   names, ApiVersionUrl)

Presentation references Infrastructure (outermost layer, measured need: `DbUpdateExceptionStrategy`
renders an EF/Npgsql exception as a ProblemDetails; Stage 20 D1/D2's detector abstraction removes
that need and the arrow can go when it lands). `BuildingBlocks.Application` keeps two small ASP.NET
2.3 abstraction packages: `Microsoft.AspNetCore.Http.Abstractions`, because `RuleProblem` carries an
`HttpContext` in its signature, and `Microsoft.AspNetCore.HttpOverrides`, because
`WebEnv.TrustedProxies()` returns `IPNetwork` and `EnvSchema.ValidateAtBoot` forces every schema
(the host's shared framework supersedes both at run time); every `<M>.Application` already holds
Carter endpoints (D1), so this adds nothing to any module's transitive graph.

<M>.Domain         ─► Shared.Domain, BuildingBlocks.Domain
<M>.Application    ─► <M>.Domain, BuildingBlocks.Presentation, <other>.Contracts
<M>.Infrastructure ─► <M>.Application
Api                ─► every <M>.Infrastructure
```

**Measured in 18.4/18.5 — `<M>.Application` references `BuildingBlocks.Presentation`, not
`BuildingBlocks.Application`.** D1 keeps endpoints in the slice, so a module's Application layer *is* its
presentation layer: 66 files in Identity and 221 in Content import Carter and
`Microsoft.AspNetCore.{Routing,Builder,Http}`. Two more measured needs put EF there as well —
`EF.Functions.ILike` in the Identity role/permission specifications and `NpgsqlException` in
`AccountStatusRequirementHandler`. One reference to the outermost shared layer covers all of it, since each
shared layer references the one below. `<M>.Infrastructure` then needs no shared reference of its own.
`<M>.Domain` stays pure: zero packages, zero ASP.NET or EF imports, measured on both modules.

**Measured: a module's layers grant each other `InternalsVisibleTo`.** Splitting one project into three turns
`internal` from module-wide into layer-wide, and Stage 15 made the member factories `internal`
(`UserRoleEntity.Create`, `RolePermissionEntity.Create` — the Identity seeders call them). The module stays the
encapsulation unit: `<M>.Domain` grants `<M>.Application` and `<M>.Infrastructure`, and `<M>.Application`
grants `<M>.Infrastructure`. This is 18.10's rule arriving early, because the split cannot compile without it.

### Deterministic mapping rules

- `src/Modules/<M>/<M>/Domain/**` → `src/modules/<M>/<M>/src/<M>.Domain/**`
- `src/Modules/<M>/<M>/Application/**` → `src/modules/<M>/<M>/src/<M>.Application/**` (endpoints included)
- `src/Modules/<M>/<M>/Infrastructure/**` → `src/modules/<M>/<M>/src/<M>.Infrastructure/**` (migrations ride along)
- `src/Modules/<M>/<M>.Contracts/**` → `src/modules/<M>/<M>.Contracts/**` (a peer of the module's own project folder)
- `src/Shared/Shared/Domain/**` **except `IRepository.cs`** → `src/shared/src/Shared.Domain/**` — the seven base types plus `Exceptions/DomainRuleException.cs`, which is domain vocabulary per [study 09](../module-restructure-study/09-sharedkernel-vs-buildingblocks-rules.md). Namespaces `_116.Shared.Domain` and `_116.Shared.Domain.Exceptions` are **unchanged**, so the 178 files naming them are untouched (D14).
- `src/Shared/Shared/Domain/IRepository.cs` + `Application/Specifications/**` → `src/shared/src/BuildingBlocks.Domain/`
- `src/Shared/Shared.Contracts/Application/CQRS/**` + `Shared/Application/{Decorators,Pagination,Persistence,Metadata,DTOs,Services}/**` + `Application/Exceptions/**` **except `Handlers/`** → `src/shared/src/BuildingBlocks.Application/`
- `Shared/Application/Exceptions/Handlers/**` (18 files — the `ProblemDetails`/`HttpContext` rendering of exceptions) + `Application/Builders/RateLimit/**` except `IAccountRateLimiter.cs` (5 files — `Microsoft.AspNetCore.RateLimiting` policies, `RateLimitPartitioning` over `HttpContext`, and `AccountRateLimiter`, which needs `System.Threading.RateLimiting` and the `RateLimit` policy constants; the interface stays in `BuildingBlocks.Application/Builders/RateLimit/` for `AccountRateLimitDecorator`) + `Infrastructure/Services/HttpCurrentActor.cs` (`IHttpContextAccessor`, plus `ClaimsPrincipal.FindFirstValue` from `Microsoft.AspNetCore.Authentication.Abstractions`) → `src/shared/src/BuildingBlocks.Presentation/` — **measured in 18.2**: the first draft sent all of these to Application, where 27 of 71 files would have imported ASP.NET
- `Shared/Application/Jobs/IScheduledJob.cs` (a Quartz `IJob` contract) → `src/shared/src/BuildingBlocks.Infrastructure/Jobs/`
- `src/Shared/Shared/Infrastructure/**` except `HttpCurrentActor.cs` → `src/shared/src/BuildingBlocks.Infrastructure/`. **Carve-out (measured in 18.2):** `BaseModule.RegisterInterceptorsIfNotExists` called `AddHttpContextAccessor()` and registered `ICurrentActor → HttpCurrentActor`, neither of which Infrastructure can see once `HttpCurrentActor` is Presentation; those two lines become `BuildingBlocks.Presentation/Extensions/CurrentActorExtension.cs` (`AddHttpCurrentActor()`), called once from `Program.cs` in 18.6. The `TimeProvider` and logging registrations stay in `BaseModule`
- `src/Shared/Shared/Application/Configurations/**` except `CloudinarySettings.cs` → `src/shared/src/BuildingBlocks.Application/Configurations/` (the env schema is boot configuration; `BaseModule` and `DataSeedingHostedService` read `DatabaseEnv`, so it cannot sit above Infrastructure. `WebEnv.TrustedProxies()` returns `Microsoft.AspNetCore.HttpOverrides.IPNetwork`, and `EnvSchema.ValidateAtBoot` forces every schema including `WebEnv`, so Application takes the `Microsoft.AspNetCore.HttpOverrides` 2.3 abstractions package the same way it takes `Microsoft.AspNetCore.Http.Abstractions`)
- `src/Shared/Shared/Application/{Middleware,Extensions}/**` + `BuildingBlocks/Constants/{Authorization,RateLimit}/**` + `Utils/ApiVersionUrl.cs` → `src/shared/src/BuildingBlocks.Presentation/`
- **Exception to the two lines above:** `Configurations/CloudinarySettings.cs` and `Extensions/CloudinaryExtensions.cs` → `src/modules/Storage/Storage/src/Storage.Infrastructure/` (their unit tests follow, into `Storage.Unit.Tests`). Measured: `CloudinarySettings` is read only by `StorageModule` and `CloudinaryStorageClient`, `CloudinaryExtensions` only by `Program.cs` — single-module config, so D4's own rule sends it home `[study 08 §Hoisted]` — `Extensions/` is the host composition surface (Carter, Swagger, versioning, rate limiting, localization)
- `BuildingBlocks/Constants/{Role,Session,Permission,JwtClaims}Constants.cs` → `src/modules/Identity/Identity/src/Identity.Domain/Constants/`
- `BuildingBlocks/Constants/UserConstants.cs` **minus the locale trio** → `src/modules/Identity/Identity/src/Identity.Domain/Constants/UserConstants.cs`
- `UserConstants.{DefaultLocale,SupportedLocales,MaxLocaleLength}` → `src/shared/src/Shared.Domain/Constants/LocaleConstants.cs` (**not** Identity — `LocalizationExtension`, Mailer and Content read them, and Identity's own `UserEntity` does too, from its `Domain`; D4)
- `BuildingBlocks/Constants/FileConstants.cs` **minus the two video members** → `src/modules/Storage/Storage/src/Storage/Domain/Constants/FileConstants.cs`
- `FileConstants.{MaxVideoFileSizeBytes,AllowedVideoExtensions}` → `src/modules/Storage/Storage.Contracts/Domain/Constants/FileUploadLimits.cs` (**not** `Storage.Domain` — Content's `EditorialValidation` reads them; D4)
- `tests/Fixtures/**` → `<M>.TestData` by module; the ~10 module-agnostic files → `tests/TestData/`; namespace `_116.Tests.Fixtures` → `_116.Tests.TestData` in the same commit (D10)
- `tests/Unit/Common/Mocks/**` → `<M>.TestData/Mocks/**` by the mocked type's module (39 Content, 12 Identity, 3 Storage); `MockDispatcher`, `BaseHandlerTest`, `EnvironmentVariableCollection`, `NoOpMigrator` and `Common/Helpers/**` → `tests/TestData/`; `BaseContentHandlerTest` → `Content.TestData/`
- `tests/Unit/Modules/<M>/**` → `src/modules/<M>/<M>/tests/<M>.Unit.Tests/**`
- `tests/Integration/Modules/<M>/**` → `src/modules/<M>/<M>/tests/<M>.Integration.Tests/**`
- `tests/Integration/Common/**` → `tests/Fixtures/**` (library; every integration project references it) — its `Fixtures/` subfolder flattens into the root, since the project *is* the fixtures
- `tests/Integration/{Workflows,Api}/**` + `SmokeTest.cs` + `GlobalUsings.cs` → `tests/EndToEnd.Tests/**`
- `tests/Integration/Shared/**` → `src/shared/tests/Shared.Integration.Tests/**`

### Namespace rewrites (D14)

Files are copied into `src.refactor/` (D13); namespaces move with a scripted find-and-replace in the same
commit as the copy, so a series never leaves a file under the wrong namespace. Counts
are **distinct files carrying a `using`**, measured against the tree:

| Old namespace | New namespace | Files |
| --- | --- | --- |
| `_116.Shared.Domain`, `_116.Shared.Domain.Exceptions` | *unchanged* | 178 (no edit) |
| `_116.Shared.Application.Specifications` + `IRepository` from `_116.Shared.Domain` | `_116.BuildingBlocks.Domain.*` | 78 |
| `_116.Shared.Contracts.Application.CQRS`, `_116.Shared.Application.{Builders,DTOs,Decorators,Exceptions*,Jobs,Metadata,Pagination,Persistence,Services}` | `_116.BuildingBlocks.Application.*` | 2,049 |
| `_116.Shared.Infrastructure.*` | `_116.BuildingBlocks.Infrastructure.*` | 81 |
| `_116.Shared.Application.{Extensions,Middleware}`, `_116.BuildingBlocks.{Constants,Utils}` | `_116.BuildingBlocks.Presentation.*` | 604 |
| every `_116.<M>.{Domain,Application,Infrastructure}.*` | *unchanged* | 3,554 (no edit) |

**2,395 distinct files** carry a rewritten `using` (the groups overlap). All of it is one scripted pass
per row, each row its own commit, `dotnet build` between rows. The module split contributes **zero**.

## Test split, measured

| Scope | Test-data files | Integration files |
| --- | --- | --- |
| Content | 122 | 253 |
| Identity | 53 | 84 |
| Storage (Core) | 5 | 2 |
| Mailer | 0 | 12 |
| Cross-module (`tests/TestData`) | ~10 | — |
| Unit mocks and bases (`tests/Unit/Common`) | 62 (40 Content, 12 Identity, 3 Storage, 7 cross) | — |
| Whole-app (`tests/EndToEnd.Tests`) | — | 26 (23 Workflows + 3 Api) |
| Shared infra (`src/shared/tests/Shared.Integration.Tests`) | — | 18 |
| Fixtures (`tests/Fixtures`) | — | 47 (`Common/`) |

`<M>.TestData`, `tests/TestData` and `tests/Fixtures` are **libraries, not test projects**: unit and
integration projects both reference them, and a test project must never reference another test
project. `tests/Fixtures` is referenced by the five integration projects only, which keeps
Testcontainers and `WebApplicationFactory` out of every unit-test graph.

Totals: 253 + 84 + 2 + 12 = 351 module files, + 26 whole-app + 18 shared infra + 47 harness = 442,
plus `SmokeTest.cs` and `GlobalUsings.cs` at the project root (→ `tests/EndToEnd.Tests`) = **444**,
which is the current `tests/Integration` count exactly.

## Fixtures changes (D9)

```csharp
// one container across every test assembly, not one per assembly
private static readonly PostgreSqlContainer Container = new PostgreSqlBuilder("postgres:16-alpine")
    .WithReuse(true)
    .WithLabel("116.integration", "1")
    … // existing tmpfs + max_connections settings unchanged
```

- `SemaphoreSlim Gate` serializes only within one process. With a shared container, N assemblies race
  to create and migrate `test_116_template`; replace it with `pg_advisory_lock` on the maintenance
  database plus an existence check, so exactly one process migrates and the rest wait.
- `DatabaseName => $"test_116_{GetType().Name.ToLowerInvariant()}"` is unique per assembly but not
  across them. Prefix with the assembly name.
- **What the split does not buy:** isolation. `ApiFixture : WebApplicationFactory<Program>` boots the
  whole host, so `Content.Integration.Tests` still starts Identity, Storage and Mailer. Per-module
  boots would need per-module hosts, which is the all-in shape the study rejected. The split is
  organizational — tests live beside the code they cover — and that is reason enough.
- **The likely upside:** every test today sits in `[Collection("Database")]` and runs serially. N
  assemblies run as N processes in parallel, each on its own leased database.

## The `Core` → `Storage` schema rename (D7)

```sql
ALTER SCHEMA core RENAME TO storage;
```

**Measured in 18.12, correcting this decision's own premise:** `__EFMigrationsHistory` does **not** live in
the module's schema. No context configures a history table, so all four share `public.__EFMigrationsHistory`
— verified directly on the migrated test template, where the only tables in `storage` are `files`,
`domain_event_outbox` and `processed_domain_events`. Two consequences, both good:

- **Nothing has to be run by hand.** An earlier draft of this section argued the rename was forced to be an
  out-of-band step, because a renamed context would look for its history in `storage`, find nothing, and
  replay every migration. That reasoning was wrong: the history is in `public` and the rename does not touch
  it, so EF reads the nine applied Core rows exactly where they were and applies only the new migration.
- **The `ALTER SCHEMA` does not carry the history table**, because the table was never in `core`. The rows
  survive for the simpler reason that they never move.

A rollback is the migration's `Down`, or a second `ALTER SCHEMA`.

`RenameCoreSchemaToStorage` therefore cannot be the generated table-by-table move (EF's default:
`RenameTable` × 3), which would leave `core.__EFMigrationsHistory` behind and an empty `core` schema
standing. **Measured in 18.6**, its body is one guarded block that serves both paths:

```sql
DO $$
DECLARE moved record;
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'core') THEN
        CREATE SCHEMA IF NOT EXISTS storage;
        FOR moved IN SELECT tablename FROM pg_tables WHERE schemaname = 'core' LOOP
            EXECUTE format('ALTER TABLE core.%I SET SCHEMA storage', moved.tablename);
        END LOOP;
        DROP SCHEMA core;
    END IF;
END $$;
```

It runs unattended on both paths: on an existing database it finds `core`, moves its three tables into
`storage` and drops it; on one built from scratch `InitCoreSchema` has just created `core`, and the same
block moves the same three tables. Where an operator has already renamed the schema by hand it finds no
`core` and does nothing. `has-pending-model-changes` reports none, which is the other reason the migration
exists: the model snapshot has to match the model. The Core migrations themselves are
never edited: their `schema: "core"` and raw SQL are history. **The model snapshot is the exception** — it
describes the model as it is now, not as it was, so the rename reaches `StorageDbContextModelSnapshot.cs`
too. Leaving it out is not a cosmetic miss: the snapshot then says `core` while the model says `storage`,
`has-pending-model-changes` reports a pending migration, and any fixture that migrates a database fails.

## Tests

No new behaviour — the suites *are* the test. Extra gates: architecture rules with an empty
allowlist, `dotnet ef migrations list` per context, the schema assertions below, and `docker build`
(the Dockerfile is rewritten for the new paths — Stage 10's Mailer fix must survive).

Anything behavioural discovered mid-move gets its own commit **before** the move commit that
exposed it.

---

## Rollout

Development-freeze coordination: this is the one stage where parallel feature branches all conflict.
Land it between releases; the rebase cost for any open branch is the move map, which is why 18.1
commits it as a file others can script against.

Under D13 the freeze is a rule: while `src.refactor/` exists, `src/` takes no change that is not also
applied to its twin. The old tree stays buildable and its suites stay green the whole way through —
that is what makes "verify both" in 18.12 possible — and it is renamed `src.old/` at the swap rather
than deleted, so the deletion (18.13) is a separate, owner-approved step.

---

## Verification

1. Build/format/unit/integration/architecture green; 0 warnings — **in both solutions** before the swap, and
   every test in the old tree still exists in the new one (a test that went missing in the move is a failure).
   **Measured after 18.8, the totals reconcile exactly:**

   | | old tree | new tree |
   | --- | --- | --- |
   | unit | 8,523 | 8,444 (Content 3,741 · Identity 2,896 · Shared 1,120 · Storage 354 · Mailer 333) |
   | integration | 2,152 | 2,041 (Content 1,525 · Identity 393 · Shared 58 · Mailer 47 · Storage 18) |
   | end-to-end | — | 190 |
   | architecture | 6 | 6 |
   | **total** | **10,681** | **10,681** |

   The redistribution is accounted for, not lost: 111 integration tests are the Workflows and Api-surface files
   that become `EndToEnd.Tests`, and 79 are the two whole-app files that leave the unit suites for it —
   `ResourceCompletenessTests` (which inventories every module's resource families) and `CqrsExtensionTests`
   (which needs an assembly with real handlers). 111 + 79 = 190, `EndToEnd.Tests` exactly.
2. `dotnet ef migrations list` × 4 contexts — identical to the pre-move capture, except Storage's,
   which is the old Core list plus `RenameCoreSchemaToStorage`. Nothing regenerated.
3. `select schema_name from information_schema.schemata` → `storage` present, `core` absent;
   `storage."__EFMigrationsHistory"` carries every prior Core migration row.
4. `grep -rn "Microsoft.AspNetCore" src/modules/*/*/src/*.Domain` → empty; `Shared.Domain.csproj` has zero
   `PackageReference`.
4b. **D14 held:** `git diff --stat <pre-move>..HEAD -- '*.cs'` shows no `using` change in any file whose
   only shared import is a module namespace (the module split is path-only); and
   `grep -rn "using _116.Shared.Application\|using _116.Shared.Infrastructure\|using _116.Shared.Contracts\|using _116.BuildingBlocks.Constants\|using _116.BuildingBlocks.Utils" .`
   → empty (the shared rename is complete, not half-applied).
5. `grep -rn "UserConstants\|FileConstants" src/modules/Content src/modules/Mailer src/shared/src` → empty
   (single-module constants went home), while `LocaleConstants` still resolves from
   `Shared.Domain` for `LocalizationExtension`, Mailer's 4 sites, Content's 1 and Identity's `UserEntity`, and
   `FileUploadLimits` from `Storage.Contracts` for Content's `EditorialValidation` and Identity's `FileValidation`. No `<M>.Domain` appears in
   another module's `ProjectReference` list — `ModuleBoundaryTests` is the gate.
6. No test project references another test project; `<M>.TestData`, `tests/TestData` and `tests/Fixtures` are libraries; `grep -rn "_116.Tests.Fixtures" modules shared tests` finds only the boot library. No unit-test project has Testcontainers in its graph.
7. After the swap: `docker build .` succeeds **without a Dockerfile edit per project** (the glob from 18.11); CI paths green; one `.slnf` per module loads; `dotnet sln list` matches the 34 in item 9.
8. After the swap: `git log --follow` on a sampled moved file shows pre-move history — this is the proof that the
   swap was one commit (D13 rule 1); if it comes back empty, stop before deleting anything.
9. Project count lands at **39** (`git ls-files '*.csproj' | grep -v '\.old/' | wc -l`) and is stated in the PR, not
   discovered in review. `src.old/` and `tests.old/` are present, excluded from the solution, and untouched.

---

**PR:** `refactor(structure): shared foundation split, layer projects, per-module tests and the storage rename`
