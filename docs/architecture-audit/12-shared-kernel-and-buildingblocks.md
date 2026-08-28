# 12 — Shared Kernel vs BuildingBlocks: what each should really be

> **Correction (superseded on two points by
> [module-restructure-study/08](module-restructure-study/08-shared-foundation-structure.md)).** This
> doc uses "Shared Kernel" for the shared **domain base types** (`Aggregate`, `Entity`, `Specification`).
> That is inaccurate: a DDD *Shared Kernel* is domain **model co-owned by ≥2 bounded contexts** (which
> this codebase does not have) — the base types are the tactical **building blocks / SeedWork**, and the
> honest project name is **`Shared.Domain`**, not `Shared.Kernel`. Also, `BuildingBlocks` should be
> **deleted** (its files each have an owner), and its residual web vocabulary belongs in a **`Shared.Web`**
> presentation layer, **not** in any `.Contracts` project. Read [08](module-restructure-study/08-shared-foundation-structure.md)
> for the corrected model; the reasoning below still holds, only the naming is fixed there.

Scope: `src/Shared/Shared`, `src/Shared/Shared.Contracts`, and `src/BuildingBlocks` — the three
"foundation" projects every module depends on. The question: do they make sense under DDD and
Clean Architecture, and what *should* `BuildingBlocks` be compared to `Shared`?

**Verdict:** the current split is not principled. `Shared` is three different things fused into
one project, and `BuildingBlocks` is a *misnamed constants bag* that also holds module-specific
values. The fix falls straight out of the layer-split decision in
[11](11-project-structure-and-packages.md).

---

## What's there today

| Project | Files | Contents | Assessment |
|---|---|---|---|
| `Shared.Contracts` | 7 | CQRS interfaces (`ICommand`, `IDispatcher`, …) | Clean leaf, zero packages. **Correct.** |
| `Shared/Shared` | 87 | `Domain/` (Aggregate, Entity, IDomainEvent) **+** `Application/` (decorators, exceptions, pagination) **+** `Infrastructure/` (BaseModule, interceptors, Carter/EF/Quartz host wiring) | Three layers in one project; drags EF, Carter, Quartz, **Bogus** into every consumer's domain. **Overloaded.** |
| `BuildingBlocks` | 20 | Only constants: rate-limit policies, auth policy names, **and** `UserConstants`/`RoleConstants`/`JwtClaimsConstants`/`SessionConstants` (Identity's) + `FileConstants` (Core's) | A constants bag holding module-specific values under a global-visibility name. **Misnamed + leaky.** |

---

## The DDD/Clean distinction that actually matters

There are **three** separable concerns fused into "Shared" + "BuildingBlocks":

1. **Shared Kernel** *(DDD term)* — the small set of **domain** building blocks every module's
   domain layer agrees to share: `Aggregate<T>`, `Entity<T>`, `IEntity`, `IDomainEvent`,
   `Specification<T>`, and (once promoted) value-object bases like `Money`/`Slug`. Domain-level,
   **zero package dependencies**. Today this is `Shared/Shared/Domain/*` — buried inside the
   kitchen-sink project and therefore reachable only by dragging the whole web stack along.

2. **Shared technical infrastructure** — the CQRS pipeline, decorators, `IUnitOfWork`, pagination,
   the exception model, `BaseModule`, the EF interceptors, and the host wiring. This is
   **framework glue**, not a kernel. It is domain-agnostic but heavily package-dependent.

3. **Global constants** — truly inert, domain-agnostic values every module references: rate-limit
   policy *name strings*, authorization policy *name strings*, `ApiVersionUrl`. No behaviour, no
   type anyone models against.

"Shared" fuses #1 and #2. "BuildingBlocks" is meant to be #3 but has been polluted with
single-module constants.

