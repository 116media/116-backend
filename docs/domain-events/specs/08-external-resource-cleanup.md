# Spec 08 — External-Resource Cleanup Events

## Goal

Fix the contradictory Cloudinary-cleanup orderings found by the audit and
give remote-asset deletion one uniform, post-commit, retry-tolerant shape.

## The bug class being fixed

| Today | Where | Failure mode |
| --- | --- | --- |
| Delete remote assets **before** commit | `AdminDeleteArticleHandler`, `AdminDeleteVideoHandler`, `AdminDeleteShortVideoHandler` | Commit fails after remote delete succeeded → live content with dead asset URLs |
| Commit, then delete remote assets, then **second commit** to drop rows | `AdminUpdateArticleHandler` (orphaned body images) | Cloudinary failure → `article_images` rows dangle forever (second commit never runs) |

Two orderings, both hand-rolled, one of them wrong in every failure. The
correct shape for all four: **commit the business change first; clean the
remote assets as a post-commit reaction; tolerate cleanup failure** (an
orphaned remote asset is a cost problem, not a correctness problem — the
reverse, a dead URL on live content, is user-visible).

## Events

| Event | Raised in | Payload |
| --- | --- | --- |
| `ArticleDeletedEvent(ArticleId, CoverFileId?, BodyImageStorageKeys)` | article removal path | storage keys captured before removal |
| `VideoDeletedEvent(VideoId, ThumbnailFileId?)` | video removal path | |
| `ShortVideoDeletedEvent(ShortVideoId, VideoFileId?, ThumbnailFileId?)` | short-video removal path | |
| `ArticleBodyImagesOrphanedEvent(ArticleId, StorageKeys)` | `ArticleEntity` body update when images drop out | replaces the second-commit dance |

Deletion events are shared consumers with spec 06 (cache) — one event, two
handlers.

## Handlers

`ContentAssetCleanupHandler` per event (or one generic handler per asset
shape), in Content's `Application/Shared/EventHandlers/`:

- calls `ICloudinaryService.DeleteImagesAsync` / `IFileService.DeleteFileAsync`
  exactly as the inline code does today;
- on the orphaned-images event, also removes the `article_images` rows in its
  own scope/commit — the dangling-row bug disappears because row removal and
  asset deletion live in the same handler with the same retry story;
- failures are logged (spec 01 policy); the assets are re-cleanable — the
  `AbandonedDraftCleanupJob` already demonstrates the tolerant per-item
  pattern and its inner purge should call the same shared cleanup path.

## Core file lifecycle (the same bug class, second module)

The audit found the avatar paths in Core carry the identical hazards with
extra twists:

- **Divergent replacement semantics**: Content's `ReplaceImageFileAsync`
  deletes the remote asset then soft-deletes the row; Identity's avatar path
  hard-deletes the row and never calls Cloudinary — surviving only because
  uploads reuse `publicId = userId` with `Overwrite = true`, an undocumented
  invariant.
- **Orphaned assets**: the social-login avatar path replaces a Cloudinary
  file with an external-URL row (no storage key), leaving the old asset
  unreferenced forever; no orphan sweeper exists.
- **Cross-module dual-write**: the file row commits on `CoreDbContext`
  before `user.UpdateAvatar` commits on `IdentityDbContext`; a failure
  between them silently blanks the avatar. The Cloudinary upload also fires
  before any commit.

Events, raised by `FileEntity`:

| Event | Raised in | Handler |
| --- | --- | --- |
| `FileReplacedEvent(FileId, OldStorageKey?)` | the replacement paths, unified onto soft-delete semantics | post-commit remote delete of the old asset (skip when `OldStorageKey` is null) |
| `FileSoftDeletedEvent(FileId, StorageKey?)` | `FileEntity` soft-delete | same cleanup handler |

The dual-write window itself shrinks but does not vanish (two DbContexts by
design); ordering flips to business-commit-first everywhere, and the cleanup
becomes retryable. The orphan sweeper (a periodic job diffing rows against
assets) is recorded as future work that now has a home.

## Boundary notes

- `ReplaceImageFileAsync` (upload flows) stays as-is — replacement is
  centralized in Core and is the operation itself, not a reaction.
- The YouTube thumbnail fetch (`AdminAttachYoutubeVideoUrlHandler`) is the
  inverse case — an external *acquisition* inside the transaction. It moves
  to a `VideoYoutubeUrlAttachedEvent` handler that downloads and attaches the
  thumbnail post-commit; the attach command stops failing on thumbnail
  outages, and the video renders thumbnail-less until the handler lands it.
  UI implication (brief thumbnail gap) is accepted and recorded here.

