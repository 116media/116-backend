# 00 — Executive Summary

This audit read `apps/backend/src` module by module and file by file, and every markdown file
under `docs/`, judged against Clean Architecture, DDD, the modular-monolith model, vertical-slice
organisation, and ASP.NET Core production practice. It produced **~100 findings** across nine
area files, each with `file:line` evidence and a complete, codebase-specific fix.

The headline: **this is a well-structured codebase with a small number of deep, systemic faults.**
The conventions are consistent, the domain-event pipeline is genuinely well-engineered, the
contracts pattern is correct where applied, and the vertical slices hold. But the same few root
causes surface as dozens of symptoms — and several of them are exploitable or corrupt data today.

---

## The picture in one page

Nine problems account for the large majority of the findings. Fix these roots and most of the
symptom-level findings resolve with them.

### 1. The application cannot safely run more than one instance

The in-memory cache, the lock-free background jobs, the check-then-act seeders, the startup
migrations, and the in-process rate limiters all silently assume a single process. Deploy a second
pod and you get stale caches served per-pod, background jobs running twice (double-purging Cloudinary
assets), seeders that crash-loop on a unique-constraint race, and destructive migrations run against a
database old pods are still serving. → [04 §8–§10](04-content-infrastructure.md), [01 §1.12–§1.13](01-composition-root-and-shared-kernel.md), [07 A3](07-identity-and-security.md).

### 2. The security boundary has holes that are exploitable without authentication

`social-login` issues a valid token pair for any email with no proof of possession — **unauthenticated
account takeover** of every social account. The same endpoint's avatar-URL download is an
**unauthenticated SSRF** into the internal network. Sign-out revokes the session row but the access
token keeps working for up to 60 minutes on 232 of 238 endpoints. A designed 28-permission system is
enforced by **zero** endpoints. Rate limits are one global bucket per policy — a single attacker
disables login for everyone with 5 requests. And the exception fallback returns raw
`DbUpdateException`/Npgsql text to anonymous callers, a working account-enumeration oracle. →
[07 S1–S12](07-identity-and-security.md), [05 §1](05-core-and-mailer.md), [08 §3](08-cross-cutting.md).

### 3. Credentials are written to the logs in plaintext

`LoggingDecorator` serializes every command at Information level with no masking, so every password,
OTP code, and refresh token lands in Console and Seq. This is a reportable breach and requires
rotation + log purge, not just a code change. → [01 §1.2](01-composition-root-and-shared-kernel.md) /
[08 §2](08-cross-cutting.md).

### 4. The "Core" module boundary does not exist

Core has no contracts project; Content and Identity project-reference the whole Core implementation
and consume its EF aggregate directly across **116 files**. Core has also absorbed avatar, thumbnail,
colour, and slug concerns it has no business owning. Nothing enforces any module boundary — 2,938
public types, 9 `internal` declarations, no architecture tests. `ARCHITECTURE.md`'s claim that modules
communicate only via domain events is false (0 cross-module handlers exist). → [02](02-module-boundaries.md),
[05 §5–§6](05-core-and-mailer.md).

### 5. There is no transaction boundary and no domain-event durability

Multi-step writes commit in stages with no rollback — a failed validation can leave the site with
**zero** exclusive categories, and an order can be marked paid with two of three promotions applied.
Domain events are in-process and best-effort: a handler failure is logged and dropped, so a paid order
can silently never send a receipt. File uploads commit to Cloudinary before the DB row, orphaning
assets with no reconciliation. The fix — a transactional outbox — already exists in Mailer to port. →
[04 §7](04-content-infrastructure.md), [01 §1.7–§1.8](01-composition-root-and-shared-kernel.md),
[03 §4](03-content-domain.md), [05 §3–§4](05-core-and-mailer.md).