> **Note on the name "BuildingBlocks".** In the wider Clean Architecture world (e.g. eShop),
> "BuildingBlocks" usually means the *technical seedwork* — base abstractions + cross-cutting
> infrastructure (#1 + #2). Here it means *constants only* (#3). That mismatch is why the name
> feels vague. Pick one meaning and hold it (this doc recommends the constants meaning, renamed).

---

## What each should be

Apply the same layer rule adopted for modules in [11](11-project-structure-and-packages.md) to the
foundation. Target:

```
Shared.Kernel          (#1 — Aggregate<T>, Entity<T>, IDomainEvent, Specification<T>, VO bases; ZERO packages)
Shared.Application     (#2a — CQRS abstractions + decorators + IUnitOfWork + pagination + exception model; FluentValidation, Mapster)
Shared.Infrastructure  (#2b — BaseModule, interceptors, EF/Carter/Quartz/Swagger host wiring)
Shared.Constants       (#3 — the genuinely-global inert constants + ApiVersionUrl; ZERO packages)
```

- **`Shared.Kernel`** is the real DDD Shared Kernel. Every `*.Domain` project references it and
  nothing else. It is the one place `Aggregate`/`Entity`/`IDomainEvent` may live so a module's
  domain can use them without inheriting Carter/EF. Fix `IEntity`/`Entity<T>` here too:
  `Id` becomes `protected set`, the audit fields `internal set`
  ([01 §1.9](01-composition-root-and-shared-kernel.md)), and delete the empty-marker `IRepository<T>`
  (27 interfaces inherit it and gain nothing).
- **`Shared.Application`** absorbs `Shared.Contracts` (the CQRS interfaces) or keeps it as a
  sub-leaf — either is fine; the point is these are *application* abstractions, not a kernel.
- **`Shared.Infrastructure`** is the only foundation project that references Carter/EF/Quartz. Move
  **Bogus** out entirely (it belongs in the test-fixtures project) and pin Mapster to a stable
  release ([01 §1.9](01-composition-root-and-shared-kernel.md)).

**`BuildingBlocks` → `Shared.Constants` (shrunk to what is actually global):**

- **Keep** (genuinely cross-cutting, inert): the rate-limit policy *name* constants,
  `UserRolePolicies`/`AccountStatusPolicies` *name* strings, `ApiVersionUrl`. These are routing
  vocabulary every module's endpoints need and carry no behaviour.
- **Move home** (single-module — [02 §10](02-module-boundaries.md)): `UserConstants`,
  `RoleConstants`, `JwtClaimsConstants`, `SessionConstants`, `PermissionConstants` → Identity's
  `Domain/Constants/` (mark `internal`); `FileConstants` → the storage module's `Domain/Constants/`
  ([13](13-core-storage-and-settings-module.md)).

After this, "BuildingBlocks" is either a small honest constants leaf (renamed `Shared.Constants`)
or it folds into `Shared.Kernel` — its 6 remaining global files don't justify a project of their
own. Either way the vague "BuildingBlocks holds whatever feels shared" pressure is removed.

---

## Answering the question directly

**What should BuildingBlocks be, compared to Shared?**

- **`Shared` should not be one thing.** It is a Shared Kernel *plus* shared application abstractions
  *plus* shared infrastructure — split it into `Shared.Kernel` / `Shared.Application` /
  `Shared.Infrastructure` (+ `Shared.Contracts`), each obeying the dependency rule. `Shared.Kernel`
  is the piece with real DDD meaning; the rest is technical glue.
- **`BuildingBlocks` should be the lowest, most boring leaf**: only the constants and micro-utils
  that are *genuinely global and domain-agnostic* (policy name strings, `ApiVersionUrl`) — no
  behaviour, no module-specific values, nothing anyone models a type against. It is the one project
  every layer of every module may reference precisely because it contains nothing but inert
  vocabulary. Today it fails that test by holding Identity's and Core's constants; move those home
  and it becomes legitimate (and small enough to consider folding into `Shared.Kernel` or renaming
  to `Shared.Constants` for clarity).

In one line: **`Shared.Kernel` = the domain types modules share; `Shared.Infrastructure` = the glue
modules share; `BuildingBlocks`/`Shared.Constants` = the inert strings modules share.** Three
different kinds of "shared", three homes — not two projects split on an arbitrary line.

---

## Rollout

This is part of the [11](11-project-structure-and-packages.md) rollout — step 2 ("split `Shared`")
is exactly this. Order:

1. `Shared.Kernel` first (zero-package leaf); repoint every `*.Domain` at it. Compile errors are
   the domain-layer violations you're hunting.
2. `Shared.Application` + fold/keep `Shared.Contracts`.
3. `Shared.Infrastructure`; move Bogus out, pin Mapster.
4. Shrink `BuildingBlocks` to global constants; move the 5 single-module constant files home
   ([02 §10](02-module-boundaries.md)); rename to `Shared.Constants` (optional but recommended).
5. The architecture test ([02 §3](02-module-boundaries.md)) locks it: `Shared.Kernel` may reference
   no NuGet package; no `*.Domain` may reference `Shared.Infrastructure`.
