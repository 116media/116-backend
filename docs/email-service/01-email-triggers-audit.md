# Email Triggers — Full Codebase Audit

A deep sweep of the backend (all three modules) and the frontend/dashboard
apps, cataloguing every business event that warrants an email, every surface
that already promises one, and every checked-and-excluded flow. Each entry
maps to the spec that covers it — or is explicitly parked as future work.

Structural facts the whole catalogue rests on:

- **Every commerce endpoint is admin-only and the B2B customer never logs
  in.** Email is the platform's *only* channel to a paying customer.
  `CustomerEntity.Email` is a required field; nothing is ever sent to it.
- Content-module aggregates store bare user `Guid`s (no FK to Identity by
  design). The existing bridge is
  `IUserLookupService.GetAuthorInfoByIdAsync` → `AuthorInfo.Email`
  (`Identity.Contracts`); `UserEntity.Email` is **nullable** (OAuth users may
  lack it), so every user-facing email needs a null-email skip path.
- Zero `TODO`/`notify`/mail markers exist anywhere in `src` — none of this is
  half-built; it is all absent.

## Commerce / paying customers → [specs/12-commerce-customer-emails.md](specs/12-commerce-customer-emails.md)

| Event | Trigger | Recipient | Notes |
| --- | --- | --- | --- |
| Order submitted → `PendingPayment` | `AdminSubmitOrderFactory` (`ContentOrderEntity.Submit`, `ContentPaymentEntity.Create`) | `order.Customer.Email` | The invoice moment — only time the customer learns the amount; all payment methods are offline (`BankTransfer`, `MobileMoney`, `Cash`) so no gateway emails exist either |
| Payment verified → `Paid` | `AdminVerifyPaymentFactory` (`Verify` + `MarkPaid`) | customer | The receipt: `ReceiptUrl` is captured but only visible via admin API; also when promotion/social-boost go live |
| Payment rejected | `AdminRejectPaymentHandler` (`Reject(notes)`) | customer | `Notes` exists "so the team can follow up with the client" — no follow-up mechanism exists; order waits in `PendingPayment` for a corrected proof the customer doesn't know to send |
| Order cancelled | `AdminCancelOrderHandler` (`Cancel`) | customer | Legal from `PendingPayment` — the customer may already be invoiced; no reason field exists |
| Force-unpromote (article/video/lyrics) | `AdminForceUnpromote*Handler` (`ForceUnpromote`) | customer via `CustomerId` on the entity | Paid placement killed early with a mandatory reason; MetaField documents a pro-rata refund formula — neither reaches the customer today |
| Commissioned content published | `AdminPublish*Handler` when `CustomerId != null` | customer | Fulfilment notice with the public URL |
| Commissioned content rejected | `AdminReject*Handler` when `CustomerId != null` | customer | Delivery failure on paid work; rejection reason is captured |
| Video shoot scheduled | `AdminScheduleShootHandler` (`ScheduleShoot`) | customer | Pre-paid productions — a date the client must show up for |

