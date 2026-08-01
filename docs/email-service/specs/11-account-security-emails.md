# Spec 11 — Account Security Emails (Identity)

## Goal

Every security-relevant account change notifies the account owner. These are
the "if this wasn't you, act now" emails — they exist to make silent account
takeover impossible. Builds on the core pipeline (specs 01–05); consumers
follow the spec 06 pattern: inject `IMailer`, enqueue inside the existing
unit of work, never touch `IEmailSender`.

## New templates (append to `EnumEmailTemplate`, spec 04 rules apply)

| Template | Required tokens |
| --- | --- |
| `PasswordChanged` | `userName`, `changeTime` |
| `PasswordResetCompleted` | `userName`, `resetTime` |
| `LocalPasswordAdded` | `userName` |
| `EmailChangedAlertOld` | `userName`, `newEmailMasked`, `changeTime` |
| `EmailChangedConfirmNew` | `userName`, `changeTime` |
| `SignedOutAllDevices` | `userName`, `time` |
| `AccountForceLoggedOut` | `userName`, `time` |
| `RoleChanged` | `userName`, `roleName`, `action` (granted/revoked) |

All eight get neutral/en/fr resources; security emails share a distinct
"security notice" layout intro line in the shared layout.

## Hook table

| # | Event | Hook site | Recipient |
| --- | --- | --- | --- |
| 1 | Password changed (knew old password) | `PublicChangePasswordHandler` + `AdminChangePasswordHandler`, after `user.UpdatePassword(...)` | `user.Email` |
| 2 | Password reset completed via OTP | `PublicResetPasswordAuthFactory` + `AdminResetPasswordAuthFactory` | `user.Email` |
| 3 | Password set on a social account | `PublicSetPasswordHandler` (`SetPasswordAndChangeToLocal`) | `user.Email` |
| 4 | Email address changed | `PublicUpdateProfileAuthFactory`, inside the `isEmailUpdated` branch | **both**: `EmailChangedAlertOld` to the pre-change `user.Email` (capture it *before* `UpdateEmail` runs), `EmailChangedConfirmNew` to the new address |
| 5 | Signed out from all devices (self) | `PublicSignOutFromAllDevicesHandler` + admin variant | `user.Email` |
| 6 | Admin force-logout | `AdminForceLogoutUserHandler` | target user — handler currently loads no user; resolve via the auth repository (in-module, no lookup service needed) |
| 7 | Role granted / revoked | `AdminAssignRoleToUserHandler` / `AdminRemoveRoleFromUserHandler` | target user — same in-module resolution as 6 |

Rules that apply to every row:

- Enqueue joins the handler's existing commit — a rolled-back change sends
  nothing.
- `user.Email` is nullable (social accounts): null ⇒ skip silently, log at
  `Debug`. Never fail the auth operation over the notification.
- In `newEmailMasked`, mask the local part (`j***@gmail.com`) — the old
  address's mailbox may be compromised; don't hand it the new address in full.

Domain-events refactor note (docs/domain-events, spec 04): every hook site
in the table moved behind an identity domain event; the enqueues now run
post-commit in the module's `EventHandlers/` classes, which also write the
matching in-app notification rows. The business handlers above no longer
inject `IMailer`.

## Open product decision — email-change re-verification

Today `UpdateEmail` swaps the address with **no re-verification**: any
logged-in session can silently repoint the account. This spec ships the two
notification emails, which close the *silence*, not the *takeover path*. The
right fix is an OTP-to-new-address verification step before the swap
(reusing `EnumOtpPurpose.EmailVerification` machinery).

**Decision needed before implementation**: notifications only (this spec as
written), or notifications + the re-verification flow (extra command/handler
work in Identity). Record the choice here.

## Ordering note

`EmailChangedAlertOld` must render with the *old* address as recipient before
the aggregate mutates — capture `string? previousEmail = user.Email;` ahead of
`UpdateEmail`. The enqueue itself still happens in the same transaction; only
the captured value predates the mutation.

## Testing

Per spec 09 conventions:

- Unit: each handler/factory asserts the exact template + tokens enqueued
  (mocked `IMailer`); null-email skip paths; email-change asserts **two**
  enqueues with old/new recipients and the masked token.
- Integration (extend the existing endpoint tests): change-password /
  reset-password / set-password / update-profile-email / sign-out-all /
  force-logout / assign-role flows each persist the expected outbox row(s);
  rollback paths persist none.

## Checklist

- [x] Eight templates + resources (neutral/en/fr) appended
- [x] Hooks 1–7 enqueueing inside the existing unit of work
- [x] Old-address capture before mutation; masking applied
- [x] Null-email skip paths in every hook
- [x] Re-verification decision recorded and, if chosen, implemented
- [x] Unit + integration coverage green

## Implementation notes

- **Decision recorded: notifications only.** The email-change
  re-verification flow (OTP to the new address before the swap) is NOT
  implemented in this wave — the two notification emails ship, the takeover
  path itself is unchanged and stays an open follow-up.
- The old address is captured before `UpdateEmail` mutates the aggregate;
  the alert masks the new address (`f***@example.com`).
- Remove-role resolves the role name through `IRoleRepository` because the
  user-role lookup does not include the Role navigation.
