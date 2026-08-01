# Spec 06 — Cache Invalidation Events

## Goal

Retire all 19 hand-placed `cacheInvalidator.Invalidate()` calls — and fix the
four proven omissions — by hanging the invalidators on the events the
mutations already justify. Invalidation is the ideal first consumer: never
transactional, idempotent, and an over-eager invalidation costs only a cache
miss.

## Events

Publication set changes (raised in the aggregates' transition methods):

| Event | Raised in | Invalidates |
| --- | --- | --- |
| `ArticlePublishedEvent` / `ArticleUnpublishedEvent` | `ArticleEntity.Publish` / transitions leaving the published set (`Reject`, `Archive`) | popular-articles |
| `ArticleDeletedEvent` | `ArticleEntity` removal path (raised by the delete handler's aggregate — see spec 08, shared event) | popular-articles (fixes the omission) |
| `VideoPublishedEvent` / `VideoUnpublishedEvent` / `VideoDeletedEvent` | `VideoEntity` transitions | popular-videos (delete fixes the omission) |
| `TagGraphChangedEvent` | tag create/update/delete handlers' aggregate paths and the three set-tags flows (article, video, **lyrics — fixing the `SetLyricsTags` omission**) | popular-tags + all-tags |

Engagement-driven invalidation arrives with spec 07's engagement events —
the same event that moves a counter also invalidates the surface's cache
(one consumer, two lines). The `AdminDeleteArticleComment` omission is fixed
there.

## Handlers

One per cache token, in Content's `Application/Shared/EventHandlers/`:

- `PopularArticlesCacheHandler` — subscribes to article published/
  unpublished/deleted + article engagement events
- `PopularVideosCacheHandler` — video equivalents
- `PopularTagsCacheHandler` — `TagGraphChangedEvent`

The three `IPopularXxxCacheInvalidator` singletons survive unchanged as the
mechanism; only their call sites move.

## Lyrics / shorts asymmetry

No popular-lyrics or shorts cache exists, so their events get **no cache
handler** — but the events themselves exist (spec 07), so adding a future
cache is one handler, not twenty call-site edits. Recorded so the asymmetry
stays a decision.

## What the migration deletes

All 19 inline `Invalidate()` calls and the invalidator constructor
parameters they forced into handlers (`AdminPublishArticleHandler` et al.
shrink accordingly).

## Testing

- Unit: each aggregate transition asserts its event; each cache handler
  calls its invalidator once per event (mocked).
- Integration: the existing popular-content endpoint tests already prove
  invalidation behavior over real HTTP (publish → appears; like → reorders)
  — they must keep passing untouched. Add one regression test per fixed
  omission (e.g. set-lyrics-tags now refreshes the tags cache; admin
  comment-delete refreshes popular articles).

## Checklist

- [x] Publication + tag events raised in aggregates
- [x] Three cache handlers registered; all inline calls deleted
- [x] All four audit omissions covered by regression tests
- [x] Existing popular-content integration tests untouched and green

## Implementation notes

- Engagement-driven invalidation lives in the spec 07 engagement handlers
  (`ArticleEngagementHandler` / `VideoEngagementHandler`), per this spec's
  own "one consumer, two lines" note — the cache handlers here subscribe
  only to the publication, deletion and tag-graph events. Subscribing them
  to the engagement events as well would have double-evicted on every
  interaction.
- `ArticleUnpublishedEvent` / `VideoUnpublishedEvent` are raised only when
  the transition actually leaves the published set (`Reject` / `Archive`
  guard on the prior status). Archiving a draft no longer evicts the cache
  the way the old inline call did — a draft was never in the ranked list,
  so the eviction was pure waste.
- `ArticleDeletedEvent` / `VideoDeletedEvent` were created here with spec
  08's payload shape (storage keys and file ids captured before removal,
  via `ArticleEntity.MarkDeleted(bodyImageStorageKeys)` /
  `VideoEntity.MarkDeleted()` called by the delete handlers ahead of the
  repository `Remove`). Only the cache consumer is attached; spec 08 adds
  the asset-cleanup consumer to the same events and removes the pre-commit
  Cloudinary calls, which were left untouched here.
- `TagGraphChangedEvent` is raised by `TagEntity` (create/update/
  `MarkDeleted`) and by the three tag-link aggregates (`ArticleTagEntity`,
  `VideoTagEntity`, `LyricsTagEntity`) on create and on `MarkRemoved`,
  which the repositories invoke in their link-removal paths. A bulk tag
  replacement therefore raises one event per changed link rather than one
  per flow; the handler is an idempotent token cancellation, so the extra
  dispatches cost only scope creation on an admin-frequency operation.
- The four omission regressions live in
  `tests/Integration/Workflows/CacheInvalidationRegressionTests.cs`. Each
  warms the cache over HTTP, goes stale via a raw counter/link write that
  bypasses the event pipeline, proves the stale read, then drives the
  previously-omitted mutation over HTTP and asserts the refreshed read.
