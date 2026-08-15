# 10 — Should the Content Module Be Split?

Scope: `src/Modules/Content/Content` — the largest module (1,466 files, ~165k lines). The
recurring question is whether its size means it should be broken into smaller, separate
modules (an `Editorial` module, a `Commerce` module, an `Interactions` module, …).

**Answer: no.** Content is big, but it is **one bounded context**, and a horizontal split into
separate modules would break 61 real foreign keys and trade referential integrity for
distributed-consistency problems, for no architectural gain. The size is a real smell — but it
points at three fixable structural faults, none of which is "make more modules".

---

## The evidence: the slices are one mesh, not separable contexts

At the **application layer** the slices look clean — measured, each slice imports another
slice's namespace in exactly 1 file (`Interactions`, `Catalog`, `Commerce`, `Shared` each →
`Editorial` once; `Editorial` and `Lookup` → nothing). So on the surface a split looks easy.

The **domain and database** say the opposite. There are **61 `HasForeignKey` constraints**
across the 49 EF configurations, in a single `content` schema, and many cross the conceptual
groups a split would cut on:

| FK crossing | Where |
|---|---|
| Editorial content → **Commerce** (`Customer`) | `ArticleConfiguration.cs`, `VideoConfiguration.cs`, `LyricsConfiguration.cs` — paid content FKs back to the commissioning customer |
| **Commerce** order item → **Catalog** (`Category`) + **Lookup** (`PromotionLevel`) | `ContentOrderItemConfiguration.cs:32,35` |
| Editorial content → **Catalog** (`Category`) + **Lookup** (`PromotionLevel`, `Tag`) | `ArticleConfiguration.cs`, `VideoConfiguration.cs`, `LyricsConfiguration.cs` |
| ~16 **Interaction** entities (like/comment/share/bookmark/view) → **Editorial** content | every `*Like`/`*Comment`/`*Share`/`*Bookmark`/`*View` config |

These are real in-database foreign keys with `OnDelete` behaviours, not loose references.

This codebase's own module rule — which it holds perfectly at every existing boundary — is
**one schema per module, zero cross-schema foreign keys** ([02](02-module-boundaries.md)).
Splitting Content into `Editorial`/`Commerce`/`Interactions` modules means separate schemas,
which means converting a large share of those 61 FKs into FK-free `Guid` references plus
eventual consistency. That is a strict downgrade in integrity — in a domain where an Article
being *liked*, *commissioned*, *categorized*, *tagged*, and *promoted* are facets of one
lifecycle, not five independent things.

**Editorial / Commerce / Interactions / Catalog / Lookup are aspects of one bounded context
(Publishing), not separate contexts that happen to co-locate.** A horizontal split is the wrong
move.

---

## What the size actually indicates — and the fix

The 165k lines / 1,466 files is pointing at three faults, all fixable without moving a feature
into a new module.

### A. The layering is not enforced (do this split instead)

Content is a **single `Content.csproj`** with Domain/Application/Infrastructure as *folders*, so
the dependency rule exists only by convention. It is already violated: 74 domain-entity
signatures take an Application-layer i18n factory ([03 §6](03-content-domain.md)), and the query
builders in `Application/*/Builders/` import `ContentDbContext` ([06 §14](06-content-application.md)).

**Fix — the split that is actually worth doing:** break `Content.csproj` into
`Content.Domain` / `Content.Application` / `Content.Infrastructure`, with references pointing
inward only. This keeps it **one module, one schema**, but makes the dependency rule a compile
error — the offending files stop compiling, which is how you find and fix them. It shrinks the
*perceived* size far more than a feature split would, because most of the cognitive load is the
absent layer boundary, not the line count.

> This layer-into-projects split has been **adopted as the standard for every module**, not just
> Content, with Central Package Management so ~20 projects can't drift on versions. Content is
> where it pays off most (the leaks are real and numerous here), but the target structure, the
> reference graph, and the package strategy are in
> [11 — Target Project Structure & Package Management](11-project-structure-and-packages.md).
> This whole document is still about the *feature* split (which is rejected); the *layer* split is
> the adopted fix.

### B. Everything is an aggregate root

49 entities are all `Aggregate<Guid>` — 49 `DbSet`s, 49 event-scanned classes — where there
should be ~15–20 real aggregates with child entities ([03 §1](03-content-domain.md)). Roughly
half are junction/child rows (`ArticleTagEntity`, `ContentOrderItemEntity`, the vote entities)
that inflate the count and the mental model. Demoting them to `Entity<Guid>` children cuts the
surface substantially — again, without moving anything between modules.

### C. `Application/Shared` is a god-folder every slice binds to

139 files that every slice depends on — Editorial 279 files import it, Interactions 89, Catalog
93, Lookup 82, Commerce 61 — fronted by the 22-dependency `ContentI18n` facade
([06 §9](06-content-application.md)). **This is the real coupling hotspot.** Split it into
per-slice shared kernels (`EditorialI18n`, `CommerceI18n`, …) and the slices become genuinely
independent *within* the one module — which is the isolation a module split was reaching for,
achieved without the schema fragmentation.

---

## When a split *would* make sense — and its precondition

The one sub-area that is a legitimately separate bounded context in DDD terms is **Commerce**
(orders, payments, packages, pricing tiers). It is about money and fulfilment, not content, and
it has its own natural language. If the platform grows to where commerce needs its own team or
release cadence, that is the seam to cut — **and the only one worth considering.**

But it is **not separable today.** `ContentOrderItemEntity` FKs `Category` (Catalog) and
`PromotionLevel` (Lookup), and Editorial content FKs `Customer` (Commerce). The realistic path:

1. First do faults A, B, C above (layer projects, real aggregates, per-slice kernels).
2. Convert Commerce's cross-group FKs (`OrderItem → Category/PromotionLevel`, content →
   `Customer`) to FK-free `Guid` references + integration events — the same discipline the
   codebase already uses at every module boundary.
3. Only then extract `Commerce` into its own module with its own schema.

Extract Commerce first, extract it last, and only after the coupling is severed — not as a
starting point.

---

## Verdict

- **Do not** carve Content into feature modules — it is one bounded context, and the split
  breaks 61 FKs for no gain.
- **Do** split it by *layer* into three projects so the dependency rule is compile-enforced.
- **Do** collapse the 49 aggregate roots into ~15–20 real aggregates with children.
- **Do** dissolve `Application/Shared` into per-slice kernels.

Same module, same schema, a fraction of the perceived size, and the boundaries you actually want
become enforced by the compiler. The top-level solution is otherwise sound; the two structural
defects worth more attention than resizing Content are the **Core contracts leak**
([02 §1](02-module-boundaries.md)) and the **absence of any boundary enforcement**
([02 §3](02-module-boundaries.md)).