> **"Should we split the Content module?"** No — it is 165k lines but **one bounded context**,
> and a feature split would break 61 in-schema foreign keys for no gain. The size points at a
> *layer* split (`Content.Domain`/`.Application`/`.Infrastructure`), collapsing the 49 aggregate
> roots into real aggregates, and dissolving `Application/Shared` — not more modules. Full
> reasoning in [10 — Should the Content module be split?](10-content-module-sizing.md).
>
> **Adopted decision:** the layer-into-projects split is the standard for **every** module (still
> one monolith deploy), with Central Package Management so ~20 projects can't drift on versions —
> target structure and package strategy in
> [11 — Target Project Structure & Packages](11-project-structure-and-packages.md).
>
> **Foundation & naming:** `Shared` and `BuildingBlocks` are not a principled split — see
> [12](12-shared-kernel-and-buildingblocks.md) for what a Shared Kernel vs a constants leaf should
> be. And **"Core" is really a Storage module misnamed** — renaming it frees the space for the
> intended system-settings + user-preferences module (which also fixes the missing per-user
> language and notification opt-outs): [13](13-core-storage-and-settings-module.md).
>
> **Notifications vs email:** `Mailer` is named after a transport but holds a concept + a second
> transport + subscriptions, and models the same event twice (6 members overlap between its two
> enums). The fix is four explicit axes — the **event**; the **channel** (email/in-app/push); the
> **message class** (transactional/mandatory · operational/staff · preference-gated · opt-in) so OTP
> stays undeniable, the admin content-review feed is in-app and un-silenceable, and newsletter stays
> opt-in; and the **audience** (a user · all users/broadcast · admins by role · opt-in list) so "new
> episode published" broadcasts to everyone while the editorial work queue targets staff —
> [14](14-notifications-email-and-subscriptions.md).

### 6. The domain is not actually isolated

Every persisted class is an aggregate root (no child entities, no consistency boundaries), which is why
the order total can go stale. The editorial state machines are unguarded — a paid article can be
published without payment, and un-publishing raises no event so caches keep serving it. And the domain
takes a localization service as a method parameter (74 signatures), so business rules can't run in a
background job and new guards are expensive to add — which is exactly why so many invariants leaked
into handlers. → [03](03-content-domain.md), [08 §9](08-cross-cutting.md).

### 7. The public API is a DoS and data-leak surface

`pageSize` is unbounded on ~100 endpoints (`?pageSize=1000000` materializes a whole table,
anonymously). The `CancellationToken` is dropped by all 293 endpoints, so aborted requests run to
completion and exhaust the connection pool. 27 public endpoints return admin DTOs, leaking
`RejectionReason`, the commissioning `CustomerName`, and staff UUIDs to anonymous readers; content
mappers leak staff email addresses next to their role. → [06 §2/§6](06-content-application.md),
[08 §6](08-cross-cutting.md), [07 S9](07-identity-and-security.md).

### 8. Query performance degrades under load in predictable, fixable ways

The like path runs the heaviest article query twice, each a cartesian product with no `AsSplitQuery`.
Engagement counters are lost-update races. Reads track by default (7 of ~154 use `AsNoTracking`). The
homepage promotion query has no usable index. Mappers do one file lookup per row (N+1) despite a
correct batch implementation sitting next door. → [04](04-content-infrastructure.md),
[06 §4](06-content-application.md).

### 9. The documentation is actively misleading

The canonical guide is gitignored and untracked. Its structure tree omits 73% of the codebase, its
migration commands fail, its endpoint template doesn't compile, and no doc in the repo can boot the
app. Shipped feature specs are presented as pending work with code snippets — written to be *executed
by an agent* — that no longer match real signatures, one of which silently writes a GUID into a slug
column. → [09](09-documentation.md).

---

## Severity roll-up

| Area | Critical | High | Medium | Low |
|------|:---:|:---:|:---:|:---:|
| [01 Composition root & shared kernel](01-composition-root-and-shared-kernel.md) | 2 | 6 | 6 | 1 |
| [02 Module boundaries](02-module-boundaries.md) | 3 | 6 | 4 | — |
| [03 Content domain](03-content-domain.md) | 3 | 4 | 5 | 1 |
| [04 Content infrastructure](04-content-infrastructure.md) | 2 | 6 | 8 | — |
| [05 Core & Mailer](05-core-and-mailer.md) | 1 | 6 | 7 | — |
| [06 Content application](06-content-application.md) | 2 | 6 | 7 | 1 |
| [07 Identity & security](07-identity-and-security.md) | 1 | 6 | 8 | 2 |
| [08 Cross-cutting](08-cross-cutting.md) | 3 | 6 | 9 | 2 |
| [09 Documentation](09-documentation.md) | 5 | 5 | 2 | 2 |

