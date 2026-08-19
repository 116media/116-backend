# Spec 04 — Identity Events

## Goal

Move the identity security notifications onto events — and use the same
events to host the **missing security reactions** the audit proved absent:
session invalidation on credential/authorization changes, the Public/Admin
symmetry gaps, and (via one extensible consumer slot) a future audit trail.

## The finding that shapes this spec

Every security-sensitive change today carries exactly one inline email and
**zero security reactions**. Verified absent, with no session repository even
injected at the call sites:

- password change does **not** revoke sessions
- password reset does **not** revoke sessions — a stolen refresh token
  survives the victim's reset
- set-password (social → local) does **not** revoke sessions
- email change does **not** revoke sessions
- role grant/revoke does **not** revoke sessions — roles are baked into the
  JWT and never re-checked, so a revoked role stays effective until token
  expiry

All five share one shape: *credential/authorization change → revoke
sessions, notify, audit*. A textbook three-consumer event.

## Events

| Event | Raised in | Consumers (handlers) |
| --- | --- | --- |
| `UserVerifiedEvent(UserId)` | `UserEntity.MarkAsVerified` | welcome email — **fixes the Admin-verify-has-no-welcome gap by construction** |
| `UserPasswordChangedEvent(UserId, Origin)` | `UserEntity.UpdatePassword` — `Origin` (`EnumPasswordChangeOrigin`: Changed, Reset, SetLocal) passed as an argument, resolving the changed-vs-reset decision flagged in the first draft | email (template by origin) + in-app (session revocation is **not** a consumer: it is inline, see below) |
| `UserEmailChangedEvent(UserId, OldEmail, NewEmail)` | `UserEntity.UpdateEmail` | dual email (alert-old masked + confirm-new) + in-app — also **fixes the Admin-profile-has-no-email-notification gap** |
| `UserRoleGrantedEvent(UserId, RoleId, RoleName)` / `UserRoleRevokedEvent(...)` | `UserRoleEntity.Create` / removal path | email + in-app — also deletes the re-fetch-user-just-to-email dance in both role handlers |
| `UserSignedOutAllDevicesEvent(UserId, ByAdmin)` | raised at the sign-out-all/force-logout call sites via a `UserEntity.RecordMassSignOut(byAdmin)` method (the session family has no single aggregate to raise from; the user aggregate owns the fact) | email + in-app |
| `SessionRevokedEvent(UserId, SessionId, Reason)` | `SessionEntity.Revoke` — `Reason` (`EnumSessionRevokeReason`: SelfSignOut, AdminRevoke, SecurityInvalidation, Expiry) | v1: audit-ready slot; the event exists so the revocation consumers (denylist, push, audit) have their hook — **no v1 handler beyond logging** |
| `SessionCreatedEvent(SessionId, UserId, IsNewDevice)` / `SessionReactivatedEvent(SessionId, UserId)` | `SessionFactory` branches via entity factory methods | v1: **deferred consumers** — this is where the deferred login alert (email spec 06) finally gets its seam (`IsNewDevice` computed where the reuse-or-create decision is already made), plus the metadata-refresh fix; implement the events, gate the alert handler on a follow-up decision |
| `RefreshTokenReplayDetectedEvent(UserId, SessionId)` | does not exist today — `RefreshTokenFactory` just throws on replay; raise it where the invalid-rotation is detected, consumers: revoke session family + alert email | **decided: in scope this wave** — the revocation machinery ships anyway; this is one raise site + one handler |

## Session invalidation — inline and atomic

Session revocation is **not** an event consumer. It happens inline, in the
same transaction as the change that requires it:

- every flow that changes a credential or an authorization calls
  `ISessionRepository.DeleteAllByUserIdAsync(userId, EnumSessionRevokeReason.SecurityInvalidation, exemptSessionId, ct)`
  **before** its existing `CommitAsync`, so the new hash / new address / new
  role set and the revocation land together or not at all;
- the **current session is preserved** on self-service password/email change
  (the user should not be logged out by their own action); flows without a
  session (the OTP-driven reset) and administrative role changes revoke
  everything;
- **product-visible behavior change** — before this work users stayed logged
  in everywhere after a password reset. This spec says that is a bug, not a
  feature. Sign-off happens here before implementation: [x] approved —
  self-service changes exempt the acting session; admin-driven changes
  revoke everything.

### Why inline rather than a post-commit handler

The first implementation hosted the revocation in a
`SecuritySessionInvalidationHandler` subscribed to the four events. It was
replaced because post-commit dispatch is deliberately swallow-and-log: a
revocation that failed left a stolen refresh token alive while the endpoint
had already answered `200`. That trade is correct for a notification and wrong
for a security invariant. The work is also same-module, same
`IdentityDbContext` — it never needed the decoupling an event buys, and the
codebase already revokes inline in `PublicSignOutFromAllDevicesHandler` and
`AdminForceLogoutUserHandler`. Events keep what they are good at: the
notifications, which are allowed to be best-effort because their durability
lives in the outbox.

The revocation call sites are the public and admin change-password handlers,
the public and admin reset-password factories, the set-password handler, the
public update-profile factory (email branch only), and the role
assign/remove handlers. `RefreshTokenReplaySecurityHandler` stays an event
handler: it reacts to a fact detected in a *rejected* request, and it already
revokes before it emails.

## OTP flows — unchanged

OTP creation, invalidation cascades, and the four OTP delivery emails stay
direct (spec 02 exclusions). The welcome email moves (it reacts to
verification; it is not the verification).

## What the migration deletes

