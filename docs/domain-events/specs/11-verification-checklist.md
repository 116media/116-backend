# Spec 11 — Verification Checklist

The final sweep once specs 01–10 are implemented and individually checked.
Verify against the current codebase; fix regressions before ticking.

## Build and suites

- [x] `dotnet build` clean; `dotnet csharpier .` produces no diff —
      0 warnings, 0 errors; `dotnet csharpier check .` clean over 3560 files
- [x] Full unit suite green — 7527 passed, 7 skipped (pre-existing skips),
      0 failed
- [x] Full integration suite green — 1867 passed, 0 failed. Two
      pre-existing test files changed, both argued in the safety-net section
      below: they were reading seed-time side effects that spec 10's
      seeding-reconstitution convention removes

## Architecture invariants (grep-provable)

Grep commands and results are recorded in the verification notes below.

- [x] No business handler or factory outside the Mailer module injects
      `IMailer`, except the OTP flows (signup, forgot ×2, resend ×2) — the
      documented exceptions
- [x] No handler outside `EventHandlers/` folders calls
      `cacheInvalidator.Invalidate()`
- [x] No handler mutates a counter on an aggregate it does not own
- [x] No remote-asset deletion (`DeleteImagesAsync` / `DeleteFileAsync`)
      executes before its business commit outside the cleanup handlers and
      the draft-cleanup job — the job itself now raises `MarkDeleted` and
      commits before the event-driven cleanup runs
- [x] Every outbound HTTP call reachable from a synchronous post-commit
      handler runs on a typed client with an explicit timeout, and no
      handler-side retry can multiply it — the two are
      `OdesliStreamingLinkResolutionService` and `YoutubeThumbnailService`,
      both at 10 seconds, the latter with its fallback excluding
      `OperationCanceledException` (spec 08)
- [x] `AddDomainEvent` is called only inside `Domain/Entities` methods
- [x] `INotifier` is called only from event handlers
- [x] Every event type has ≥1 registered handler, and every registration's
      event is raised somewhere (dead-event check — by review, not
      meta-test); the three consumer-less events are the recorded
      exceptions: `SessionCreatedEvent` / `SessionReactivatedEvent`
      (spec 04 — login-alert handler deferred) and
      `ArtistClaimRequestedEvent` (spec 09 — durable row is the v1 record)

## Behavioral sweep (dev, against Mailpit + the API)

Each item is mapped to the automated integration coverage that now proves
it; boxes are ticked only where an automated test covers the full item.

- [ ] Password change: email + in-app row appear; the operation still
      succeeds with the mailer stubbed to fail (swallow-and-log observed) —
      rows + session invalidation are automated in
      `Workflows/DomainEventDispatchFlowTests`
      (`ChangePassword_Committed_ProducesEmailNotificationAndSessionInvalidation`,
      `ChangePassword_Failed_ProducesNoReactionRows`); swallow-and-log is
      unit-proven
      (`DomainEventPublisherTests.Publish_WhenHandlerThrows_SwallowsLogsErrorAndRunsRemainingHandlers`)
      and SMTP failure is absorbed by the outbox
      (`Workflows/EmailDeliveryFlowTests.Dispatcher_TransientFailure_SchedulesARetryInsteadOfFailing`),
      but no integration test drives change-password with a failing event
      handler — that observation: manual sweep pending
- [ ] Payment verification: receipt email, promotion stamped with the
      window computed at verification time (compare against a delayed
      manual dispatch) — stamping + receipt outbox row are automated in
      `Workflows/CommerceEventDispatchFlowTests`
      (`VerifyPayment_Committed_StampsContentAndWritesReceiptOutboxRowThroughTheEventPath`,
      `VerifyPayment_WhenAlreadyVerified_Returns409AndDispatchesNothing`);
      the raise-time window payload is unit-proven in
      `ContentOrderEntityTests` / `AdminVerifyPaymentFactoryTests`; the
      delayed-manual-dispatch comparison: manual sweep pending
- [x] Article delete with Cloudinary down: content gone, cleanup failure
      logged, no dangling rows —
      `Workflows/ExternalAssetCleanupFlowTests.DeleteArticle_WhenCloudinaryDeleteFails_StillDeletesArticleAndImageRows`
      (plus the video / short-video / update-orphan variants in the same
      class)
- [x] Set lyrics tags: tags cache refreshes (fixed omission) —
      `Workflows/CacheInvalidationRegressionTests.SetLyricsTags_RefreshesPopularTagsCache`
- [x] Submission rejection: the moderator note arrives in the submitter's
      email and notification feed —
      `Workflows/CommunityEventFlowTests.RejectLyricsSubmission_OverRealHttp_DeliversTheModeratorNoteToTheSubmitter`
- [x] Notification feed: badge count, read, read-all across two users —
      `PublicGetUnreadNotificationCountEndpointV1Tests.UnreadCount_CountsOwnUnreadRowsOnly`,
      `PublicMarkNotificationReadEndpointV1Tests.MarkRead_AnotherUsersRow_ReturnsNotFoundAndLeavesItUnread`
      (and idempotency siblings),
      `PublicMarkAllNotificationsReadEndpointV1Tests.ReadAll_MarksEveryOwnUnreadRowAndLeavesOtherUsersUntouched`

