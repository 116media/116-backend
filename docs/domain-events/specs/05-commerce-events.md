# Spec 05 — Commerce Events

## Goal

Move the commerce lifecycle onto events, and dissolve the module's worst
coupling: `ApplyPaidEffectsAsync` probing three unrelated editorial
aggregates from inside the payment factory.

## Events (raised by the commerce aggregates)

| Event | Raised in | Payload notes |
| --- | --- | --- |
| `OrderSubmittedEvent(OrderId)` | `ContentOrderEntity.Submit` | |
| `OrderPaidEvent(OrderId, PaymentId, PaidAt, Items)` | `ContentOrderEntity.MarkPaid` | `Items` = `PaidItemEffect(OrderItemId, PromotionLevelId?, PromotionUntil?, SocialBoost)` — **windows computed at raise time**, never by a consumer; `PaidAt` is the payment's verification instant, the reference point consumers use to date later promotion decisions |
| `PaymentRejectedEvent(OrderId, PaymentId, Notes)` | `ContentPaymentEntity.Reject` | notes are the reviewer's text, part of the fact |
| `OrderCancelledEvent(OrderId)` | `ContentOrderEntity.Cancel` | |
| `ContentPromotionRemovedEvent(ContentId, ContentType, CustomerId?, Title, Reason)` | the three `ForceUnpromote` entity methods | one event, `EnumContentType` discriminator |
| `CommissionedContentPublishedEvent(ContentId, ContentType, CustomerId?, Title, Slug)` | `Publish` entity methods | raised always; handlers no-op on null customer |
| `CommissionedContentRejectedEvent(ContentId, ContentType, CustomerId?, Title, Reason)` | `Reject` entity methods | |
| `VideoShootScheduledEvent(VideoId, CustomerId?, Title, ShootDate)` | `VideoEntity.ScheduleShoot` | |

## Handlers

| Handler | Concern |
| --- | --- |
| `OrderSubmittedInvoiceEmailHandler` | `OrderInvoice` via the notifier service |
| `OrderPaidReceiptEmailHandler` | `PaymentReceipt` |
| `OrderPaidEffectsHandler` | **the `ApplyPaidEffectsAsync` replacement** — see below |
| `PaymentRejectedEmailHandler`, `OrderCancelledEmailHandler`, `ContentPromotionRemovedEmailHandler`, `CommissionedContentPublishedEmailHandler`, `CommissionedContentRejectedEmailHandler`, `VideoShootScheduledEmailHandler` | one email each, all delegating to `ICommerceCustomerNotifier` (which survives as the *how*: customer resolution, null guards, formatting, neutral culture) |

No in-app handlers in this spec — commerce recipients are B2B customers
without accounts (spec 03 scope rule).

## The paid-effects decision

`ApplyPaidEffectsAsync` today: `payment.Verify` + `order.MarkPaid`, then per
item three speculative lookups (article? video? lyrics?) stamping
promotion/social-boost/pending-review — all in one transaction.

Moving it behind `OrderPaidEvent` trades same-transaction stamping for:
retry isolation, the removal of the type-probing cascade from commerce, and
per-content-type handlers that live with their aggregates. The consistency
cost is real and must be stated honestly: **a crash between the order commit
and the effects handler leaves a paid order whose content is not yet
stamped.** Mitigations, all required:

1. Windows ride on the payload (computed at `MarkPaid` raise time) — a late
   handler applies the *original* paid-for window, so the customer never
   loses days to a delay.
2. The effects handler is idempotent — an effect already applied, or since
   settled by a later editorial or promotion decision, is skipped without a
   write — so manual redispatch after a crash is safe. The per-effect
   contract is spelled out in the implementation notes below.
3. A reconciliation query (paid orders with unstamped items) is documented in
   the runbook section of spec 11 — same posture as the lyrics-submission
   reconciliation comment that already exists in the codebase.

If, at implementation time, that trade reads as too weak for billing-visible
state, the fallback is recorded here in advance: keep the stamping
synchronous in the factory and raise `OrderPaidEvent` only for
email/cache/audit consumers.

**Decision: full move.** The three mitigations above are all mandatory; the
reconciliation query lands in spec 11's runbook. The fallback stays recorded
here and is only revisited if production ever shows the crash window being
hit in practice.

## What the migration deletes

- The eight `ICommerceCustomerNotifier` call sites and the notifier
  constructor parameters added to commerce/editorial handlers by the email
  wave — handlers shrink back to their pre-email shape.
- `ApplyPaidEffectsAsync`'s cross-aggregate probing (if the full move is
  chosen).

