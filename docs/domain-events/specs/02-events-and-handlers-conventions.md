# Spec 02 — Events and Handlers: Conventions

## Goal

One set of rules every migrated concern follows, so events stay uniform
across modules and reviews can be mechanical.

## Event records

- Live in the owning module's `Domain/Events/`, one file per event.
- Named `<Aggregate><FactInPastTense>Event`: `UserPasswordChangedEvent`,
  `OrderPaidEvent`, `ArticlePublishedEvent`.
- Immutable records implementing `IDomainEvent`:

```csharp
/// <summary>
/// Raised when a payment is verified and its order marked paid.
/// </summary>
/// <param name="OrderId">The paid order.</param>
/// <param name="PaymentId">The verified payment.</param>
/// <param name="Items">The paid items with their effects computed at raise time.</param>
public record OrderPaidEvent(Guid OrderId, Guid PaymentId, IReadOnlyList<PaidItemEffect> Items) : IDomainEvent;
```

- **Raised inside aggregate methods** via `AddDomainEvent` — the fact is
  declared where the mutation lives. Handlers and factories never call
  `AddDomainEvent` on someone else's aggregate.
- Payload rules:
  - ids and domain facts the aggregate owns at the moment of mutation;
  - **time-sensitive derived values are computed at raise time and carried**
    (the promotion-window trap from the audit) — a deferred consumer must
    never recompute a value whose inputs include "now";
  - never presentation data: no culture, no tokens, no URLs, no localized
    text. The email address appears only where the address itself is the
    fact (`UserEmailChangedEvent.OldEmail/NewEmail`).

## Handlers

- Live in the raising module under `Application/<Area>/EventHandlers/`, one
  class per (event, concern): `OrderPaidReceiptEmailHandler`,
  `OrderPaidPromotionStampHandler`, `ArticlePublishedCacheHandler`,
  `CommentReplyAddedNotificationHandler`.
- Implement `IDomainEventHandler<TEvent>`; constructor-inject what they need
  (repositories, `IMailer`, `INotifier`, invalidators) and **re-resolve
  entities by id** — they run post-commit in a fresh scope and must not
  assume anything beyond the payload.
- A handler owns exactly one concern. When one fact feeds email and in-app,
  that is two handlers (or one `…NotificationsHandler` when both channels
  share every lookup — decide per event, note it in the spec that owns it).
- Handlers are idempotent where cheap (cache invalidation, notification
  upsert by natural key when retried manually); exact-once is not promised.

## Registration

Explicit per module, next to the other service registrations:

```csharp
services.AddScoped<IDomainEventHandler<OrderPaidEvent>, OrderPaidReceiptEmailHandler>();
services.AddScoped<IDomainEventHandler<OrderPaidEvent>, OrderPaidPromotionStampHandler>();
```

No assembly scanning — the module file remains the single readable registry
of every reaction in the module.

## What stays out of events (the standing exclusions)

| Concern | Reason |
| --- | --- |
| OTP creation and OTP delivery emails | The email *is* the use case; the code lives on a sibling aggregate the event would leak or re-query |
| Newsletter confirmation/welcome emails | Operational steps of the double-opt-in flow inside the Mailer module |
| Interaction rows, order totals, `HasLyrics`, name-index recomputation | Same-transaction invariants or intra-aggregate derived state |
| Revision text application (`Accept` → `ReplaceLyricsText`) | One invariant with the acceptance — in-transaction; only the decision *fact* fans out |
| Background cleanup jobs | Time-threshold polling has no raising moment |

## Testing conventions

- Aggregates: unit tests assert raised events
  (`entity.DomainEvents.Should().ContainSingle(e => e is …)`) alongside the
  state assertions that already exist.
- Handlers: unit tests with mocked dependencies per concern.
- Endpoints: existing integration tests keep asserting the *outcome* (outbox
  row, notification row, cache behavior) over real HTTP — they are
  implementation-agnostic and act as the refactor's safety net.

## Checklist

- [x] `Domain/Events/` folders and naming in place per module — created
      per module as its first concrete event lands (specs 03–09); no empty
      placeholder folders (verified at closure: Content, Core and Identity
      each carry a populated `Domain/Events/` folder)
- [x] Raise-time-computation rule honored for every time-sensitive payload —
      applies from the first concrete event onward (promotion windows on
      `OrderPaidEvent`, storage keys captured before removal on the
      deletion events)
- [x] One-concern-per-handler held; exceptions recorded in the owning spec —
      applies from the first concrete handler onward (dual-channel
      `…NotificationsHandler`s in specs 04/09, multi-event cache and
      cleanup handlers in specs 06/08)
- [x] Explicit registration only; no scanning
- [x] Standing exclusions untouched

## Implementation notes

- The `IDomainEventHandler<>` assembly scan was removed from
  `CqrsExtension.AddCqrsWithAssemblies`; modules now register every handler
  explicitly (`services.AddScoped<IDomainEventHandler<TEvent>, THandler>()`)
  next to their other service registrations. No module registered handlers
  through the scan (zero handlers existed), so this changes no runtime
  behavior.
- `IDomainEventPublisher` and `IDomainEventHandler<TDomainEvent>` moved out
  of `DomainEventPublisher.cs` into their own files under
  `src/Shared/Shared/Application/Services/`; the namespace is unchanged, so
  no call site changed.