## Documentation closure

- [x] Every spec's checklist ticked with deviations recorded in that spec
- [x] The two scoped decisions recorded where they live: paid-effects
      (spec 05, "full move" with the reconciliation query below),
      submission saga (spec 09, "approval orchestration stays explicit —
      no saga was built")
- [x] [00-index.md](00-index.md) global progress all `[x]`
- [x] The email-service docs cross-reference updated: spec 06/11/12/13 hook
      tables note their call sites moved to event handlers

## Runbook additions

- [x] Paid-orders-with-unstamped-items reconciliation query (spec 05's full
      move was chosen) — see below
- [x] Counter recompute-from-raw-events statement (spec 07 note) — see below
- [x] Notification retention decision revisit date — see below

### Counter recompute from raw events (spec 07)

Engagement counters are deltas applied post-commit, so they are not
naturally idempotent: a crash between the interaction commit and its
engagement handler loses at most one increment. The module defines the
counters as cached approximations and the raw interaction rows (likes,
bookmarks, comments, ratings, view events, shares) as the source of truth,
so the recovery is a periodic recompute — a `COUNT(*)` (and, for ratings,
`AVG`) per surface from the raw rows, overwriting the drifted counter. The
reconciliation job itself stays future work (spec 07); until it exists, run
the recompute manually when a counter is suspected of drifting.

### Notification retention revisit (spec 03)

V1 ships with no notification retention: rows accumulate indefinitely.
Spec 03 notes the eventual shape — a cleanup job deleting read rows older
than 90 days, in the same time-threshold polling category as the existing
cleanup jobs. Revisit the decision once the feed has a quarter of
production volume behind it (target: 2026-Q4); the trigger to act sooner is
the notifications table showing up in slow-query or storage reviews.

### Paid orders with unstamped items (spec 05)

Spec 05 moved the paid-effects stamping behind `OrderPaidEvent`; a crash
between the order commit and the effects handler leaves a paid order whose
content is not yet stamped. This query lists those orders. The recovery is
to redispatch the effects for the listed items (or stamp them manually) —
`OrderPaidEffectsHandler` is idempotent, so re-running it against an already
partially stamped order is safe, and the promotion window rides the event
payload, so a late application never shortens the customer's paid-for
window. Redispatch preserves later decisions rather than overwriting them: a
promotion force-removed after the order was paid (`UnpromotedAt >= PaidAt`)
stays removed, and content whose editorial state has moved past review
(`Approved`, `Published`, `Archived`) keeps that status. Note the deliberate
asymmetries baked into the predicates: lyrics have no social-boost concept
and persist no promotion level; `MarkPendingReview` advances only `Draft`,
`PendingPayment` and `Rejected`, so review status is checked only for
content still sitting in `PendingPayment`; and the promotion predicate
matches only content that was never unpromoted, since a force-unpromoted row
is settled, not unstamped.

```sql
-- Paid orders whose items have effects that never landed on the content
-- fulfilling them. Enum storage is integer:
--   content_orders.status:  2 = Paid
--   articles/videos/lyrics.status: 1 = PendingPayment (EnumContentStatus)
SELECT o.id AS order_id,
       i.id AS order_item_id,
       COALESCE(a.id, v.id, l.id) AS content_id,
       CASE WHEN a.id IS NOT NULL THEN 'article'
            WHEN v.id IS NOT NULL THEN 'video'
            WHEN l.id IS NOT NULL THEN 'lyrics'
       END AS content_type
FROM content.content_orders o
JOIN content.content_order_items i ON i.order_id = o.id
LEFT JOIN content.articles a ON a.order_item_id = i.id
LEFT JOIN content.videos   v ON v.order_item_id = i.id
LEFT JOIN content.lyrics   l ON l.order_item_id = i.id
WHERE o.status = 2
  AND (a.id IS NOT NULL OR v.id IS NOT NULL OR l.id IS NOT NULL)
  AND (
    -- purchased promotion never stamped (lyrics persist no level id).
    -- A force-unpromoted row is settled, not unstamped, so it is excluded.
    (i.promotion_level_id IS NOT NULL AND (
         (a.id IS NOT NULL AND a.unpromoted_at IS NULL
             AND (a.is_promoted = FALSE OR a.promotion_level_id IS DISTINCT FROM i.promotion_level_id))
      OR (v.id IS NOT NULL AND v.unpromoted_at IS NULL
             AND (v.is_promoted = FALSE OR v.promotion_level_id IS DISTINCT FROM i.promotion_level_id))
      OR (l.id IS NOT NULL AND l.unpromoted_at IS NULL AND l.is_promoted = FALSE)
    ))
    -- purchased social boost never stamped (no social boost on lyrics)
    OR (i.social_boost = TRUE AND (
         (a.id IS NOT NULL AND a.social_boost = FALSE)
      OR (v.id IS NOT NULL AND v.social_boost = FALSE)
    ))
    -- content still awaiting payment although the order is paid
    OR COALESCE(a.status, v.status, l.status) = 1
  );
```

## Verification notes (final sweep evidence)

Recorded at sign-off; every grep runs from the repository root.

### Invariant greps

- `grep -rln "IMailer" src | grep -v src/Modules/Mailer` — 18 files: the
  five documented OTP flows (`PublicSignUpAuthFactory`,
  `Public/AdminForgotPasswordHandler`, `Public/AdminResendOtpHandler`),
  twelve `EventHandlers/` classes, and `CommerceCustomerNotifier` — a
  service consumed only by the eight commerce event handlers (verified by
  grepping `ICommerceCustomerNotifier` consumers). No business handler or
  factory outside those exceptions touches the mailer.
- `grep -rn "\.Invalidate(" src` — 11 call sites, all inside
  `Application/**/EventHandlers/` (the two engagement handlers, the three
  popular-content cache handlers).
- Engagement handler review: `ArticleEngagementHandler`,
  `VideoEngagementHandler`, `LyricsEngagementHandler`,
  `ShortVideoEngagementHandler`, `CommentEngagementHandler` each load,
  mutate and commit only the aggregate they own.
- `grep -rn "DeleteImagesAsync\|DeleteFileAsync\|DeleteImageAsync" src`
  (excluding infrastructure adapters and contracts) — call sites exist only
  in `ContentAssetCleanupHandler` and `FileAssetCleanupHandler`, both
  post-commit. `AbandonedDraftCleanupJob` calls
  `draft.MarkDeleted(bodyImageStorageKeys)` + `Remove` + commit and lets
  the same event pipeline clean the remote assets.
- Each remote asset is deleted exactly once: both article removal paths
  filter their captured `article_images` keys to
  `ImageType == EnumArticleImageType.Body`, because a cover row's key is the
  cover `FileEntity`'s key and that asset is owned by the file soft-delete
  reaction (spec 08).
- `grep -rn "AddDomainEvent(" src` — every call site lives under a
  `Domain/Entities/` folder (or the `Shared/Domain` aggregate base).
- `grep -rln "INotifier" src | grep -v src/Modules/Mailer` — 10 files, all
  `EventHandlers/` classes.

### Dead-event inventory

41 event records exist; 38 have explicit module registrations; every one of
the 41 is raised at least once, exclusively from `Domain/Entities` methods.
The three consumer-less events and their recorded justifications:

- `SessionCreatedEvent` / `SessionReactivatedEvent` — spec 04: events land
  now, the login-alert consumer stays gated on the email-noise product call.
- `ArtistClaimRequestedEvent` — spec 09: the durable
  `content.artist_claim_requests` row is the v1 record; the admin review
  queue is future work. The row is deduped rather than consumed — an
  already-owned profile and a repeat request from the same account both
  conflict, and migration `20260818094853_AddArtistClaimRequestUniqueIndex`
  makes `(artist_id, user_id)` unique.

`SessionRevokedEvent` is not an exception: it has its registered
`SessionRevokedLogHandler` (log-only audit slot, spec 04).

### Safety-net exceptions

The safety net fired on seeding, not on production behavior. Integration
seed helpers resolve their context from the application container, so the
dispatch interceptor was attached and every arrangement ran production
reactions before its act — three welcome-email outbox rows before *every*
test from `SeedTestUsersAsync` alone. Spec 10 now states the standing rule:
`BaseApiTest`'s seed helpers clear pending domain events before saving, so
seeding is reconstitution. With that in place the earlier `VideoBuilder`
reflection workaround was removed and the builder calls the real
`AttachYoutubeVideoUrl` again.

Two pre-existing integration test files changed as a direct consequence,
both because they asserted a value that only existed as a seed-time side
effect:

- `Workflows/CacheInvalidationRegressionTests.AdminDeleteArticleComment_DecrementsCounterAndRefreshesPopularArticlesCache`
  — warmed the cache expecting `CommentCount == 1`, produced by the seeded
  comment's engagement event. The arrangement now states the counter
  outright (`entity.IncrementCommentCount()` on the seeded article); the
  act (the admin delete over real HTTP) still drives the decrement through
  the event path, which is what the regression exists to prove.
- `.../GetOwnPlaylists/V1/PublicGetOwnPlaylistsEndpointV1Tests.GetOwnPlaylists_ReturnsFirstFourNullableThumbnailSlotsInPlaylistOrder`
  — built its arrangement with a hand-rolled `CreateDbContext` +
  `SaveChangesAsync` block instead of `SeedAsync`, so it opted out of the
  central clear and kept auto-attaching stub thumbnails over its seeded
  thumbnail state. The arrangement now routes through `SeedAsync`; no
  assertion changed.

Production behavior is unaffected in both cases: the only production raise
path for these events is the corresponding command handler, where the
reaction is the intended one.