Excluded from commerce: draft-order basket edits (`EnsureDraft`-guarded),
`MarkPendingReview` (same transaction as the receipt), payment-proof-attached
acknowledgement (admin uploads on the customer's behalf), new-customer record
creation (no account to confirm), all catalog/pricing CRUD, all queries.

## Account security (Identity) → [specs/11-account-security-emails.md](specs/11-account-security-emails.md)

| Event | Trigger | Notes |
| --- | --- | --- |
| Email address changed | `PublicUpdateProfileAuthFactory` (`UpdateEmail`) | **Most severe gap found**: the new address is written with no re-verification and no OTP; nothing tells the old address it lost the account. Needs alert-to-old + confirmation-to-new; a proper re-verify flow is an open product decision recorded in spec 11 |
| Password changed (knew old) | `Public/AdminChangePasswordHandler` (`UpdatePassword`) | No session invalidation happens on change — the alert is the only defence |
| Password reset completed | `Public/AdminResetPasswordAuthFactory` | The success mail is the takeover-detection signal, distinct from the OTP delivery |
| Password set on a social account | `PublicSetPasswordHandler` → `SetPasswordAndChangeToLocal` | Account gains a second credential path |
| Signed out from all devices | `Public/AdminSignOutFromAllDevicesHandler` | Canonical mass-session-termination alert |
| Admin force-logout | `AdminForceLogoutUserHandler` | Staff action against a user, silently ejected from every device |
| Role granted / revoked | `AdminAssignRoleToUserHandler` / `AdminRemoveRoleFromUserHandler` | Privilege change invisible until a 403 |

Excluded: username/phone changes (low risk, deferred), self single-session
revoke and self sign-out (user-initiated, low value), token refresh.

No-trigger-exists (cannot be emailed until modeled): account
suspension/reinstatement (`UserEntity.Deactivate()` has zero callers, no
admin command exists), account deletion (no command exists).

## Engagement → [specs/13-engagement-emails.md](specs/13-engagement-emails.md)

| Event | Trigger | Notes |
| --- | --- | --- |
| Reply to your comment | `PublicAddCommentReplyHandler` | Handler already injects `IUserLookupService`; parent author's email is one call away. Skip self-replies and null emails |
| Comment removed by moderator | `AdminDeleteArticleCommentHandler` (`SoftDelete`) | Deferred in spec 13: no reason field exists, and no moderation queue/report pipeline exists at all |
| Comment liked | like handlers | Excluded — low-signal high-volume; digest territory, and no digest exists |

## Community contribution outcomes — future spec, not currently planned

Real triggers, reachable recipients (`SubmittedByUserId`/`ProposedByUserId`
via `IUserLookupService`), but **not part of the current implementation
wave**; they get their own spec when scheduled:

| Event | Trigger |
| --- | --- |
| Lyrics submission approved / rejected / needs-revision | `AdminApproveLyricsSubmissionHandler`, `AdminRejectLyricsSubmissionHandler`, `AdminRequestLyricsSubmissionRevisionHandler` — reject/revision notes are written *to the submitter* and are unreadable without delivery; `NeedsRevision` is a dead-end call-to-action today |
| Lyrics correction accepted / rejected (moderator) | `AdminDecideLyricsRevisionHandler` — note: no rejection-reason field exists on `LyricsRevisionEntity` |
| Lyrics correction auto-accepted by vote | `PublicVoteOnLyricsRevisionHandler` — no human in the loop; nothing else can ever tell the proposer |
| Translation revision decided / vote-auto-accepted | `AdminDecideTranslationRevisionHandler`, `PublicVoteOnTranslationRevisionHandler` |
| Artist ownership verified | `AdminVerifyArtistOwnerHandler` (`ClaimOwnership`) — grants queue-bypass privilege the user is never told about |

Vote flows have **no auto-reject** — below-threshold revisions stay `Pending`
forever, so there is no "voted down" trigger to hook.

## Gaps that block or distort email work

- **Artist claim request is a `LogInformation` line only** (`PublicRequestArtistClaimHandler`) — no entity, no table. Neither a requester receipt nor a staff queue is implementable until the claim is persisted. No claim-denied concept exists at all.
- **No promotion-expiry or order-expiry job.** `PromotedUntil` is enforced only at read time (specifications); "your promotion expired / renew?" needs a new background job before it can exist.
- **No refund implementation** — refunds appear only in XML docs; the force-unpromote email can state the reason but must not promise an automated refund.
- **Dashboard resend-OTP toast already asserts delivery** ("Un nouveau code de vérification a été envoyé à votre adresse e-mail") — currently false. Frontend forgot-password copy promises "We'll email you a code"; the OTP screen says "we sent you" (past tense). Spec 06 makes these true.
- **Frontend has no newsletter form, no contact form, no notification-preference screen, and no footer.** Spec 07's newsletter has zero UI today; the frontend artists-page docs assume a contact/email inbox channel that exists nowhere.
- The abandoned-draft cleanup job hard-deletes stale draft articles silently — acceptable (empty drafts), noted for completeness.

## Checked and excluded (no email warranted)

Likes/bookmarks/shares/ratings/playlists, lyrics and short-video view
recording, self comment edit/delete, all Lookup/Catalog admin CRUD,
role/permission entity CRUD (no user recipient), token refresh, single self
sign-out, `CleanupExpiredSessions`, avatar upload, both background cleanup
jobs, streaming-link resolution, SEO/tags/social-links admin ops,
promotion/unpromotion of free content, editorial approve/archive for
in-house content (`AuthorId` is a staff byline, not a community user), and
the session CSV/XLSX export (synchronous download — the file *is* the HTTP
response, there is no "export ready" moment).
