# Side-Effects Audit — What Really Belongs on Domain Events

The measured inventory behind the spec set: every place a business operation
triggers (or should trigger) a secondary reaction, with a verdict per finding.
Two criteria decide every verdict:

1. **Same-transaction required?** If splitting the reaction out to a
   post-commit event would break an invariant a user can observe, it stays in
   the transaction and out of events.
2. **Multiple plausible consumers?** Email + in-app + cache + audit on one
   fact is the pattern's justification; a single mechanical consumer usually
   wants a shared service, not an event.

## Content module

### Cross-aggregate mutations

| Finding | Where | Verdict |
| --- | --- | --- |
| Payment verification stamps promotion/social-boost/pending-review onto articles, videos and lyrics via three speculative repository probes per order item | `AdminVerifyPaymentFactory.ApplyPaidEffectsAsync` | **Strongest candidate in the module** — one operation, three foreign aggregates, four plausible consumers (receipt email, promotion cache, review queue, revenue audit). One trap: the promotion window is computed as `UtcNow + DurationDays` at verification time, so the window must be computed at raise time and carried on the event payload, never recomputed by a deferred consumer |
| Approving a lyrics submission creates a whole `LyricsEntity`, then links it back | `AdminApproveLyricsSubmissionHandler` | **Candidate, but a saga, not a fire-and-forget hook** — the code already runs two separate commits with a documented reconciliation gap, and the submission needs the created lyrics id back |
| Vote threshold auto-accepts a revision and applies its text to the lyrics/translation | `PublicVoteOnLyricsRevisionHandler` / `PublicVoteOnTranslationRevisionHandler` | **Split it**: `Accept` + text application are one invariant (the code says so explicitly) and stay in-transaction; the *acceptance fact* fans out to notify/cache/audit via an event. The threshold block is duplicated across both handlers — the event consumer also deduplicates it |
| Admin revision decisions (accept/reject) — same accept→apply pair, currently with **no notification at all** | `AdminDecideLyricsRevisionHandler` / `AdminDecideTranslationRevisionHandler` | Same split as above; four near-duplicate handlers prove the fan-out belongs in one place |
| Lyrics↔video `HasLyrics` denormalized flag maintained from three lyrics handlers | `AdminCreateLyricsHandler`, `AdminUpdateLyricsHandler`, `AdminDeleteLyricsHandler` | **Not an event** — single-consumer projection that must stay in-transaction (a stale flag renders a broken affordance). Wants a shared `VideoLyricsLinkService`; note the album-link handler maintains no equivalent, so today's cascade is inconsistent by aggregate |
| Artist claim request writes **only a log line**; ownership verification has zero reactions | `PublicRequestArtistClaimHandler`, `AdminVerifyArtistOwnerHandler` | **Candidate by absence** — a business fact with no durable record and no consumers; `ArtistClaimRequested` (admin inbox + audit) and `ArtistOwnershipVerified` (claimant email + in-app) are missing, not misplaced |

### External-resource side effects (Cloudinary, YouTube)

| Finding | Where | Verdict |
| --- | --- | --- |
| Article update purges orphaned body images **after** commit, then needs a second commit to drop the rows — a Cloudinary failure leaves dangling rows | `AdminUpdateArticleHandler` | **Candidate** — a hand-rolled post-commit reaction; an `ArticleBodyImagesOrphaned` consumer adds retry/failure isolation to what the handler already does |
| Article/video/short-video **deletes** call Cloudinary **before** the commit — a failed commit leaves live content with dead assets | `AdminDeleteArticleHandler`, `AdminDeleteVideoHandler`, `AdminDeleteShortVideoHandler` | **Candidate and a live bug risk** — the opposite ordering from the update path; one of the two is wrong. Post-commit `…Deleted` events fix the ordering and make all four uniform |
| YouTube thumbnail download + Cloudinary upload run **inside** the attach-URL command; a thumbnail outage fails the whole attach | `AdminAttachYoutubeVideoUrlHandler` | **Candidate** — the thumbnail is a reaction (`YoutubeUrlAttached`), not part of the operation's validity; today two network calls hold a change-tracked entity unflushed |
| Odesli resolution before any mutation, failures surfaced as domain errors | resolve-streaming-links handlers | **Not a candidate** — the external call *is* the operation |
| Old-asset deletion on image replacement | `ReplaceImageFileAsync` (Core), used by 8 upload handlers | **Not a candidate** — already centralized; it is the pattern the delete handlers above should reuse |

### Cache invalidation — the best-justified consumer

Four `IMemoryCache` projections (popular articles/videos/tags + all-tags
sharing the tags token) invalidated by **19 hand-placed calls**, with these
**proven omissions**:

