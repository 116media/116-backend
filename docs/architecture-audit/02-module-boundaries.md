# 02 — Module Boundaries

Scope: the project-reference graph, every cross-module `using`, the contracts-project
pattern, and whether the "modular monolith" claim holds.

The verdict: the database boundary is airtight (four schemas, zero cross-schema foreign
keys, zero foreign `DbContext` access), and the `.Contracts` pattern is correct where it
is applied (Identity, Mailer). But **Core has no contracts project**, and Content +
Identity project-reference the whole Core implementation and consume its domain aggregate
directly across 115 files. Nothing enforces any boundary — there are 2,938 public types
and 9 `internal` declarations in the whole of `src`.

## Project-reference graph (as declared)

```
Api ──► Identity, Content, Mailer          (Core reached only transitively — a latent bug, §2.4)

Content  ─► BuildingBlocks, Shared, Shared.Contracts, Mailer.Contracts, Identity.Contracts, ██ Core (full module — LEAK)
Identity ─► BuildingBlocks, Shared, Shared.Contracts, Mailer.Contracts, Identity.Contracts, ██ Core (full module — LEAK)
Mailer   ─► BuildingBlocks, Shared, Shared.Contracts, Mailer.Contracts, Identity.Contracts
Core     ─► Shared, Shared.Contracts, BuildingBlocks        (no module deps — Core is clean; the leak is entirely inbound)

Identity.Contracts (3 files) · Mailer.Contracts (6 files) · Core.Contracts — DOES NOT EXIST (orphaned obj/ only)
```

## Cross-module type usage (grep counts)

| Consumer | Foreign type | Owner | Occurrences | Files | Via contract? |
|---|---|---|---|---|---|
| Content | `IFileRepository` | Core | 134 | 97 | **No** |
| Content | `FileEntity` | Core | 100 | 44 | **No** |
| Content | `ICloudinaryService` | Core | 3 | 3 | **No** |
| Content | `SlugHelper` | Core | 2 | 2 | **No** |
| Identity | `FileEntity` | Core | 16 | 13 | **No** |
| Identity | `IFileRepository` | Core | 12 | 12 | **No** |
| Content | `IClaimsProvider` / `IUserLookupService` / `AuthorInfo` | Identity.Contracts | 127 | — | yes |
| Identity/Content | Mailer contract types | Mailer.Contracts | 119 | — | yes |

---

## 2.1 Core has no contracts project; 115 files bind to its domain aggregate directly

**Severity: Critical** · this is the root cause of §2.4, §2.6, §2.8, §2.9 and much of
[05](05-core-and-mailer.md).

**Where:** `Content.csproj:21` and `Identity.csproj:21` both
`ProjectReference Include="../../Core/Core/Core.csproj"` (the implementation assembly).
`src/Modules/Core/Core.Contracts/` exists only as build droppings — no `.csproj`, not in
the solution. Consumers hold Core's aggregate: `FileEntity` is
`Aggregate<Guid>` with `Delete()`/`MarkReplaced()` raising Core domain events, referenced
in 44 Content + 13 Identity files.

**Problem/why.** Any change to Core's aggregate — a renamed property, a new required
`Create` parameter, a changed event payload — breaks compilation across 101 Content and 14
Identity files. Extraction to a service is impossible because `IFileRepository` returns a
*tracked EF aggregate*, not data. A previous extraction attempt was reverted, almost
certainly because it tried to move the whole surface (including `FileEntity`, which cannot
live in a leaf contracts assembly).

**Solution — scoped down and sequenced so each step ships green.**
1. Create `Core.Contracts.csproj` (leaf, mirrors `Mailer.Contracts`). Add to the solution;
   delete the stale `obj/`.
2. Add `FileRef` (a plain record: `Id`, `StorageUrl`, `StorageKey`, `MimeType`, colour
   hexes) and move `FileDto` in as-is.