## Testing

- Unit: aggregates assert deletion/orphan events with captured keys; cleanup
  handlers call the right service per payload (mocked); thumbnail handler
  attach-and-commit path.
- Integration: delete endpoints keep their existing tests (rows gone over
  real HTTP) — plus regressions: a delete with a failing (stubbed) Cloudinary
  still deletes the content; an article body update with failing Cloudinary
  no longer leaves `article_images` rows behind.

## Checklist

- [x] Four cleanup events with keys captured pre-removal
- [x] Cleanup handlers; all pre-commit remote deletions removed
- [x] Thumbnail fetch moved post-commit; attach no longer fails on outages
- [x] Draft-cleanup job routed through the shared cleanup path
- [x] Failure-injection regressions green

## Implementation notes

- One `ContentAssetCleanupHandler` hosts all four content events (one concern:
  post-commit asset cleanup), mirroring the multi-event cache handlers rather
  than one class per event — both shapes are allowed above.
- File-entity-tracked assets (article covers, video thumbnails, short-video
  files) are not remote-deleted by the content handler directly: it soft-deletes
  the file rows, and the raised `FileSoftDeletedEvent` carries the storage key
  to the Core `FileAssetCleanupHandler`, which performs the single remote
  delete. Calling `IFileService.DeleteFileAsync` inline as the old code did
  would now double-delete every asset (the soft delete raises the file event
  regardless). Only the keyed `article_images` assets, which have no file row,
  are deleted directly via `ICloudinaryService.DeleteImagesAsync`.
- The `article_images` table holds cover rows too, and a cover row's storage
  key is *the cover `FileEntity`'s key* — the upload handler writes the file
  row's key onto the row it creates. Capturing every row's key for
  `ArticleDeletedEvent.BodyImageStorageKeys` therefore re-deletes the cover
  asset the file soft-delete already owns, and burns a one-shot
  `NextDeleteFailure` stub on the wrong call. Both removal paths
  (`AdminDeleteArticleHandler` and `AbandonedDraftCleanupJob`) filter to
  `ImageType == EnumArticleImageType.Body`, matching the orphan filter
  `AdminUpdateArticleHandler` already applies. The payload name is the
  contract: body keys only.
- A side effect of the unification: article covers' remote assets are now
  deleted on article deletion; the old inline code only soft-deleted the row
  and left the asset orphaned.
- In the orphaned-images handler the row removal commits before the remote
  delete, so a Cloudinary outage can orphan assets but can never leave
  `article_images` rows dangling — the regression the second-commit dance
  used to lose.
- `FileEntity` distinguishes `Delete()` (raises `FileSoftDeletedEvent`) from
  `MarkReplaced()` (same soft-delete state, raises `FileReplacedEvent`), so
  replacement flows do not double-raise. `ReplaceImageFileAsync` /
  `ReplaceVideoFileAsync` and both avatar update paths now go through
  `MarkReplaced`; Identity's avatar hard-delete (`Remove` + save) is gone.
- `VideoYoutubeUrlAttachedEvent` carries the attached URL (the fact itself);
  the post-commit handler extracts the 11-character id, downloads and attaches
  the thumbnail in its own scope/commit. Because dispatch is synchronous
  within the request, the thumbnail is normally attached before the response
  returns; only on a thumbnail outage does the video render thumbnail-less
  until a later attach. The attach command no longer performs any extraction
  or download; the URL-format guarantee lives in the command validator.
- Because that dispatch is synchronous inside the request, the thumbnail fetch
  is on the caller's clock and must be bounded. The typed client carries a
  10-second timeout (the `OdesliStreamingLinkResolutionService` precedent),
  and `YoutubeThumbnailService`'s maxres→hqdefault fallback excludes
  `OperationCanceledException` — the fallback covers a missing rendition, not
  a fetch that ran out of time. Without both, an unconfigured client's
  100-second default timeout was retried by the bare catch and the publisher
  swallowed the result, so a YouTube outage blocked the attach request for
  roughly 200 seconds.
- Failure injection: `StubCloudinaryService` gained a one-shot
  `NextDeleteFailure` (the `StubEmailSender.NextFailure` precedent) and is now
  registered as a singleton; regressions live in
  `tests/Integration/Workflows/ExternalAssetCleanupFlowTests.cs` and cover
  the three deletes, the body-image orphan path, the YouTube attach, and the
  avatar replacement path.
- The orphan sweeper (rows-vs-assets diff job) remains future work, as
  recorded above.