The 17 inline `IMailer` call sites in Identity (minus the OTP flows), their
constructor parameters, the mailer mocks across ~17 unit test classes, and
the role handlers' post-commit user re-fetches.

## Bugs found by the audit that are NOT event work (tracked, not fixed here)

- `CleanupExpiredSessions` is an admin endpoint, never scheduled; its
  "cleanup" pseudo-revokes instead of purging, corrupting `RevokedAt`
  semantics; `OtpRepository.CleanupExpiredOtpsAsync` is fully dead code.
  → separate fix: Quartz job + purge semantics + wire or delete the OTP
  cleanup.
- Session reactivation never refreshes IP/user-agent metadata — session
  metrics silently corrupt. → one-line fix inside the reactivate branch,
  independent of events.

## Testing

- Unit: each aggregate method asserts its event (+ origin/reason payloads);
  the invalidation handler with mocked session repository including the
  current-session exemption; welcome/email handlers per concern.
- Integration: existing identity endpoint tests keep passing (outbox rows
  unchanged); new: password reset over real HTTP leaves the other seeded
  session revoked; role revoke revokes sessions; admin profile email change
  now produces the dual emails (the fixed gap).

## Checklist

- [x] Events raised in `UserEntity` / `UserRoleEntity` / `SessionEntity` / factories
- [x] Email + in-app handlers replace the non-OTP inline hooks
- [x] Inline, same-transaction session revocation with current-session
      exemption at every credential/authorization change site (replaces the
      former `SecuritySessionInvalidationHandler`)
- [x] Product sign-off recorded for the invalidation behavior change
- [x] Symmetry gaps closed (admin welcome, admin email-change notification)
- [x] Replay-detection decision recorded (in or out of this wave) — in scope, implemented
- [x] Unit + integration coverage green

## Implementation notes

- The ten event records live in `Identity/Domain/Events/`. They carry no session-routing data:
  the acting session id travels through the command and handler layer, which consumes it for the
  inline exemption. `EnumPasswordChangeOrigin` and `EnumSessionRevokeReason` live in
  `Identity/Domain/Enums/`.
- `UserVerifiedEvent` is raised only on the actual unverified → verified transition, so
  verifying an already-verified account (and the password-reset OTP path, which also calls
  `MarkAsVerified`) never re-sends the welcome email — matching the previous purpose-gated
  behavior by construction.
- The event-raising and silent seams are named, not overloaded: `UserEntity.UpdatePassword`
  always raises and `UserEntity.InitializePasswordHash` never does (the initialization path that
  gives an account a known credential outside a user-facing flow), and `UserRoleEntity.Create`
  always raises while `UserRoleEntity.CreateBootstrap` never does (the signup visitor assignment
  and the seeders, silent per the audit's not-a-candidate ruling). An overload or an optional
  argument would have made two different security behaviors indistinguishable at the call site.
- Where one fact feeds email and in-app, a single `…NotificationsHandler` serves both channels
  (spec 02's allowed shape) because the channels share every lookup: password changed, email
  changed, role granted, role revoked, signed-out-all-devices. Welcome stays email-only.
- The current-session exemption is implemented as an optional `exemptSessionId` parameter on
  `ISessionRepository.DeleteAllByUserIdAsync`, which also gained the revoke `reason`;
  `RevokeAsync` carries the reason too. The parameters default (`SelfSignOut`, no exemption)
  so existing call sites and tests remain source-compatible.
- Replay detection: when the presented refresh token matches no valid session,
  `RefreshTokenFactory` looks the hash up among revoked sessions
  (`GetRevokedSessionByRefreshTokenHashAsync`). A match means a deliberately invalidated
  credential was presented again; the session records the replay, the commit publishes the
  event, and the refresh attempt is still rejected. Rotation overwrites the stored hash, so a
  replay of a rotated-but-unrevoked token remains undetectable without storing hash history —
  recorded as the known limit of this wave. `RefreshTokenReplaySecurityHandler` revokes every
  session of the account and sends the new `RefreshTokenReplayAlert` template (added with
  neutral/en/fr resources), interpreting the session family at account level; per this spec's
  one-raise-site-one-handler decision it owns both the revocation and the alert.
- `SessionRevokedEvent` has a single v1 consumer, `SessionRevokedLogHandler` (Information log)
  — the audit-ready slot. `SessionCreatedEvent`/`SessionReactivatedEvent` land with no
  registered consumer; the login-alert handler stays gated on its follow-up decision.
- The admin-email-change integration proof could not be written as specced: no admin endpoint
  mutates an email today (`AdminUpdateOwnProfile` deliberately excludes email). The gap is
  still closed by construction — any future caller of `UserEntity.UpdateEmail` fans out to the
  dual emails, the in-app row, and the invalidation — and the dual-email behavior is proven
  end-to-end through the public profile flow in
  `tests/Integration/Workflows/IdentitySecurityEventFlowTests.cs`.
- The 17 inline `IMailer` hooks minus the five OTP files were deleted with their constructor
  parameters and mailer mocks; the role handlers' post-commit user re-fetches are gone (the
  event carries the role name; handlers re-resolve the recipient through
  `IUserLookupService`). `PublicSetPasswordCommand` gained the acting `SessionId` from claims
  so the set-password flow participates in the exemption.
- `RefreshTokenFactory` wraps its replay detection in a try/catch: the detection is a reaction to
  a rejection, not part of deciding it, so a lookup or commit failure is logged and the caller
  still receives the localized invalid-refresh-token rejection instead of a 500.