3. Add `IFileStore` covering only the ~9 operations foreign modules actually call, all
   returning `FileRef`, never `FileEntity`.
4. Implement `FileStore : IFileStore` in Core over `FileRepository`, projecting to
   `FileRef`. Register in `CoreModule`.
5. **Add the `Api → Core` reference first** ([§2.4](#24)) — it de-risks everything.
6. Migrate **Identity first** (14 files), then Content in 4 slices by vertical area
   (Commerce → Catalog/Lookup → Interactions → Editorial/Mappers). `IFileStore` and
   `IFileRepository` coexist during migration, so each slice compiles independently.
7. Swap `Content.csproj`/`Identity.csproj` to `Core.Contracts`; make `IFileRepository`
   internal ([§2.7](#27)).

**Blast radius:** ~115 files, 157 usings, 2 `.csproj`, 1 new project, 1 solution edit.

---

## 2.2 `docs/ARCHITECTURE.md` claims cross-module communication is by domain events — zero such handlers exist

**Severity: Critical** (as a documentation-vs-reality gap that drives wrong decisions)

**Where:** `docs/ARCHITECTURE.md:80,138,157`. Reality: every one of the 32
`IDomainEventHandler<T>` implementations lives in the same module as its event (Content→
Content 23, Identity→Identity 8, Core→Core 1). `grep IntegrationEvent src` → 0. 100% of
cross-module traffic is synchronous in-process interface calls.

**Problem/why.** The document is the onboarding contract and the basis for "extract a
microservice later" — a claim currently false and unenforced. It also claims an "outbox
pattern already in place" (`:157`); the only outbox is Mailer's *email-delivery* outbox,
not a module bus. A new contributor believes Content is decoupled from Core, then finds
`IFileRepository` injected into 97 files.

**Solution.** Do not invent an event bus — the direct-call design is defensible for an
in-process monolith. Fix the document to state the real, intended rule ("modules
communicate only through the target's `.Contracts` project; domain events are intra-module
post-commit reactions"), correct the module list (Content is not "planned"; Mailer
exists), and delete the false outbox claim. Then make the rule true by construction with
[§2.3](#23). See [09 §5](09-documentation.md).

---

## 2.3 Nothing enforces module boundaries — no architecture tests, no analyzer, no banned-namespace rule

**Severity: Critical**

**Where:** `grep NetArchTest|ArchUnit tests` → 0. No `tests/Architecture/`. No root
`Directory.Build.props`. The only thing stopping Content from touching `IdentityDbContext`
is that `Content.csproj` references `Identity.Contracts` not `Identity` — but it *does*
reference `Core.csproj`, so `CoreDbContext` is reachable and only convention forbids it.

**Problem/why.** Every boundary is a social agreement. The Core leak reached 115 files
precisely because nothing failed when the first file added `using _116.Core.Domain.Entities`.
Once [§2.1](#21) is fixed, nothing prevents regression.

**Solution.** Add `tests/Architecture/` with `NetArchTest.Rules` and these executable
rules: (1) no module references another module's implementation assembly (contracts are
the allowlist) — land after §2.1, or now with a documented skip listing the 115 files as
the burndown; (2) contracts projects are leaves; (3) contracts contain only interfaces/
records/enums; (4) no foreign `DbContext` access (passes today — lock it in); (5) event
handlers live with their event (passes 32/32 — lock it in); (6) `*.Domain.*` has no EF
Core dependency (validates the dependency rule `ARCHITECTURE.md:27` states but never
checks). Wire into CI.

---

## 2.4 `Api.csproj` does not reference Core, yet `Program.cs` calls `AddCoreModule`

**Severity: High** · do this **first** in the §2.1 sequence.

**Where:** `Api.csproj:13` references Identity/Content/Mailer only; `Program.cs:22,79,114`
use `CoreModule`, `AddCoreModule`, `UseCoreModule`. It compiles only because Identity/
Content transitively expose `Core.dll`.

**Problem/why.** The moment Identity/Content switch to `Core.Contracts`, `Api` loses its
transitive path to `Core.dll` and `Program.cs` fails to compile — including the only call
that registers `CoreDbContext` and runs the `core` migration. A partial migration then
looks like "Core.Contracts broke everything", which is plausibly what sank the earlier
attempt.

**Solution.** Add `ProjectReference Include="../Modules/Core/Core/Core.csproj"` to
`Api.csproj` as the first commit. The composition root legitimately depends on every
module implementation — that is what a monolith host is for. One line, independently
correct today.

---

## 2.5 Content/Identity write to `core.files` and their own schema in two uncoordinated transactions

**Severity: High** · overlaps [04 §7](04-content-infrastructure.md), [05 §4](05-core-and-mailer.md).

**Where:** each module owns a bare-`SaveChanges` UoW with no ambient transaction;
`FileRepository` self-commits (`FileRepository.cs:281` etc., 7 sites). Visible at
`AdminAttachPaymentProofHandler.cs:51-63`: `UploadAndStoreRawFileAsync` commits
`core.files`, then a separate `unitOfWork.CommitAsync` commits `content.content_payments`.
Same in all 6 Content upload handlers.

**Problem/why.** A failure between the two commits leaves a committed `core.files` row and
a paid Cloudinary asset that no content row references — a permanent orphan
`ContentAssetCleanupHandler` never sees (it only reacts to committed Content deletion
events). The replace flows are worse: the old asset is purged before the new content row
commits, so a failure leaves live content pointing at a dead URL.

**Solution.** Do not reach for distributed transactions — that couples the schemas. Make
the orphan reapable, matching the content-asset design: add a `ConfirmedAt` to `FileEntity`
(null on create), a `ConfirmAsync` on `IFileStore` called after the consumer's commit, and
an `OrphanFileCleanupJob` in Core that soft-deletes unconfirmed rows older than 1 hour
(routing them through the existing purge path). Invert the replace flows to
upload-new-unconfirmed → commit consumer → confirm → retire-old.

**Blast radius:** `FileEntity` + 1 migration, `FileRepository` (2 methods), 1 job, 14 call
sites (10 Content, 4 Identity).

---

## 2.6 `IFileRepository` is a 17-method god-interface exposing Core's persistence, cloud provider, and avatar business logic

**Severity: High**

**Where:** `IFileRepository.cs` — persistence primitives (`AddAsync`, `Remove`,
`SaveChangesAsync`), cloud-upload orchestration, **and Identity's avatar rules**
(`GetAvatarFileAsync`, `UpdateAvatarUrlFromSourceAsync(..., string userId, bool
isAvatarSourceManual, ...)` — `userId` and `isAvatarSourceManual` are Identity concepts).
`:3` is `using Microsoft.AspNetCore.Http` in a type named `…Application.Shared.Repositories`.

**Problem/why.** Every foreign consumer gets `SaveChangesAsync` on Core's context and can
`Remove` an aggregate, bypassing soft-delete + cleanup. Core cannot change its avatar rules
without an Identity review, nor Identity without touching Core.

**Solution.** During §2.1: `IFileStore` gets the ~9 storage-neutral operations; the CRUD
primitives stay on an `internal IFileRepository`. Move the six avatar methods into an
Identity `IAvatarService` over `IFileStore` — the `isAvatarSourceManual` and "same URL,
skip" logic belongs next to `UserEntity`. Keep `IFormFile` on `IFileStore` (the honest
upload signature) and note the ASP.NET dependency explicitly. See
[05 §5](05-core-and-mailer.md).

---

## 2.7 Effectively nothing is `internal` — 2,938 public types, 9 `internal` declarations

**Severity: High**

**Where:** public types: Content 2058, Identity 738, Mailer 103, Core 39. `internal`
declarations: 9 total. Core exposes 39 public types including `CoreDbContext`,
`FileRepository`, `CloudinaryService`, every specification. `Core.csproj` has no
`InternalsVisibleTo`.

**Problem/why.** `public` declares a stable contract. When 100% of a module's types are
public, the contracts pattern is decorative — `Identity.Contracts` exposes 3 types while
`Identity.dll` exposes 738, and only a `.csproj` line stops a consumer using them. This is
how the Core leak spread.

**Solution.** Do not bulk-sweep 2,938 types. Scope it to Core (small, being extracted):
add `InternalsVisibleTo` for the test assembly, then after §2.1 mark `internal` the ~21
Core types with no legitimate foreign caller (`FileEntity`, `IFileRepository`,
`CoreDbContext`, `CloudinaryService`, specifications, etc.). Move `SlugHelper` to `Shared`.
For the other modules, rely on §2.3's assembly-level rule and apply `internal`
opportunistically to new types.

---

## 2.8 Content's mapping layer depends on Core's repository, producing cross-module N+1 queries

**Severity: High** · overlaps [04 §13](04-content-infrastructure.md).

**Where:** 37 `IFileRepository` occurrences across 8 Content mapper files. N+1 is explicit
at `PublicGetArtistsHandler.cs:37-48` — one `GetByIdAsync` per row despite a batched
`GetByIdsAsync` existing. Compounded by `AuthorInfo.AvatarFileId` (Identity.Contracts)
forcing a *second* cross-module hop per author.

**Problem/why.** A 50-item artist page issues 1 content query + 50 `core.files` round-trips
on a separate connection/pool — exhausting the shared Npgsql pool under load. Content's
mapping layer being a boundary-crossing point is why the Core leak is spread across 44
files rather than a few gateways.

**Solution.** Change `AuthorInfo` to carry the resolved `AvatarUrl` (Identity batch-resolves
it in the same call). Add a single Content `IFileUrlResolver` gateway returning a
pre-resolved `{Guid → url}` dictionary; change all 8 mappers to accept the dictionary so a
mapper physically cannot query. Do this inside §2.1's Editorial slice, not as a separate
pass.

---

## 2.9 `FileEntity → FileDto` mapping is duplicated verbatim in Content and Identity

**Severity: Medium**

**Where:** identical `ToFileDto` extension in `UserMapper.cs:58` (Identity) and
`ContentOrderMapper.cs:168` (Content) — two modules mapping a third module's aggregate to
its DTO. 14 call sites; two per-module Mapster configs must independently know a Core type.

**Problem/why.** Core owns both types but neither mapping; a field added to `FileDto`
requires finding and editing two modules and can silently diverge (Mapster convention
mapping fails at runtime, not compile time).

**Solution.** Both methods evaporate under §2.1 — once `IFileStore` returns `FileRef`
directly there is nothing to map. Delete both in the same commit as each module's
`IFileStore` swap.

---

## 2.10 `BuildingBlocks` holds single-module constants in a universally-referenced leaf

**Severity: Medium**

**Where:** `UserConstants`, `RoleConstants`, `PermissionConstants`, `JwtClaimsConstants`,
`SessionConstants` are Identity-only but live in a project every module compiles against.
`FileConstants` holds Core's EF column widths (`FileEntity.cs` binds `MaxLength` to them).

**Problem/why.** Single-module types in a global-visibility project train contributors to
put the next module's internals there too. A change to `MaxStorageUrlLength` (a Core
migration) recompiles all four modules.

**Solution.** Move the five Identity constant files into `Identity/Domain/Constants/`
(pure move, ~26 usings within Identity), mark `internal`. Move `FileConstants` into Core
after §2.1 and expose only the values validators need via `Core.Contracts`. Keep
`UserRolePolicies` in BuildingBlocks — inert cross-cutting routing strings are genuinely
shared; document that BuildingBlocks holds only those.

---

## 2.11 The email-template enum couples three modules' release cycles to Mailer

**Severity: Medium** · genuinely optional; do only if template churn causes merge friction.

**Where:** `EnumEmailTemplate.cs` — 27 members spanning Identity (13), Content (12), Mailer
(2), plus per-member copy in six Mailer `.resx` files. Adding one Content email edits a
leaf assembly every module compiles against + six `.resx` + the handler.

**Problem/why.** A well-understood trade (central, localizable copy) with a coupled-cadence
cost. The `IReadOnlyDictionary<string,string> tokens` contract is also stringly-typed — a
template's required tokens are declared nowhere the owning module's compiler sees.

**Solution.** Keep the central registry; split the enum by owner
(`EnumIdentityEmailTemplate`, `EnumContentEmailTemplate`, `EnumMailerEmailTemplate`) and
add per-template token records so token sets are compile-checked. Migrate the 44 call sites
incrementally, keeping the dictionary overload until last. After §2.1.

---

## 2.12 Core is a library, not a module — zero endpoints, absent from the host, scanned for nothing

**Severity: Medium**

**Where:** Carter endpoint counts — Content 221, Identity 64, Mailer 8, **Core 0**. Yet
`Program.cs:27` passes `coreAssembly` to `AddCarterWithAssemblies`/`AddCqrsWithAssemblies`,
which find no `ICarterModule` or handlers in it.

**Problem/why.** Core is structurally a shared kernel wearing a module's clothes, and that
mislabeling is the root of §2.1: contributors treat it as peer-to-peer with Content and
reach into it like `Shared`. `ARCHITECTURE.md:74` reinforces the confusion.

**Solution.** Do **not** relocate Core to `Shared/` — that legitimizes the direct access.
Commit to it being a real module: after §2.1/§2.4/§2.6/§2.7 it exposes only `CoreModule`,
`CoreConstants`, `Core.Contracts` — the correct shape for a module with no HTTP surface.
Remove `coreAssembly` from the Carter scan. Document the module list distinguishing
HTTP-surface modules from Core.

---

## 2.13 Post-commit domain events swallow every handler failure

**Severity: Medium** · same finding as [01 §1.7](01-composition-root-and-shared-kernel.md).

**Where:** `DispatchDomainEventsInterceptor.cs:172` and `DomainEventPublisher.cs:79` — per-
handler `catch (Exception) { LogError }`; the buffer is an in-memory `ConditionalWeakTable`.
The swallowed handlers do real cross-module work (file cleanup into Core/Cloudinary,
invoices/receipts into Mailer). Note `IDomainEvent.EventId` is `=> Guid.NewGuid()` — a new
value per read, useless as an idempotency key.

**Solution.** See [01 §1.7 + §1.8](01-composition-root-and-shared-kernel.md): fix
`IDomainEvent` identity first (a 2-line, high-value change), then add a per-module domain-
event outbox written inside the same `SaveChanges`, replayed by a scheduled job. Audit the
9 email handlers for idempotency first.

---

## What is done well here

- **DbContext isolation is airtight** — each context appears only in its owning module;
  four schemas, `HasDefaultSchema`, separate migration histories. The hardest boundary to
  hold, held perfectly.
- **Zero cross-schema foreign keys** — Content stores `UserId`/`FileId` as plain `Guid`
  with no FK. Exactly right for a modular monolith; it is what keeps the extraction story
  alive at the database layer.
- **The `.Contracts` pattern, where applied, is well-executed** — `Identity.Contracts` (3
  files) and `Mailer.Contracts` (6) are true leaves with the boundary intent documented at
  the seam. `IMailer` documents its transactional contract precisely. Copy this for
  `Core.Contracts`.
- **Core has no outbound module dependencies** — the leak is entirely inbound, so there is
  no cycle to break, only a surface to narrow. That makes §2.1 tractable.
- **Every domain-event handler lives with its event** (32/32). Codify the rule (§2.3),
  don't change it.
- **Module registration is uniform** — one `BaseModule` shape, idempotent interceptor
  registration, boot-time failure on an unknown mail provider. Adding a fifth module is
  mechanical.