- `AdminSetLyricsTagsHandler` mutates the tag graph — zero invalidations
  (its article/video siblings both invalidate)
- `AdminDeleteArticleCommentHandler` decrements the comment count — zero
  invalidations (its Public sibling invalidates)
- `AdminDeleteArticleHandler` / `AdminDeleteVideoHandler` — zero
  invalidations; currently benign only because deletion is restricted to
  never-popular statuses, a distant invariant
- Lyrics and short-video engagement invalidate nothing (no cache exists —
  consistent, but the asymmetry is one new cache away from becoming a bug)

**Verdict: migrate wholesale.** Invalidation never needs the transaction, is
idempotent, and the omissions prove manual placement is failing. Events also
fix the omissions as a side effect of the migration.

### Engagement counters — the code already wants this

Every like/unlike/bookmark/share/comment handler mutates a counter on a
*different* aggregate (~20 sites), and the DTO doc comments literally
describe the target model: *"Cached number of likes… incremented by
interaction events."* The view paths (`RecordLyricsView`,
`RecordShortVideoView`) go further: they already persist raw
`…ViewEventEntity` rows with an `isCounted` flag and a retention job — an
event-sourced counter with a synchronous projection stapled on.

**Verdict: candidate, high volume/low risk.** The interaction row commits
in-transaction (it is the operation); the counter increment and the cache
invalidation move behind one engagement event per surface. Counters lag by
milliseconds — acceptable for social counts the module already calls
"cached."

### Background jobs

Both jobs (`AbandonedDraftCleanupJob`, `ShortVideoViewEventCleanupJob`) react
to *time thresholds*, not business moments — **polling is correct, keep
them.** The draft job's inner Cloudinary purge shares the external-cleanup
consumer from spec 08.

### Explicit non-candidates (Content)

`ArtistEntity.RecomputeNameIndexes` (intra-aggregate derived state, exactly
where it belongs), `RecalculateTotalFromItems` (aggregate invariant —
though note: not every item-mutating factory calls it; verify separately),
`StreamingLinkFactory` (pure function), SEO/slug updates (explicit
operations), the `HasLyrics` flag (single-consumer projection).

## Identity and Core modules

### Sessions — the highest-leverage findings in the codebase

| Finding | Where | Verdict |
| --- | --- | --- |
| Seven call sites duplicate the revoke-then-maybe-email shape (`SessionEntity.Revoke` from sign-out ×2, revoke ×2, sign-out-all ×2, force-logout) | session factories and handlers | **Strong candidate** — `SessionRevokedEvent(UserId, SessionId, Reason)` gives revocation one shape and the future denylist/audit/push consumers one hook |
| **Sessions are never revoked on**: password change, password reset, set-password, email change, role grant/revoke — verified absent; no session repository is even injected at those call sites. A stolen refresh token survives the victim's password reset; a revoked role stays effective until JWT expiry | change/reset/set-password handlers, profile factory, role handlers | **The single best argument for the whole refactor** — five security-sensitive changes share one missing reaction (*revoke + notify + audit*); one event-subscribed handler hosts it for all five |
| Session creation captures device metadata only on the new-session branch; **reactivation never refreshes IP/user-agent**, silently corrupting session metrics and exports | `SessionFactory` reuse-or-create | Metadata refresh is a plain bug fix (in-transaction, same aggregate); `SessionCreatedEvent(IsNewDevice)` / `SessionReactivatedEvent` are candidates — the deferred login-alert email finally gets its seam where the new-device decision is already made |
| No session cap, no old-session cleanup on login | `SessionFactory` | Future consumers on `SessionCreatedEvent`; recorded, not built |
| Refresh-token replay: an already-rotated token just throws `InvalidRefreshToken` — **no reuse detection**, no family revocation, no alert | `RefreshTokenFactory` | Candidate (medium): `RefreshTokenReplayDetectedEvent` is genuinely multi-consumer but must be *created*, not just moved — stretch item |
| `CleanupExpiredSessions` is an admin endpoint, **never scheduled** (no Quartz job in Identity), pseudo-revokes instead of purging (corrupting `RevokedAt` semantics), never deletes despite `DeletedCount` naming; `OtpRepository.CleanupExpiredOtpsAsync` is fully dead code | cleanup handler + repositories | **Not event work** — scheduling/semantics bugs, tracked in spec 04's non-event list |

### Files and avatars (Core) — divergent paths and ordering hazards

