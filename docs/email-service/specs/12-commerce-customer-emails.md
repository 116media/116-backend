# Spec 12 — Commerce Customer Emails

## Goal

Give the platform its missing channel to paying customers. Every commerce
endpoint is admin-only and B2B customers never log in — without email they
learn nothing: not the amount to pay, not that their payment bounced, not
that their placement was pulled. `CustomerEntity.Email` is required and
validated; this spec finally uses it.

## New templates (append to `EnumEmailTemplate`)

| Template | Required tokens |
| --- | --- |
| `OrderInvoice` | `customerName`, `orderReference`, `amountUsd`, `paymentMethods`, `itemSummary` |
| `PaymentReceipt` | `customerName`, `orderReference`, `amountUsd`, `receiptUrl` (optional-empty token allowed), `paidAt` |
| `PaymentRejected` | `customerName`, `orderReference`, `notes` |
| `OrderCancelled` | `customerName`, `orderReference` |
| `PromotionForceRemoved` | `customerName`, `contentTitle`, `reason`, `removedAt` |
| `CommissionedContentPublished` | `customerName`, `contentTitle`, `publicUrl` |
| `CommissionedContentRejected` | `customerName`, `contentTitle`, `reason` |
| `ShootScheduled` | `customerName`, `contentTitle`, `shootDate` |

## Hook table

| # | Event | Hook site | Recipient reachability |
| --- | --- | --- | --- |
| 1 | Order submitted → `PendingPayment` | `AdminSubmitOrderFactory` (after `Submit` + payment creation) | order loaded via `GetByIdWithItemsAsync` — ensure the query includes `Customer`, else extend it |
| 2 | Payment verified → `Paid` | `AdminVerifyPaymentFactory` (after `Verify` + `MarkPaid`) | same include path as 1 |
| 3 | Payment rejected | `AdminRejectPaymentHandler` (after `Reject(notes)`) | handler loads only the payment — resolve customer via `payment.Order` include or `ICustomerRepository.GetByIdAsync(order.CustomerId)`; pick whichever the existing repository supports and record it here |
| 4 | Order cancelled | `AdminCancelOrderHandler` | handler loads the order without customer — resolve via `ICustomerRepository.GetByIdAsync` |
| 5 | Force-unpromote article / video / lyrics | the three `AdminForceUnpromote*Handler`s (after `ForceUnpromote`) | entity carries `CustomerId` (nullable) — email only when non-null, resolve via `ICustomerRepository` |
| 6 | Commissioned content published | `AdminPublishArticleHandler` / `AdminPublishVideoHandler` / `AdminPublishLyricsHandler` | **only when `CustomerId != null`** — free editorial content sends nothing |
| 7 | Commissioned content rejected | `AdminReject*Handler`s | same `CustomerId != null` guard; include the captured rejection reason |
| 8 | Video shoot scheduled | `AdminScheduleShootHandler` (after `ScheduleShoot`) | via `VideoEntity.CustomerId` |

Shared rules:

- Enqueue joins the handler's existing commit, as always.
- `MarkPendingReview` (inside the verify-payment transaction) sends nothing —
  it is the same business moment as the receipt (#2).
- **No refund promises.** The force-unpromote email states the reason and that
  the team will follow up; no refund automation exists and the template must
  not imply one.
- Amounts render from the stored `AmountUsd` snapshot, never recomputed.

## Culture for customer emails

Customers are CRM records with no request culture and no language field.
V1 sends **neutral-culture** (English source) emails. If the customer base
turns out francophone-first, the clean fix is a `Language` column on
`CustomerEntity` — an open decision to record here, not a reason to guess.

## Order reference

There is no human-readable order number — templates need one for
`orderReference`. Use the short id form (first 8 hex chars of the order
`Guid`, uppercased) consistently across all commerce templates; a real
sequential reference column is future work if the business asks.

## Testing

- Unit: each factory/handler asserts template + tokens (mocked `IMailer`);
  the `CustomerId == null` guards on 5–7 send nothing; rejected-payment notes
  passthrough.
- Integration (extend existing commerce endpoint tests): submit-order,
  verify-payment, reject-payment, cancel-order, force-unpromote,
  publish/reject-commissioned, schedule-shoot each persist the expected
  outbox row with the customer's address; free-content publish persists none.

## Checklist

- [x] Eight templates + resources appended
- [x] Hooks 1–8 with the customer-resolution strategy per row recorded
- [x] `CustomerId` null guards verified on promotion/publish/reject paths
- [x] No-refund-promise wording reviewed in all three cultures
- [x] Unit + integration coverage green

## Implementation notes

- All eight hooks route through one `ICommerceCustomerNotifier`
  (`Application/Commerce/Services`), keeping handlers to a single call and
  centralizing the `CustomerId == null` guards, customer resolution
  (`GetByIdWithItemsAsync` includes `Customer`; cancel/unpromote paths
  resolve via `ICustomerRepository`), the short order reference, and the
  neutral-culture decision.
- `CustomerEntity`'s display field is `FullName`.
- Reject-payment reloads the order with items+customer after the payment
  update to reach the recipient.
