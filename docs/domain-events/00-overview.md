# Domain Events — Overview

## Why this exists

The codebase ships a complete domain-event mechanism — `Aggregate<T>` collects
`IDomainEvent`s, `DomainEventPublisher` fans them out to
`IDomainEventHandler<TEvent>` implementations from DI, and
`DispatchDomainEventsInterceptor` is registered in every module's EF pipeline —
and **none of it is used**: zero concrete events, zero handlers, zero
`AddDomainEvent` callers. Meanwhile the codebase has accumulated, by hand, all
the problems the mechanism exists to solve:

- **~30 post-commit side-effect call sites** (email via `IMailer` /
  `ICommerceCustomerNotifier`, popular-content cache invalidation) injected
  into handlers one by one.
- **19 hand-placed `Invalidate()` calls with at least 4 proven omissions** —
  the "someone forgot the second call" failure mode, measured, not
  hypothetical (see the audit).
- **One commerce operation reaching into three unrelated editorial
  aggregates** through speculative repository probing
  (`ApplyPaidEffectsAsync`).
- **~20 near-identical counter-increment pairs** whose own DTO doc comments
  already say *"incremented by interaction events"* — the intended model is
  documented, just never built.
- **External-resource cleanup with contradictory orderings** — one handler
  deletes Cloudinary assets after commit, three delete before; one of those
  orderings is wrong.
- **Missing reactions**: business facts (artist claim requests, revision
  decisions) that today produce a log line or nothing at all.

## What this feature does

1. **Fixes the machinery** — dispatch moves to after the commit, handlers run
   isolated in fresh scopes, a handler failure can never fail a committed
   operation (spec 01).
2. **Defines the conventions** — where events live, what they carry, where
   handlers live, how they register (spec 02).
3. **Adds the in-app notification feed** — a new consumer fed exclusively by
   events, with its own entity and endpoints (spec 03).
4. **Migrates the real candidates** — identity security emails (spec 04),
   commerce lifecycle (spec 05), cache invalidation (spec 06), engagement
   counters (spec 07), external-resource cleanup (spec 08), community and
   revision facts (spec 09).
5. **Keeps the non-candidates put** — every "no" is recorded with its reason
   in the audit, so scope is a decision, not an accident.

## The one-sentence rule

An operation's own invariants stay in its transaction; everything that
*reacts* to a committed fact — email, in-app notification, cache, counters,
external cleanup, audit — moves behind an event.

## Reading order

[01-side-effects-audit.md](01-side-effects-audit.md) is the evidence — the
full measured inventory of every reaction in the codebase and its verdict.
Then work through [specs/00-index.md](specs/00-index.md) in order: the
machinery first (01–02), the new notification consumer (03), then the
migrations (04–09), then testing and verification (10–11).
