# Spec 01 — Post-Commit Dispatch

## Goal

Make the dormant event machinery safe to build on. The single blocking defect:
`DispatchDomainEventsInterceptor` dispatches on `SavingChanges` — **before**
the commit — so a handler failure aborts the business operation and handlers
observe uncommitted state. Nothing migrates until this is fixed.

## Required behavior

| Concern | Behavior |
| --- | --- |
| Timing | Events collected from tracked aggregates in `SavingChanges*` (the change tracker still knows them), buffered, dispatched in `SavedChanges*` — only after the commit succeeded |
| Failed saves | Buffered events are discarded; nothing dispatches |
| Handler isolation | Each event dispatches in a fresh DI scope; handlers never share the business operation's `DbContext` instance |
| Handler failure | Logged at `Error` with the event type and aggregate id, then swallowed — a side effect must never make a committed operation appear failed. (Email durability is unaffected: it lives in the outbox, not in the handler call) |
| Handler resolution failure | Isolated to the handler that cannot be constructed: the fan-out falls back to per-handler construction, logs the failing handler by type, and still runs its siblings |
| Cancellation | Dispatch runs with `CancellationToken.None`. The caller's token is bound to the request (`HttpContext.RequestAborted`); the reactions belong to the committed change, so a client disconnect must not skip them |
| Explicit transactions | The dispatch point is the completion of `SaveChanges*`, **not** the commit of an enclosing explicit transaction. Domain events must not be raised by work running inside `BeginTransaction`, or handlers would observe uncommitted state |
| Ordering | Events in raise order; handlers per event in registration order; no further guarantees — handlers must not depend on each other |
| Reentrancy | A handler that commits its own context (e.g. writing a notification row) can raise further events; cap the dispatch depth (constant, e.g. 3) and log-and-drop beyond it to make cycles impossible |

## Implementation shape

- `DispatchDomainEventsInterceptor` keeps the collection logic, moves the
  publish call into `SavedChanges` / `SavedChangesAsync`, buffering the
  collected events on the interceptor's per-save state (`eventData.Context`
  keyed) between the two phases.
- `DomainEventPublisher.Publish` replaces per-call reflection with a cached
  compiled delegate per event type — events fire on every mutation once
  specs 06–07 land; reflection per publish is avoidable overhead.
- The publisher takes an `ILogger` and owns the swallow-and-log policy so
  every dispatch path (interceptor or manual) behaves identically.

## Culture note

Handlers run post-commit in the request's async flow —
`CultureInfo.CurrentUICulture` (via `EmailCulture.Current()`) still reflects
the request. Events therefore never carry culture.

## Testing

- Unit (interceptor): committed save dispatches collected events exactly
  once; failed save dispatches nothing; events cleared from aggregates either
  way; handler exception is logged and not propagated; depth cap drops and
  logs.
- Unit (publisher): fan-out to all registered handlers; typed fast path hits
  the delegate cache; a handler that cannot be constructed is logged and its
  siblings still run; an event with no handler logs at `Debug`.
- Integration: one end-to-end proof — an endpoint whose aggregate raises an
  event produces the reaction row post-commit, and a request failing
  validation/conflict produces none.

## Checklist

- [x] Dispatch moved to `SavedChanges*` with per-save buffering
- [x] Fresh scope per event; swallow-and-log with event type + aggregate id
- [x] Dispatch depth cap
- [x] Compiled-delegate publisher fast path
- [x] Unit + integration coverage above green — unit coverage green
      (interceptor + publisher, full suite passing); the integration
      end-to-end proof landed with the first migrated raise sites (spec 04)

## Implementation notes

- The per-save buffer is keyed per `DbContext` via a `ConditionalWeakTable`
  on the singleton interceptor; it is discarded in `SaveChangesFailed`,
  `SaveChangesFailedAsync`, and `SaveChangesCanceledAsync`, so a failed or
  canceled save can never leak events into a later successful save on the
  same context.
- The depth cap lives in `DomainEventPublisher` (an `AsyncLocal<int>` around
  `Publish`), not in the interceptor, so the interceptor path and any manual
  `Publish` call share one policy — extending the spec's "publisher owns the
  swallow-and-log policy" rationale to reentrancy. The cap is
  `DomainEventPublisher.MaxDispatchDepth = 3`; the drop is logged at
  `Warning`.
- `IDomainEvent` exposes no aggregate-id member, so the failure log carries
  the event type name plus the event record itself — record `ToString()`
  renders the payload, which carries the aggregate id per spec 02's payload
  rules.
- Handler *resolution* failures (a handler whose constructor dependencies
  cannot be resolved) are swallowed and logged the same way as `Handle`
  failures, for the same reason: the operation is already committed. They are
  also isolated: resolving the fan-out in one container call is the normal
  path, but when that call throws, `DomainEventPublisher` falls back to
  constructing each registration on its own through
  `IDomainEventHandlerRegistry` — a snapshot of the handler registrations of
  the service collection the application was composed from. One broken
  constructor therefore removes exactly one reaction instead of every reaction
  subscribed to that event.
- An event that resolves zero handlers is logged at `Debug` with the event
  type, so a forgotten `AddScoped` registration is diagnosable. It is not a
  warning: `SessionCreatedEvent`, `SessionReactivatedEvent`, and
  `ArtistClaimRequestedEvent` are intentionally consumerless.
- Dispatch uses `CancellationToken.None` on both the sync and async paths, and
  `DispatchBufferedEvents` takes no token at all so the invariant cannot
  regress. Endpoints bind their token to `HttpContext.RequestAborted`; a
  client disconnecting after the commit would otherwise silently skip the
  session revocations, outbox rows, and counter updates the committed change
  owes.
- The per-event dispatch loop is itself guarded: a throw from `CreateScope()`
  or from resolving `IDomainEventPublisher` is logged and swallowed. The data
  is durable by then, so no dispatch failure may make the committed operation
  appear failed.
- `SavedChanges*` fires when the save completes, not when an enclosing
  explicit transaction commits. Under `Database.BeginTransaction` handlers
  would observe state no other connection can read and a rollback would leave
  the reactions behind, so domain events must not be raised inside an explicit
  transaction. The constraint is documented on the interceptor's class
  comment; the two explicit-transaction call sites today (the SuperAdmin
  seeder and the outbox dispatcher job) raise no events.
- Handlers for one event share that event's scope (fresh scope per event,
  not per handler); events dispatch sequentially in raise order, handlers
  sequentially in registration order.
- The interceptor takes an `ILogger` for the dispatch failures it swallows;
  `BaseModule` registers the logging services alongside the interceptor so any
  module database registration is self-sufficient.
- The end-to-end dispatch proof landed with spec 04's identity events:
  `tests/Integration/Workflows/DomainEventDispatchFlowTests.cs` drives the
  public change-password endpoint over real HTTP and asserts the committed
  operation produces its reaction rows (outbox email, notification row,
  session invalidation) while a rejected request produces none.