Many criticals are the same root cause seen from different angles (the global rate limiter appears in
01/07/08; the Core leak in 02/05; the SSRF/takeover pair in 05/07). The dependency-aware counts, not
the raw sums, are what the sequencing below reflects.

---

## Recommended order of work

Sequenced by exploitability and by dependency (several fixes are prerequisites for others).

**Now — security incidents and data corruption already live:**
1. Stop credential logging; rotate secrets; purge logs. ([01 §1.2](01-composition-root-and-shared-kernel.md))
2. Fix `social-login` (verify the provider token) and the SSRF guard. ([07 S1](07-identity-and-security.md), [05 §1](05-core-and-mailer.md))
3. Sanitize the exception fallback (no raw messages to clients). ([08 §3](08-cross-cutting.md))
4. Clamp `pageSize` inside `PaginatedRequest`. ([06 §2](06-content-application.md))
5. Guard the editorial state machines and the order-total recalculation. ([03 §3–§4](03-content-domain.md))
6. Make engagement counters atomic (`ExecuteUpdateAsync`). ([04 §1](04-content-infrastructure.md))

**Next — correctness and the ability to scale:**
7. Fix forwarded-proxy trust, then partition the rate limiters. ([08 §20](08-cross-cutting.md) → [01 §1.1](01-composition-root-and-shared-kernel.md))
8. Session revocation on the auth pipeline; stop issuing tokens to unverified signups. ([07 S2/S8](07-identity-and-security.md))
9. Distributed cache + clustered jobs + advisory-locked, out-of-pipeline seeding/migration. ([04 §8–§10](04-content-infrastructure.md))
10. Thread `CancellationToken` through all endpoints. ([06 §1](06-content-application.md))
11. Config as validated `IOptions` with `ValidateOnStart`; health checks. ([08 §10/§12](08-cross-cutting.md))
12. Split public vs admin DTOs; stop leaking staff data. ([06 §6](06-content-application.md), [07 S9](07-identity-and-security.md))

**Then — the structural refactors (large, sequence carefully):**
13. `IDomainEvent` identity fix → domain-event outbox → transaction boundary. ([01 §1.8/§1.7](01-composition-root-and-shared-kernel.md), [04 §7](04-content-infrastructure.md))
14. `Core.Contracts` extraction (4 PRs) + architecture tests + `internal`-ization. ([02 §1/§3/§4](02-module-boundaries.md))
15. Invert the localization-in-domain (`DomainException` + strategy). ([03 §6](03-content-domain.md))
16. Split `Shared` and `Content` along the dependency rule; demote child entities. ([01 §1.9](01-composition-root-and-shared-kernel.md), [03 §1](03-content-domain.md), [06 §14](06-content-application.md))

**Continuously:**
17. Restructure `docs/` (entry point, status front-matter, archive shipped specs, banner the executable
    stale specs). ([09](09-documentation.md))

---

## What is genuinely strong (do not regress these)

- **The domain-event dispatch interceptor** — collects pre-commit, dispatches post-commit in a fresh DI
  scope, discards on failure, documents its own caveats. The missing piece is durability, not design.
- **The Mailer outbox** — a real transient/permanent retry split, `FOR UPDATE SKIP LOCKED`, self-
  contained rows. The pattern the rest of the system needs, already in the repo.
- **The `TimeProvider` seam and the `AuditableEntityInterceptor`** — a testable clock injected
  everywhere, one timestamp per save, correct actor attribution.
- **No client-side query evaluation anywhere** — every specification, including correlated subqueries,
  translates to SQL.
- **The contracts pattern where applied** (`Identity.Contracts`, `Mailer.Contracts`, `Shared.Contracts`)
  — true leaf assemblies with the boundary intent documented at the seam.
- **`ArtistEntity`, `OrderPaidEffectsHandler`, and the refresh-token flow** — three pieces of genuinely
  careful, well-reasoned domain and security work. `ArtistEntity` is the model the other aggregates
  should follow.
- **Discipline that held:** pure validators, handlers that never touch `DbContext` or each other,
  near-zero cross-slice coupling, complete rate-limit and authorization *coverage* (the problems are
  *which* policy, not *whether*), controlled mass assignment, parameterized SQL, and no swallowed
  exceptions.

The faults are concentrated and fixable. None of them require rearchitecting — they require closing
boundaries the codebase already knows how to draw, and adding durability the codebase already
demonstrates in Mailer.