## Testing

- Unit: each aggregate method asserts its raised event (windows on the
  `OrderPaidEvent` payload asserted against the item durations); each
  handler with mocked notifier/repositories; effects-handler idempotency.
- Integration: the existing commerce endpoint tests keep passing untouched
  (they assert outbox rows and stamped content over real HTTP); one new test
  proves a rejected business operation (409 on double-verify) raises nothing.

## Checklist

- [x] Eight events raised in aggregates with raise-time-computed payloads
- [x] Email handlers replace all notifier call sites
- [x] Paid-effects decision made and recorded here
- [x] Reconciliation query documented (if the full move is chosen)
- [x] Unit + integration coverage green; existing suites untouched

## Implementation notes

- `ContentOrderEntity.MarkPaid` takes
  `(paymentId, verifiedAt, promotionDurationsByLevelId, errors)`: the
  aggregate cannot query promotion levels itself, so
  `AdminVerifyPaymentFactory` resolves the duration in days for every level
  referenced by the order's items and passes the map in, along with the
  verification instant it just stamped on the payment; the aggregate
  computes each item's `PromotionUntil` at raise time from that map.
  `PaidItemEffect` lives in `Domain/Events/` next to `OrderPaidEvent`.
- A level missing from the map raises the localized
  `ContentOrderErrors.PromotionDurationUnavailable()` (400) rather than a raw
  `KeyNotFoundException` escaping mid-verification.
- `MarkPaid` truncates the verification instant, and every window derived
  from it, to whole milliseconds. Postgres `timestamptz` keeps microseconds,
  so a millisecond-aligned value round-trips unchanged and a reloaded
  `PromotedUntil` can be compared to the payload for equality — without the
  truncation the already-stamped check false-negatived on every redispatch
  and re-stamped the same window forever. The entities' `StampPromotion`
  doc comments (`payment.verified_at + promotion_level.duration_days`) are
  therefore literally true to millisecond precision.
- The discriminator is the existing `EnumCoreContentType` (the spec's
  `EnumContentType` shorthand) — no new enum was introduced. The published
  email handler maps `Article`/`Video`/`Lyrics` to `ContentPublicLinks` and
  skips other members.
- All nine handlers live in `Application/Commerce/EventHandlers/`, including
  those for events raised by editorial aggregates: the concern (customer
  emails, paid effects) is commerce, and they all delegate to commerce
  services.
- `OrderPaidEffectsHandler` idempotency is defined per effect, and each
  effect is skipped when it is either already applied or has since been
  settled by a later decision:
  - **social boost** — a one-way flag, skipped when already set;
  - **promotion** — skipped when the target already carries the payload's
    exact level and expiry (`IsPromoted` + `PromotedUntil == PromotionUntil`,
    level compared where persisted — lyrics persist no level), and skipped
    as *settled* when `UnpromotedAt >= PaidAt`, i.e. a SuperAdmin
    force-removed the placement after this order was paid. Without that
    second test a redispatch revived a deliberately pulled promotion and
    left `UnpromotedAt/By/Reason` contradicting the live state;
  - **review status** — `MarkPendingReview` advances only from `Draft`,
    `PendingPayment` and `Rejected`, so a redispatch cannot pull `Approved`,
    `Published` or `Archived` content back into the review queue.
  An item with nothing left to apply produces no repository update and no
  commit. Each item commits on its own so one failing item never rolls back
  its siblings.
- The `ForceUnpromote` methods on `ArticleEntity` and `VideoEntity` clear
  `PromotionLevelId` alongside `IsPromoted`/`PromotedUntil`, so no stale
  placement level outlives the promotion it belonged to.
- `PaymentRejectedEvent.Notes` is `string?` — optional notes are part of the
  fact. `ICommerceCustomerNotifier.NotifyPaymentRejectedAsync` now takes
  `string?` and renders absent notes as an empty token, which also removed
  the long-standing CS8604 warning at the old inline call site.
- `VideoShootScheduledEvent` also fires when `AdminCreateVideoHandler`
  creates a video with an initial shoot date (it calls
  `VideoEntity.ScheduleShoot`), so pre-booked customers get the shoot-date
  email on creation too — a deliberate widening, not an accident.
- `CommissionedContentPublishedEvent` / `CommissionedContentRejectedEvent` /
  `ContentPromotionRemovedEvent` are raised for free editorial content as
  well (null `CustomerId`); the notifier owns the null-customer no-op.
- The reconciliation query landed in spec 11's runbook section as required.
