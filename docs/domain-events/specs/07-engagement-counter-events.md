# Spec 07 — Engagement Counter Events

## Goal

Collapse ~20 near-identical "write interaction row, reach into the content
aggregate, bump a counter, invalidate a cache" handler tails into one event
per surface. The DTOs already document the target model — *"Cached number of
likes… incremented by interaction events"* — and the view paths already
persist raw event rows with an `isCounted` flag. This spec builds what the
comments describe.

## The split

- **In-transaction (the operation):** the interaction row itself — like,
  bookmark, share, comment, view-event row. Unique constraints on these rows
  remain the concurrency control.
- **Post-commit (the reaction):** the denormalized counter on the content
  aggregate and the cache invalidation. Counters lag by milliseconds;
  the module already calls them "cached."

## Events

One event per surface, raised by the **interaction aggregate's** create/
delete paths:

| Event | Raised by | Payload |
| --- | --- | --- |
| `ArticleEngagedEvent(ArticleId, Kind, Delta)` | article like/unlike, bookmark/unbookmark, share, comment add/delete (both public and admin — **fixing the admin-delete omission**), reply add | `Kind`: `EnumEngagementKind` (Like, Bookmark, Share, Comment, View); `Delta`: +1/−1 |
| `VideoEngagedEvent(VideoId, Kind, Delta)` | video share, rating | |
| `LyricsEngagedEvent(LyricsId, Kind, Delta)` | lyrics like/unlike, share, counted view | |
| `ShortVideoEngagedEvent(ShortVideoId, Kind, Delta)` | shorts like/unlike, bookmark/unbookmark, share, counted view | |
| `CommentEngagedEvent(CommentId, Delta)` | comment like/unlike | comment-local counter |

The comment-reply event from the engagement/notification side
(`CommentReplyAddedEvent`, spec 09 for identity of record) *also* implies an
`ArticleEngagedEvent(Comment, +1)` — raised alongside, not derived.

## Handlers

Per surface, one `<Surface>EngagementHandler` that:

1. loads the content aggregate by id and applies the counter delta
   (`IncrementLikeCount()` etc. — the entity methods survive unchanged);
2. invalidates the surface's cache where one exists (articles, videos — the
   spec 06 asymmetry note covers lyrics/shorts).

Idempotency note: counter deltas are not naturally idempotent. The handler
relies on exactly-once-per-commit dispatch (spec 01) — acceptable because a
crash loses at most one increment on a value the module defines as a cached
approximation, and the raw rows remain the source of truth. The
`ShortVideoViewEventCleanupJob` doc comment already anticipates a recompute
pass from raw events; that recompute (a periodic `COUNT(*)` reconciliation)
is noted for the runbook, not built here.

## View counting

`RecordLyricsView` / `RecordShortVideoView` keep their gating logic (the
`isCounted` decision *is* the operation), then raise the engagement event
instead of incrementing inline. The raw `…ViewEventEntity` write stays
in-transaction, untouched.

## What the migration deletes

The counter-mutation + invalidate tails of ~20 interaction handlers, plus
their content-repository and invalidator constructor dependencies where the
handler needed them only for the tail.

## Testing

- Unit: interaction aggregates/handlers assert the raised event (kind,
  delta); engagement handlers apply the right entity method per kind and
  invalidate the right cache (mocked).
- Integration: the existing interaction endpoint tests assert persisted
  counters over real HTTP — they keep passing untouched, which proves the
  event path end to end. Add the admin-comment-delete regression (counter
  decrements *and* cache refreshes).

## Checklist

- [x] Five engagement events raised by every interaction path
- [x] Per-surface handlers applying counters + cache
- [x] View gating untouched; raw view rows in-transaction
- [x] ~20 inline tails deleted; handler dependencies pruned
- [x] Existing integration suites untouched and green; regression added

## Implementation notes

- The events are raised inside the interaction aggregates themselves: the
  `Create` factories raise the `+1` fact, and a `MarkRemoved()` method on
  the removable rows (likes, bookmarks, comment likes) raises the `-1`
  fact. The repositories call `MarkRemoved` in their existing
  load-then-remove paths, so unlike/unbookmark flows raise without handler
  involvement. `ArticleCommentEntity.SoftDelete` raises the comment `-1`
  directly — which is what fixes the admin-delete omission by construction
  (both the public and admin handlers go through `SoftDelete`).
- `SoftDelete` returns `bool` and is a no-op on an already-deleted comment:
  the comment lookups do not filter deleted rows, so an owner delete
  followed by admin moderation of the same comment would otherwise raise a
  second `-1` and drift the article's cached comment count permanently
  (`DecrementCommentCount` floors at zero, but drift above zero persists).
  Both delete handlers skip the repository update and the commit when the
  call returns `false`, and still report success — the endpoint status codes
  and response shapes are unchanged.
- `EnumEngagementKind` gained a `Rating` member beyond the five listed
  kinds: `PublicRateVideoHandler`'s recompute tail (count + average from
  the rating rows) moved into `VideoEngagementHandler`, which recomputes
  from the committed rows and ignores the delta. Rating events carry
  `Delta: +1` on row creation and `Delta: 0` on an in-place restar.
- `ArticleEngagedEvent(Comment, +1)` is raised by both
  `ArticleCommentEntity.Create` and `CreateReply`, covering the
  reply-implies-engagement note without waiting for spec 09's
  `CommentReplyAddedEvent`.
- Article and video engagement handlers own their surface's cache eviction
  (spec 06's "one consumer, two lines"); lyrics, shorts and comment
  handlers apply counters only, per the cache-asymmetry note.
- Each engagement handler mutates only the aggregate it owns (article,
  video, lyrics, short video, comment respectively) — the spec 11 grep
  invariant holds.
- All five handlers load their target with the nullable `GetByIdAsync` and
  skip on `null`, logging at Debug. A target deleted between the interaction
  commit and the post-commit dispatch is a legitimate race — the counter
  dies with the row — not an error, so no handler throws for it and none
  invalidates a cache it did not change.
- Because the `+1` facts ride the `Create` factories, rows seeded directly
  in integration tests also produce counter increments post-commit. The
  full pre-existing integration suite was audited for seeded rows paired
  with counter assertions before choosing this shape; none exist, and the
  new admin-comment-delete regression exploits the behavior to arrange its
  counter through the event path.
- The counter-recompute reconciliation job stays future work for the
  runbook, unchanged from the spec.
