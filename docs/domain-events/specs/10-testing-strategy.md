# Spec 10 — Testing Strategy

Everything follows
[docs/testing/00-unit-vs-integration-rules.md](../../testing/00-unit-vs-integration-rules.md);
this spec only adds the event-specific conventions.

## The safety net

The single most important property of this refactor: **the existing
integration suites must pass untouched.** They assert outcomes (outbox rows,
notification rows, counters, cache behavior, deleted rows) over real HTTP —
implementation-agnostic by design. Any integration test that has to change
because call sites became events is a signal the refactor changed behavior,
not shape. Exceptions must be argued in the PR, not slipped in.

## Unit layer

| Target | Convention |
| --- | --- |
| Aggregates | Every mutation that raises an event gets an assertion alongside its existing state test: `entity.DomainEvents.Should().ContainSingle(e => e is OrderPaidEvent)` — plus payload assertions for raise-time-computed values (promotion windows, captured storage keys) |
| Event handlers | One test class per handler, dependencies mocked; assert the single concern (mailer called with template X / invalidator called once / counter method applied per kind) |
| Interceptor + publisher | Spec 01's timing table, line by line: no dispatch on failed saves, swallow-and-log, depth cap, delegate cache |
| Renderers | New email templates and notification types × {en, fr}, no unresolved placeholders (existing renderer test pattern) |

Handler unit tests that previously asserted "handler calls mailer" move to
the event handler's test class; the business-handler tests lose those
assertions and their mock parameters — they get *simpler*, which is the
point.

## Integration layer

| Addition | Proves |
| --- | --- |
| One end-to-end dispatch test (spec 01) | committed op → reaction row; failed op → nothing |
| Notification feed endpoints (spec 03) | pagination, ownership isolation, read idempotency |
| Regression per fixed cache omission (spec 06) | set-lyrics-tags, admin comment-delete, article/video delete |
| Failure-injection cleanup tests (spec 08) | content deletion survives Cloudinary failure; no dangling image rows |
| Community decision emails/notifications (spec 09) | moderator notes reach their addressee |

## What not to write

- No reflection-based "every event has a handler" meta-tests — registration
  is explicit in module files and reviewed there.
- No direct construction of events/handlers inside `tests/Integration/`
  (house rule); handler behavior is covered by unit tests plus the real-HTTP
  outcome assertions.
- No timing/sleep-based tests around dispatch — post-commit dispatch is
  synchronous in the request flow; if a test needs a sleep, the design drifted.

## Checklist

- [x] Existing integration suites green — the argued exceptions are the
      shared test builders (not test files) plus the tests corrected by the
      seeding-reconstitution convention below, each listed in the
      implementation notes
- [x] Aggregate event assertions added alongside state tests
- [x] Per-handler unit classes; business-handler tests simplified
- [x] The five integration additions above green

## Seeding is reconstitution, not behavior

The standing convention for integration arrangement, and the rule any new
builder inherits for free:

**`BaseApiTest`'s seed helpers discard pending domain events before saving.**
`SeedAsync` (both overloads) and `SeedTestUsersAsync` route through
`SaveSeededAsync`, which walks `ChangeTracker.Entries<IAggregate>()` and calls
`ClearDomainEvents()` immediately before `SaveChangesAsync`. Seeding a row is
therefore equivalent to loading a row that already existed — which is what an
arrangement means.

The reason is that seed helpers resolve their context from `Api.Services`, so
the dispatch interceptor is attached. Builders reach a target state by calling
real domain methods, and those methods raise the events they own, so without
the clear every test's *arrangement* ran production reactions before the act:
`SeedTestUsersAsync`'s three `MarkAsVerified()` calls wrote three welcome-email
outbox rows before every single integration test; `artist.ClaimOwnership(...)`
wrote email and in-app rows addressed to a seeded user;
`ContentOrderBuilder.MarkPaid` stamped promotions onto seeded content. Those
are arrangement steps rewriting the state under test and polluting every
count assertion.

Consequences to keep in mind when writing tests:

- A counter, promotion, notification, or outbox row that the arrangement needs
  is **seeded explicitly** (`article.IncrementCommentCount()` alongside the
  seeded comment row), never obtained as a side effect of the seed.
- Only the **act** produces reactions. A test asserting a reaction must drive
  it through real HTTP or a real repository — which is the house rule anyway.
- Direct `CreateDbContext<T>()` + `SaveChangesAsync()` inside a test body is
  *not* covered by the clear, and should not be: that shape is used for act
  steps and for deliberate raw mutations, where dispatch is the point.

## Implementation notes

- The convention above replaced an earlier, narrower fix. During the first
  full verification `PublicGetOwnPlaylistsEndpointV1Tests` failed because
  `VideoBuilder` seeded published videos through the real
  `AttachYoutubeVideoUrl`, which since spec 08 raises
  `VideoYoutubeUrlAttachedEvent`, so seeding dispatched the post-commit
  thumbnail workflow over the test's explicitly seeded thumbnail state. That
  was patched in the builder by writing the URL as plain state via reflection.
  The patch treated one symptom of a systemic problem and left the same trap
  in `LyricsRevisionBuilder`, `LyricsTranslationRevisionBuilder`,
  `LyricsSubmissionBuilder`, `ArtistBuilder`, `VideoBuilder.ScheduleShoot`,
  and `ContentOrderBuilder.MarkPaid`. With the clear centralized in
  `BaseApiTest`, the reflection workaround is gone and `VideoBuilder` calls
  the real `AttachYoutubeVideoUrl` again.
- Production behavior was never in question in any of this: the only
  production raise path for each of these events is its command handler,
  where the reaction is the intended one.
- This supersedes the spec 07 note that seeded interaction rows produce their
  counter increments post-commit. They no longer do, and the
  admin-comment-delete regression that relied on it now seeds the comment
  counter explicitly.
