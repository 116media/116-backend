# Domain Events — Implementation Specs

Read [../00-overview.md](../00-overview.md) for the *why* and
[../01-side-effects-audit.md](../01-side-effects-audit.md) for the measured
evidence behind every inclusion and exclusion. Work in the order below — the
machinery (01) blocks everything; the conventions (02) and the notification
consumer (03) unblock the migrations (04–09).

| # | File | Covers |
| --- | --- | --- |
| 01 | [01-post-commit-dispatch.md](01-post-commit-dispatch.md) | Move dispatch after the commit; scope/failure isolation; depth cap; typed publisher |
| 02 | [02-events-and-handlers-conventions.md](02-events-and-handlers-conventions.md) | Event/handler placement, naming, payload rules, registration, standing exclusions |
| 03 | [03-in-app-notifications.md](03-in-app-notifications.md) | `NotificationEntity`, `INotifier`, four feed endpoints, v1 type catalog |
| 04 | [04-identity-events.md](04-identity-events.md) | Nine identity events; the security-invalidation handler hosting five missing reactions; symmetry-gap fixes |
| 05 | [05-commerce-events.md](05-commerce-events.md) | Eight commerce events; the `OrderPaidEvent` paid-effects fan-out decision |
| 06 | [06-cache-invalidation-events.md](06-cache-invalidation-events.md) | Publication/tag events; three cache handlers replacing 19 call sites + 4 omissions |
| 07 | [07-engagement-counter-events.md](07-engagement-counter-events.md) | Five engagement events; counters + cache as one consumer per surface |
| 08 | [08-external-resource-cleanup.md](08-external-resource-cleanup.md) | Content deletion/orphan events + Core file lifecycle; ordering-bug fixes; thumbnail fetch |
| 09 | [09-community-and-revision-events.md](09-community-and-revision-events.md) | Revision/submission decision facts; artist-claim entity + events; new templates |
| 10 | [10-testing-strategy.md](10-testing-strategy.md) | The untouched-suites safety net; unit/integration conventions per layer |
| 11 | [11-verification-checklist.md](11-verification-checklist.md) | Build/suites, grep-provable invariants, behavioral sweep, runbook |

## Global progress

- [x] 01 — Post-commit dispatch
- [x] 02 — Events and handlers conventions
- [x] 03 — In-app notifications
- [x] 04 — Identity events + security invalidation
- [x] 05 — Commerce events
- [x] 06 — Cache invalidation events
- [x] 07 — Engagement counter events
- [x] 08 — External-resource cleanup
- [x] 09 — Community and revision events
- [x] 10 — Testing strategy
- [x] 11 — Verification

## Decisions (resolved; each recorded in its owning spec)

| Decision | Spec | Resolution |
| --- | --- | --- |
| Session invalidation on password/email/role change — product sign-off (behavior change) | 04 | **approved** — do it; self-service exempts the acting session, admin-driven revokes all |
| Refresh-token replay detection in this wave | 04 | **in scope** — the revocation machinery ships in the same wave; marginal cost is one raise site + one handler |
| Paid-effects: full event move vs synchronous stamping + event for fan-out only | 05 | **full move** — raise-time windows + idempotent handler + reconciliation query make the crash window operable |
| Login-alert handler on `SessionCreatedEvent(IsNewDevice)` | 04/email-06 | **events land now, alert handler deferred** — email-noise threshold is a product call |

## Ground rules

- The audit's exclusion list is binding: nothing on it moves without
  reopening the audit.
- Existing integration suites pass untouched (spec 10) — the refactor's
  safety net.
- Every event follows spec 02; every deviation is written into the owning
  spec's implementation notes, never left implicit.
