# Architecture Audit — 116 Backend

A full architectural review of `apps/backend/src`, read module by module and file by
file, judged against Clean Architecture, Domain-Driven Design, the modular-monolith
model, vertical-slice organisation, and ASP.NET Core production practice.

Every finding names the exact file and line it comes from. Each one states the problem,
why it is a problem (the concrete failure it causes, not a style opinion), and the
complete fix scoped to this codebase.

## How to read this

Start with [`00-executive-summary.md`](00-executive-summary.md) for the cross-cutting
picture and the recommended order of work. Then read the area file for whatever you are
touching.

| # | Area | Scope |
|---|------|-------|
| 00 | [Executive summary](00-executive-summary.md) | Themes, severity roll-up, sequencing |
| 01 | [Composition root & shared kernel](01-composition-root-and-shared-kernel.md) | `Api`, `BuildingBlocks`, `Shared`, CQRS dispatch, DI, rate limiting |
| 02 | [Module boundaries](02-module-boundaries.md) | Project graph, cross-module coupling, the Core leak, boundary enforcement |
| 03 | [Content — domain layer](03-content-domain.md) | Aggregates, invariants, state machines, value objects |
| 04 | [Content — infrastructure & data](04-content-infrastructure.md) | Repositories, specifications, query performance, transactions, jobs, cache |
| 05 | [Core & Mailer modules](05-core-and-mailer.md) | File storage, Cloudinary, SSRF, mail delivery, outbox |
| 06 | [Content — application layer](06-content-application.md) | Vertical slices, handlers, duplication, validation, endpoints |
| 07 | [Identity & security](07-identity-and-security.md) | Auth flow, tokens, sessions, permission enforcement, IDOR |
| 08 | [Cross-cutting concerns](08-cross-cutting.md) | i18n, error contract, logging, API design, config, security posture |
| 09 | [Documentation](09-documentation.md) | Doc-vs-code drift, organisation, what is missing |
| 10 | [Should the Content module be split?](10-content-module-sizing.md) | Why Content stays one module — split by layer, not by feature |
| 11 | [Target project structure & packages](11-project-structure-and-packages.md) | Adopted: one project per layer per module, monolith deploy, Central Package Management |
| 12 | [Shared Kernel vs BuildingBlocks](12-shared-kernel-and-buildingblocks.md) | What each foundation project should really be under DDD/Clean |
| 13 | [Core is Storage; the Settings module](13-core-storage-and-settings-module.md) | Rename Core→Storage; design the intended system-settings + user-preferences module |
| 14 | [Notifications, email & subscriptions](14-notifications-email-and-subscriptions.md) | Notification = concept, email/in-app = channels; mandatory vs preference-gated vs opt-in |
| — | [Module restructure study](module-restructure-study/README.md) | Deep study: per-module src/tests + layers-as-projects — is 13→30 projects worth it? (verdict: no; do CPM + NetArchTest instead) |
| — | [Implementation specs](implementation-specs/00-implementation-plan.md) | The staged, checkbox-driven remediation plan — 16 sequential stages, one PR each, with full code per stage |

## Severity legend

- **Critical** — data loss, security breach, revenue loss, or "cannot run more than one
  instance". Fix before the next production deploy.
- **High** — latent correctness bug, systemic coupling that blocks change, or a hazard
  one code change away from becoming critical.
- **Medium** — real problem with a bounded blast radius; schedule it.
- **Low** — worth fixing when the surrounding code is touched.

## Ground rules for anyone acting on this

1. The audit reports the state of the code as read. Verify a finding still holds before
   you act on it — the tree moves.
2. Findings are cross-referenced. Several fixes share a root cause (the Core boundary,
   the missing domain-event outbox, the missing transaction boundary). Fix the root, not
   each symptom in isolation — the sequencing notes in each file say when.
3. Nothing here is a style complaint. If a finding reads like taste rather than a
   failure mode, it was mis-filed — flag it.