| Finding | Where | Verdict |
| --- | --- | --- |
| Two replacement paths disagree: Content's `ReplaceImageFileAsync` deletes the Cloudinary asset then **soft**-deletes the row; Identity's avatar path **hard**-deletes the row and never calls Cloudinary — surviving only because uploads reuse `publicId = userId` with `Overwrite = true`, an undocumented invariant one change away from leaking every historical avatar | `FileRepository` | **Strong candidate** — `FileReplacedEvent(OldStorageKey)` / `FileSoftDeletedEvent` unify both paths and move the external delete post-commit |
| Social-login avatar replacement hard-deletes the row while the new "file" is an external URL with no storage key — a previously uploaded Cloudinary asset is orphaned with no row pointing at it; **no orphan sweeper exists anywhere in Core** | social avatar path | Same events; the sweeper finally has somewhere to live |
| Avatar update is a cross-module dual-write: file row deleted + committed on `CoreDbContext` **before** `user.UpdateAvatar` commits on `IdentityDbContext`; a failure between them leaves the user pointing at a deleted file (avatar silently vanishes). Cloudinary upload also fires before the business commit | avatar handlers + auth factory | Ordering hazard in the same class as Content's delete handlers — the file events + post-commit handling shrink the window and make the external calls retryable |

### Cross-aggregate mutations (Identity)

| Finding | Verdict |
| --- | --- |
| Signup → visitor role assignment (one commit, broken invariant without it) | **Not a candidate** — same-transaction invariant |
| Verify-OTP: `otp.MarkAsUsed` + `user.MarkAsVerified` + bulk OTP invalidation under one commit; welcome email inline in the Public handler and **missing in the Admin handler** | The OTP cascade stays inline; `UserVerifiedEvent` hosts the welcome and erases the Public/Admin asymmetry by construction |
| Role grant/revoke handlers re-fetch the user after commit *only* to send the email | Candidate — the event payload (userId + roleName) deletes the extra query |
| Admin profile factory sends **no email-change notification** (its Public twin sends two) | Symmetry gap erased by `UserEmailChangedEvent` |
| Resend-OTP invalidate-then-create cascade; seeders | **Not candidates** — atomic single-consumer cascade; idempotent bootstrap |

### Audit trail — absent entirely

No `AuditLog`/`IAuditService`/action log exists anywhere in `src`; the only
audit surface is row-level `CreatedBy`/`UpdatedBy` columns. Force-logout,
role changes, session revocations and admin user edits leave no trail beyond
the mutated row. Every event in specs 04–09 is a ready-made audit feed — the
audit consumer itself is future work, but the hooks come free with this
refactor.

### One more dual-write, in our own new code

`OutboxMailer.EnqueueAsync` commits the outbox row *after* the business
commit (the spec 02 email-service decision) — a crash between the two loses
the email silently. The events refactor does not change this (handlers run
post-commit too); the honest mitigation remains the resend paths and, for
the commerce side, the reconciliation queries. Recorded so the limitation
stays visible rather than assumed away.

## The ranked list — what really needs to move

1. **Security invalidation on credential/authorization change** (spec 04) —
   five verified-absent reactions (password change/reset/set, email change,
   role change → sessions never revoked) hosted by one event-subscribed
   handler; the single best argument for the refactor because it is a live
   security gap, not a refactor nicety.
2. **`SessionRevokedEvent`** (spec 04) — seven duplicated revoke sites get
   one shape; denylist/audit/push consumers get their hook.
3. **`OrderPaidEvent` fan-out** (spec 05) — replaces the three-aggregate
   probing cascade; windows computed at raise time ride the payload.
4. **Cache invalidation** (spec 06) — 19 sites plus 4 proven omissions
   collapse into three handlers; the omissions get fixed by construction.
5. **Engagement counters** (spec 07) — ~20 increment+invalidate pairs become
   one consumer per surface; the DTOs already document this as the model.
6. **External-resource cleanup ordering** (spec 08) — post-commit
   `…Deleted` / `ImagesOrphaned` / `FileReplaced` events fix the
   delete-before-commit bug class in Content **and** Core (divergent avatar
   paths, orphaned social-login assets) and add retry.
7. **All notification emails + the new in-app feed** (specs 03–05, 09) —
   identity security, commerce lifecycle, comment replies: one event, two
   channels; the Public/Admin symmetry gaps (admin welcome, admin
   email-change notification) disappear by construction.
8. **Revision/submission decision facts** (spec 09) — the accept→apply
   invariant stays in-transaction; the decision fact fans out and gains the
   notifications that don't exist today.
9. **Missing-fact events** (specs 04, 09) — artist claim requested/verified,
   refresh-token replay: durable facts and consumers for what today is a
   log line or a bare throw.

And what must not move: OTP creation + delivery, newsletter opt-in emails,
paid-effect **application** semantics beyond the event payload (windows
computed at raise time), counters' interaction rows, `HasLyrics`, order
total recalculation, both background jobs.
