# 02 — Decision B: Each Layer as Its Own Project

The proposal: split each module's `Domain` / `Application` / `Infrastructure` (and a new `Presentation`)
from folders inside one `.csproj` into separate projects, so the Clean-Architecture dependency rule is
enforced by the compiler. This takes the solution from 13 projects toward ~30.

**Verdict: not worth it — for any module, including Content.** The single benefit (compile-time boundary
enforcement) is ~90% deliverable *today* by a handful of architecture-test rules at zero structural cost,
while the split demands invasive refactoring, breaks compilation until a latent violation is fixed,
relocates ~586 files, and fragments every one of the 293 vertical slices. This refines the lighter-touch
adoption in [11](../11-project-structure-and-packages.md): after reading the actual code, the layer-as-
project split does not pay for itself here.

---

## Why it doesn't pay off

### 1. The Application layer is not ASP.NET-free today, so the split isn't a relocation

The premise of a clean layer split is that Application depends only on Domain, not on the web framework.
It doesn't hold here: **~44 non-endpoint Application files use `Microsoft.AspNetCore.Http`, and 34 use
`IFormFile` — including the CQRS Commands themselves.** For example
`AdminUploadArtistAvatarCommand` is `record(Guid ArtistId, IFormFile? File)`, and the authorization layer
(`Application/Shared/Authorizations/*`) implements `IAuthorizationRequirement`/`AuthorizationHandler`,
also ASP.NET. Making Application framework-clean therefore requires refactoring ~34 upload slices to a
stream abstraction and relocating the authorization layer — invasive work that changes behaviour risk for
almost no functional gain.

### 2. The Domain already violates the dependency rule — a Domain project won't compile

The decisive fact: **24 Domain files import the Application layer** (Content 17, Identity 5, Core 1,
Mailer 1) — domain entities reach *up* into `Application.Shared.Errors` to throw localized errors (the
error-injection pattern flagged in [03 §6](../03-content-domain.md)) — plus one Domain→ASP.NET leak in
`NewsletterSubscriberEntity`. Under a correct layer split (Domain references nothing), **this code does
not build** until all 24 files are reworked (move error/exception types into Domain or the kernel). This
is the best argument *for* the split — it surfaces a real latent violation — but it is also a hard
prerequisite cost, and the violation can be surfaced *without* the split (see §5).

### 3. ~586 files relocate to a new Presentation project, and Carter must be repointed

To keep Application framework-free, each slice's endpoint + its in-file `Request`/`Response` records +
`MetaField` must move to a `Presentation` project: **~586 files** across the 293 slices. Carter discovers
endpoints by **assembly scanning** (`CarterExtension` scans the assemblies passed in `Program.cs`), so the
registration must be repointed from the module assemblies to the Presentation assemblies, and each
`*Module.cs` composition root must reference Presentation.

### 4. Every vertical slice fragments across two project trees

Today a use case is **one folder**: Command + Handler + Validator + MetaField + `V1/Endpoint` (with the
Request/Response records and the 4-line Request→Command / Result→Response mapping right there). After the
split, each slice fragments across parallel trees in two projects —
`Application/…/VerifyPayment/{Command,Handler,Validator}` and
`Presentation/…/VerifyPayment/{Endpoint,Request,Response,MetaField}`. With 293 slices, that friction is
felt on nearly every feature change. The co-location that makes vertical slices pleasant is exactly what
the layer split trades away — which is why **Content is the *worst* candidate, not the exception**: most
slices, most fragmentation.

### 5. The one real benefit is ~90% available today, for free

The genuine payoff of layer-as-project is compile-time enforcement of the dependency rule. A
`NetArchTest`/`ArchUnit` test delivers the same guarantee — and catches the exact §1 and §2 violations —
with **zero structural churn**, addable today:

```csharp
Types.InAssembly(DomainAssembly).Should().NotHaveDependencyOn("…Application")           // catches the 24
Types.InAssembly(ApplicationAssembly).Should().NotHaveDependencyOn("Microsoft.AspNetCore") // catches IFormFile-in-command
Types.InCurrentDomain().That().ResideInNamespace("…Domain").Should().NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
```

Transitive-package containment (Domain can't accidentally use EF because it doesn't reference Infra) is
also expressible as a rule. Independent layer versioning/reuse is irrelevant in a single-host monolith.

### 6. Project proliferation is pure overhead at the low end

Core is 37 files. Splitting it into Domain/Application/Infrastructure ≈ 9–12 files per assembly, adding
restore nodes, MSBuild graph edges, and cold-load assemblies to enforce a boundary a single test file
already guarantees. Mailer (88), BuildingBlocks (20), and the Contracts projects (3–7 files) are the same
— overhead only.

## Incremental build: the advertised win is mostly illusory

The pitch is "change an Application file without recompiling Domain." But the fan-out runs the other way,
and the numbers don't support it (Content, from the real DAG — Application→Domain pervasive,
Infrastructure→Application one-way):

| You edit… | Recompiles | Win vs today |
|---|---|---|
| a **Domain** file (~7%) | Domain → Application → Infrastructure = **all** | none (dependents recompile) |
| an **Application** file (**84%**) | Application + Infrastructure | ~7% fewer files; still recompiles the giant |
| an **Infrastructure** file (~9%) | Infrastructure only | large — but only 9% of edits |

**84% of edits land in Application, which stays one assembly** — so the common edit (an endpoint or
handler) gets essentially no incremental win, while every build pays 3× the restore/graph/assembly-load
overhead. The layer split optimizes the wrong axis.

## What *would* help incremental builds (if that's the goal)

Split **Application by feature area**, not by layer. Content's Application already subdivides into
`Commerce / Interactions / Editorial / Catalog / Lookup`; making those separate projects lets a Commerce
edit skip recompiling Editorial, and it **matches the vertical slices** instead of shredding them. Same
for Identity. That is a targeted split of the two big modules only (~18–22 projects total), applied *if
and only if* incremental build time on those two is a measured, recurring pain — not a blanket layering.

## Recommendation

- **Do not** split layers into projects. **Add `NetArchTest` rules** (`Domain !→ Application`,
  `Application !→ AspNetCore`, `Domain !→ EntityFrameworkCore`, `Infrastructure` is the only EF consumer)
  to a small `tests/Architecture` project, and **fix the 24 Domain→Application violations** they flag
  (real debt regardless of structure — it's the domain error-injection anti-pattern from
  [03 §6](../03-content-domain.md)).
- **Keep the vertical slice as one folder.**
- **If** big-module incremental build becomes measured pain, split **Content/Identity Application by
  feature area** only.

This is the honest answer to "is there a huge advantage": the enforcement you want costs a few test rules,
not a 13→30 project reorganization.
