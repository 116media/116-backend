# Spec 09 — Community and Revision Events

## Goal

Give the community-contribution facts their events: the revision/submission
decisions that today notify nobody, and the artist-claim facts that today
produce a log line. This spec also closes the audit's community-email gap
(the moderator notes written into a void).

## The accept→apply split (the rule this spec lives by)

A revision's `Accept` and the application of its text
(`ReplaceLyricsText` / `ApplyAcceptedRevision`) are **one invariant** — the
code says so and it stays in one transaction, in all four handlers (two vote,
two admin-decide). What fans out is the *decision fact*. The four
near-duplicate threshold/decide blocks collapse: the aggregates raise the
event; consumers do the rest.

## Events

| Event | Raised in | Consumers |
| --- | --- | --- |
| `LyricsRevisionDecidedEvent(RevisionId, LyricsId, ProposedByUserId, Accepted, ByModerator)` | `LyricsRevisionEntity.Accept` / `Reject` | email (`RevisionDecided` template — new) + in-app (`RevisionDecided`) to the proposer |
| `TranslationRevisionDecidedEvent(RevisionId, TranslationId, ProposedByUserId, Accepted, ByModerator)` | `LyricsTranslationRevisionEntity.Accept` / `Reject` | same pair |
| `LyricsSubmissionDecidedEvent(SubmissionId, SubmittedByUserId, Outcome, ReviewNote?, PublishedLyricsId?)` | `LyricsSubmissionEntity.Approve` / `Reject` / `RequestRevision` | email (`SubmissionDecided` — new; carries the moderator note that is currently unreadable by its addressee) + in-app |
| `ArtistClaimRequestedEvent(ArtistId, UserId)` | new `ArtistClaimRequestEntity.Create` (see below) | admin in-app/audit consumer (v1: log + notification row to admins is out of scope — record only) |
| `ArtistOwnershipVerifiedEvent(ArtistId, UserId)` | `ArtistEntity.ClaimOwnership` | email (`ArtistVerified` — new) + in-app to the new owner |

New email templates (`RevisionDecided`, `SubmissionDecided`,
`ArtistVerified`) and notification types follow the spec 04 (email) / spec
03 (in-app) conventions — append-only enums, neutral/en/fr resources, null
recipient guards (`IUserLookupService` resolution, nullable emails).

## The submission-approval saga (scoped decision)

`ApproveLyricsSubmission` already runs two commits with a documented
reconciliation gap (create lyrics → approve submission). This spec does
**not** convert that orchestration into events — the create-then-link flow
needs the created id back and stays explicit in the handler. Only the final
`LyricsSubmissionDecidedEvent` (raised by `Approve` with the
`PublishedLyricsId` it just received) fans out. The existing reconciliation
posture is unchanged. Recorded so nobody "finishes" this into a saga without
a decision.

## The claim-request gap

`PublicRequestArtistClaimHandler` persists nothing — an event cannot be
raised without an aggregate to raise it. Minimum viable fix bundled here:
`ArtistClaimRequestEntity` (id, artist id, user id, created at; no workflow
states — approval remains the existing manual `VerifyArtistOwner` command),
whose creation raises `ArtistClaimRequestedEvent`. The handler's log line is
replaced by a durable row. Claim review UI/queue stays future work.

## Testing

- Unit: each aggregate decision method asserts its event; the four handler
  duplicates lose their fan-out blocks; new-template rendering both cultures;
  claim-request entity + event.
- Integration: decide/vote/approve endpoints assert outbox + notification
  rows for the proposer/submitter (including the moderator note landing in
  the email body); claim request persists a row over real HTTP; existing
  revision/submission endpoint tests untouched.

## Checklist

- [x] Five events raised in aggregates; fan-out blocks deleted from handlers
- [x] Three new email templates + notification types (both cultures)
- [x] `ArtistClaimRequestEntity` + migration replacing the log-line handler
- [x] Claim dedupe: already-owned and duplicate-request guards, backed by a
      unique `(artist_id, user_id)` index
- [x] Saga decision recorded (approval orchestration stays explicit)
- [x] Unit + integration coverage green

## Implementation notes

- A sixth event ships with this wave: `CommentReplyAddedEvent`, raised by
  `ArticleCommentEntity.CreateReply` alongside its engagement event. Spec 07
  already names this spec as its identity of record and the audit's item 7
  lists comment replies in the one-event-two-channels group, so the
  email-wave's inline hook (`PublicAddCommentReplyHandler.NotifyParentAuthorAsync`)
  moved behind the event: `CommentReplyAddedNotificationsHandler` now sends
  the existing `CommentReply` email *and* writes the previously-unreachable
  `CommentReply` in-app row. The `Excerpt` helper moved with it; the business
  handler lost its `IMailer` dependency.
- The revision/submission decision handlers had no fan-out blocks to delete —
  the audit's point was that the decisions notified nobody. The four
  vote/decide handlers kept their in-transaction accept→apply pairs
  untouched; the aggregates' `Accept`/`Reject`/`Approve`/`RequestRevision`
  now raise the decision facts, and one `…NotificationsHandler` per event
  owns both channels (they share every lookup, per the spec 02 exception).
- Both revision events share one `RevisionDecided` template/notification
  type; the translation handler resolves translation → lyrics so both copy
  variants speak in song titles and link to the lyrics page.
- The decision/outcome wording rides raw English tokens (`accepted`,
  `rejected`, `approved`, `returned for revision`) substituted into both
  cultures' copy — the established `RoleChanged` `{{action}}` precedent, kept
  for consistency rather than fixed here.
- `LyricsSubmissionDecidedEvent.ReviewNote` is null on approval; the email
  template renders the note token as an empty paragraph in that case. An
  approval's in-app row carries `linkPath` to the published lyrics page;
  rejection/revision rows carry no link.
- `ArtistClaimRequestedEvent` is raised by `ArtistClaimRequestEntity.Create`
  but has no registered consumer, exactly as scoped — the durable row
  (migration `20260817202830_AddArtistClaimRequests`, table
  `content.artist_claim_requests`) is the v1 record; the admin review queue
  stays future work. `PublicRequestArtistClaimHandler` lost its logger and
  gained the claim-request repository + unit of work.
- The row is only worth reviewing if it is a *distinct* request, so the
  handler carries two guards on top of the existence check: a profile that
  already has a verified owner (`ArtistEntity.UserId` set) rejects further
  claims with the existing `AlreadyClaimed` conflict, and a second request
  from the same account for the same profile rejects with a new
  `ClaimRequestAlreadyExists` conflict (resx triple on
  `ArtistErrorMessage`). Migration
  `20260818094853_AddArtistClaimRequestUniqueIndex` makes
  `(artist_id, user_id)` unique so the invariant survives concurrent
  submissions rather than resting on the read; the two single-column indexes
  stay for the per-artist and per-user listings. Still no admin review
  endpoint — that remains future work.
- The submission-approval orchestration is untouched: two commits, explicit
  create-then-link in `AdminApproveLyricsSubmissionHandler`, reconciliation
  posture unchanged. Only the `Approve` transition's event (carrying the
  `PublishedLyricsId` it just received) fans out. No saga was built.
- The reserved-type failure test in `NotificationRendererTests` now uses an
  out-of-range enum value to keep the missing-resource guard covered, since
  every real catalog member has copy after this wave.
