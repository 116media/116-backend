# 07 — Identity & Security

Scope: `src/Modules/Identity` — auth, users, roles, permissions, sessions, OTPs — read domain
and infrastructure in full plus a deep application sample.

Because this module is the authentication boundary, security is treated as a first-class
architectural concern. The refresh-token handling, credential-change session revocation, and
JWT validation are genuinely correct. But there is a **critical unauthenticated account-takeover**
in social-login, a designed-but-unenforced permission system, and sign-out that revokes the
session row while the access token keeps working. Findings are split into SECURITY and
ARCHITECTURE.

---

# SECURITY

## S1 — `social-login` trusts a client-supplied email: unauthenticated takeover of every social account

**Severity: Critical** · the same endpoint's SSRF is [05 §1](05-core-and-mailer.md).

**Where:** `PublicSocialLoginEndpointV1.cs:22` — anonymous `PublicSocialLoginRequest(string Email,
string UserName, string? AvatarUrl, string Provider)`. `AuthRepository.cs:292-351` looks the user
up by email, and on `NotFoundException` calls `UserEntity.CreateExternal` which sets `IsVerified =
true`. There is **no provider-token verification anywhere** — no `id_token` field, no
`GoogleJsonWebSignature`, no Facebook debug_token, no provider SDK. `SessionFactory` then issues a
full JWT + refresh token.

**Problem/why.** `POST /api/v1/public/auth/social-login` with `{"email":"victim@gmail.com",
"provider":"Google"}` returns a valid token pair for the victim — no proof of possession of the
Google/Facebook account is ever requested. Two attacks: (a) full takeover of any account whose
provider is Google/Facebook; (b) pre-registration squatting — create a *pre-verified* account for
an email you don't own, permanently blocking the real owner from signing up. The `AuthProvider ==
Local` guard only protects password accounts; it is not an authentication step.

**Solution.**
1. Change the request to `(string Provider, string IdToken)` — never accept `Email`/`UserName`/
   `AvatarUrl` from the client.
2. Add `IExternalTokenVerifier.VerifyAsync(provider, idToken, ct)` returning
   `(ProviderSubjectId, Email, EmailVerified, Name, PictureUrl)`; implement Google
   (`GoogleJsonWebSignature.ValidateAsync` with the client-id audience) and Facebook (debug_token).
3. Reject `EmailVerified == false`.
4. Add `ProviderSubjectId` to `UserEntity` with a unique `(AuthProvider, ProviderSubjectId)` index;
   match subject-id first, email second — an email match with a mismatched subject id is rejected.
5. Deploy backend + clients together (breaking contract change).

---

## S2 — Sign-out revokes the session row but the access token keeps working (6 of 238 endpoints re-check)

**Severity: High**

**Where:** `PublicSignOutSessionFactory` only calls `RevokeAsync` + clears cookies (a no-op for
mobile). The only revocation check, `IsSessionValidAsync`, is called from exactly 6 handlers.
`AccountStatusRequirementHandler` checks `IsVerified`/`IsActive` but never looks at the session;
JWT config has no revocation hook. Access-token lifetime is 60 minutes.

**Problem/why.** A stolen access token stays valid for up to 60 minutes after the victim signs out,
signs out all devices, or an admin force-logs-out — on 232 of 238 authenticated endpoints. Every
"revoke and alert" reaction is neutered for that window: the refresh token dies, the access token
doesn't.

**Solution.** Add a `SessionActiveRequirement` + handler reading the `SessionId` claim through an
`ISessionRevocationCache` (`IMemoryCache`, TTL ≤ token lifetime, invalidated from the existing
`SessionRevokedLogHandler`). Add the requirement to every policy in one place
(`AuthorizationExtensions`), so all 238 endpoints inherit it without touching endpoint files. Drop
the 6 hand-rolled checks. Cut the access-token lifetime to 15 min.

---

## S3 — The 28-permission model is generated, stored, seeded, and signed into every JWT — and checked by zero endpoints

**Severity: High**

**Where:** `VisitorPermissions.cs` builds 28 permission rows; `VisitorRoleSeeder` persists them;
`JwtService` serializes them into a `permissions` claim. Enforcement census across all modules:
endpoints checking a permission = **0**; checking a role = 231; account-status only = 7; anonymous
= 55. `grep JwtClaimsConstants.Permissions` → 1 hit (the writer). No `RequirePermission`,
`PermissionRequirement`, or `HasPermission` anywhere.

**Problem/why.** Authorization is role-only in practice. The 20 role/permission-management admin
endpoints mutate rows that change nothing — a SuperAdmin revoking `comments.create` from Visitor
sees the DB and JWT change while the endpoint keeps serving. Granularity is impossible: anyone who
can reach an endpoint can do everything it does; there is no read-only admin. The permission array
still ships to clients, so the frontend may hide UI the backend does not enforce.

**Solution.** Either **enforce** — add `PermissionRequirement(resource, action)` + handler parsing
the claim (treating `system:all` as wildcard), `PermissionPolicies` constants generated from the
same list, and a `.WithAuthorization` line on the 238 endpoints in slices — with the caveat that
permission changes must revoke the affected users' sessions (permissions ride the token). Or
**retreat honestly** — delete `VisitorPermissions`, the 20 management endpoints, the claim, and
`PermissionDto`, and document that authorization is role-based. Do not leave it half-built.

---

## S4 — Password-reset OTP is never consumed: the same code resets the password repeatedly for 60 minutes

**Severity: High**

**Where:** `OtpRepository.ValidateUsedOtpAsync` returns the matching OTP but never marks it used/
invalidated. `PublicResetPasswordHandler`/`AdminResetPasswordHandler` call it then reset the
password without invalidating the OTP. `OtpExpirationMinutes = 60`.

**Problem/why.** The reset flow is forgot → verify-otp (marks used) → reset (requires a used OTP).
Since reset leaves the row untouched, the code remains a valid reset credential for the rest of the
60-minute window. Anyone who observes it once — shoulder-surf, forwarded email, mail-gateway log —
can keep resetting the password after the legitimate reset, in a loop (and each reset revokes all
sessions, locking the victim out).

**Solution.** Add `ConsumeAsync`/`MarkAsConsumed` and call it between `ValidateUsedOtpAsync` and the
password write, in the same transaction. Cut `OtpExpirationMinutes` to 10.

---

## S5 — OTP attempt limiting is defeated by `resend-otp`; effective space is 3 guesses per resend

**Severity: High**

**Where:** `MaxOtpAttempts = 3`, `OtpCodeLength = 6`. `PublicResendOtpFactory` invalidates the old
OTP and creates a new row whose `AttemptCount` starts at 0 — the counter is per-row, not
per-account. `resend-otp` is anonymous with the (global) `Otp` policy. No lockout.

**Problem/why.** An attacker loops `resend-otp` → 3× `verify-otp`; nothing on the account degrades
between rounds. Sustained at the ceiling this is ~3 guesses/min against 10⁶, the account never locks
and never alerts, and combined with S6 an attacker who saturates the window also denies OTP to every
other user. A `PasswordReset` guess then feeds S4.

**Solution.** Move the counter onto the account (`OtpFailedAttempts` + `OtpLockedUntil` on
`UserEntity`); cap resends per account per window; cut expiry to 10 min; raise the code to 8 digits.

---

## S6 — Rate limiters are global singletons, not per-caller (5 logins/min for the whole application)

**Severity: High** · same finding as [01 §1.1](01-composition-root-and-shared-kernel.md),
[08 §1](08-cross-cutting.md).

**Where:** the three RateLimit builders use the non-partitioned overloads with no partition key.

**Problem/why.** One client issuing 5 logins/min consumes the entire app's login budget — everyone
else gets 429. `PasswordManagement` (3 per 5 min globally) lets a single attacker permanently
disable password reset for all users at ~1 request/100s. And it provides no actual per-attacker
limiting.

**Solution.** Partition on authenticated subject then client IP; additionally partition
`Authentication`/`Otp`/`PasswordManagement` on the target account. Fix the forwarded-headers trust
(S… / [08 §20](08-cross-cutting.md)) first so the IP key isn't attacker-controlled.

---

## S7 — Login, forgot-password and resend-OTP disclose whether an account exists — and whether it is an admin

**Severity: Medium**

**Where:** `PublicLoginAuthFactory` throws `NotFoundException` → **404** for an unknown account vs
`InvalidCredentials` → 401 for a wrong password (the 404 is even in `.Produces`). The unknown-account
branch returns *before* the 25,000-round PBKDF2 verify — a timing channel. `AdminForgotPasswordHandler`
returns 200 for unknown but 403 for a known non-admin — a *role* oracle.

**Problem/why.** `POST /public/auth/login` is an account-existence oracle. `POST
/admin/auth/forgot-password` is a role oracle (200 = no account, 403 = non-admin, 200-sent =
admin), handing an attacker the exact list of privileged accounts to target with S1/S5/S6.

**Solution.** Use a non-throwing lookup and run a dummy `Verify` against a constant hash when the
user is null (same 401, same work). Remove `.Produces(404)`. Make the admin forgot/resend/reset
paths return the same success shape as the unknown-email branch; log the real reason server-side.

---

## S8 — Signup issues a full session to an unverified account; `RequireVerifiedUser` is on zero endpoints; 88 authenticated endpoints don't check `IsActive`

**Severity: High**

**Where:** `PublicSignUpHandler` calls `CreateSessionAsync` immediately after registration and
returns tokens; `UserEntity.Create` leaves `IsVerified = false`. `RequireVerifiedUser` has 0
endpoint usages. 88 authenticated endpoints carry no account-status policy at all.
`RequireAdminOnly` has 0 usages.

**Problem/why.** A throwaway address you never confirm yields a working Visitor token that opens all
49 `RequireVisitorOnly` endpoints — email verification is a formality nothing enforces. On the 88
status-policy-less endpoints, a deactivated account keeps working for the token lifetime and (S11)
gets refreshed indefinitely.

**Solution.** Stop issuing tokens at signup — return the user + a "verify your email" flag; the
client logs in after `verify-otp`. Fold `RequireVerifiedUser` into `RequireVisitorOnly` and
`RequireActiveUser` into every policy centrally. Delete or apply `RequireAdminOnly`.

---

## S9 — Anonymous content endpoints leak staff email addresses

**Severity: Medium**

**Where:** `Identity.Contracts.AuthorInfo.Email` is populated from `user.Email`; `ShortVideoMapper`
and `LyricsMapper` put it on `AuthorDto`, reached from `.AllowAnonymous()` endpoints. The comment
paths correctly pass `Email: null`; the mappers never got the same treatment.

**Problem/why.** `GET /api/v1/public/shorts` unauthenticated returns `author.email` alongside
`author.role` — pairing `"role":"SuperAdmin"` with that admin's address. A targeting list for
S1/S5/S7, published to anyone.

**Solution.** Drop `Email` from `AuthorInfo` and from the content `AuthorDto`; give the five
mail-sending event handlers a separate `GetEmailByIdAsync` so the address is fetched only where it is
mailed. Fix the four mapper call sites.

---

## S10 — Weak password stretching and no lockout: PBKDF2-SHA256 at 25,000 iterations, 6-char minimum

**Severity: Medium**

**Where:** `PasswordService.Iterations = 25000`; `MinPasswordLength = 6`; complexity satisfied by
`Abc123`. No lockout anywhere (`grep FailedLoginAttempts|LockoutEnd` → 0). (The salt is per-hash
CSPRNG, the format is version-prefixed, and `Verify` is `FixedTimeEquals` — those are right.)

**Problem/why.** OWASP guidance is 600,000 iterations; 25,000 runs an offline attack on a leaked
table ~24× faster than it should. With a 6-char floor, a large share is recoverable. Online, a
correct guess is never slowed by the account locking — only by the shared global bucket.

**Solution.** Raise to 600,000 with a `v2:` prefix and lazy re-hash on next login (keep `v1:` at
25k for existing hashes). Give OTPs a separate cheap `IOtpHasher` (HMAC + pepper) so a 600k×2 cost
isn't paid per OTP check. Raise the minimum to 12 chars. Add `FailedLoginAttempts`/`LockedUntil` to
`UserEntity` (the same fields S5 needs).

---

## S11 — Refresh never re-checks account state, and rotation extends the session's expiry without bound

**Severity: Medium**

**Where:** `RefreshTokenFactory.RefreshTokenAsync` validates only that a session matches the token
hash — no `IsUserAccountActive`/`ValidateCanLogin`. Every refresh resets `ExpiresAt` to now + 30
days. (Rotation, hashing at rest, and replay detection are all correct.)

**Problem/why.** A deactivated account keeps refreshing forever — nothing invalidates sessions on
deactivate, and the `isActive:false` claim only helps on the 150 status-checked endpoints (S8). And
because every refresh resets `ExpiresAt`, a session used at least monthly never expires — the 30-day
bound is on idleness, not age, so a compromised refresh token is a permanent credential.

**Solution.** Call `session.User.ValidateCanLogin(...)` on refresh (the user is already `Include`d,
so it's free). Add `AbsoluteExpiresAt` (now + 30 days at create) and cap `ExpiresAt` at
`Min(now + window, AbsoluteExpiresAt)`. Add a deactivate-user use case that revokes sessions in the
same transaction.

---

## S12 — `AccountStatusRequirementHandler` hits the DB per request and silently degrades to unverified JWT claims

**Severity: Medium**

**Where:** `AccountStatusRequirementHandler.cs:47-66` loads the user per request (150 endpoints) and,
on `IsDbConnectivityError`, falls back to the token claim. `IsDbConnectivityError` returns true for
`TaskCanceledException`/`OperationCanceledException` — ordinary client disconnects.

**Problem/why.** Under any DB slowness the handler trusts the token's stale `is_active`, so a user
deactivated ten minutes ago regains access for the incident's duration. Plus one extra `SELECT` per
request on 150 hot endpoints with no caching.

**Solution.** Narrow `IsDbConnectivityError` to the Npgsql SQLSTATEs and `TimeoutException`; let
cancellation propagate. Decide the fallback deliberately (fail closed with 503, or fall back only
for `RequireActiveUser`). Cache the lookup in `IMemoryCache` (30–60s TTL, invalidated on the
relevant events) — the same cache S2 needs.

---

# ARCHITECTURE

## A1 — `IAuthRepository` is a god interface mixing persistence, business validation, authorization, and claim parsing

**Severity: High**

**Where:** `IAuthRepository : IRepository<UserEntity>, IClaimsProvider` — 18 members: 7 persistence,
4 uniqueness rules, 4 authorization decisions that throw (`IsUserAdmin` throws 403; `IsSessionValidAsync`
queries the sessions table from the *user* repository), 2 `ClaimsPrincipal`-parsing. Registered under
two interfaces.

**Problem/why.** An ASP.NET hosting type (`ClaimsPrincipal`) reaches the Application layer through a
persistence port, so "current user" is not abstracted. Authorization is invisible — `IsUserAdmin`
looks like a boolean read but throws 403 (this is how the S7 enumeration got written). Every handler
needing one lookup depends on all 18 members.

**Solution.** Introduce `ICurrentUser` (in `Identity.Contracts`) over `IHttpContextAccessor`; delete
`IClaimsProvider` and the `ClaimsPrincipal` parameter from the 98 call sites. Split `IAuthRepository`
into `IUserRepository` + `IUserUniquenessChecker`. Delete the throwing authorization methods (state
is `ValidateCanLogin`; admin-ness is a policy). Move `GetOrCreateExternalUserAsync` (which self-commits)
into the social-login factory.

---

## A2 — `UserEntity` is a god aggregate: credentials, profile, roles, and sessions in one root

**Severity: Medium**

**Where:** `UserEntity` carries credentials + profile + `ICollection<UserRoleEntity>` +
`ICollection<SessionEntity>` — both of which are themselves aggregate roots. `RecordMassSignOut`
raises an event but changes no state ("the session rows are revoked by the caller in the same
transaction").

**Problem/why.** `SessionEntity` is a root that is *also* a child collection — two consistency
boundaries over the same rows. The consequence: 8 handlers pair "mutate user + revoke sessions"
by hand; miss one and the invariant breaks. `RecordMassSignOut` is a pure event-emitter on an
aggregate that owns none of the state the event describes.

**Solution.** Remove the `Sessions` collection (reach sessions via `ISessionRepository`). Replace the
8 pairings with an `IAccountSecurityService.InvalidateOtherSessionsAsync(...)` so the pairing exists
once; move `RecordMassSignOut` there. Extract `UserProfile` as an owned value object (no schema
change).

---

## A3 — Seeding runs synchronously on every startup with check-then-act races and a hardcoded SuperAdmin identity

**Severity: High** · overlaps [01 §1.12/§1.13](01-composition-root-and-shared-kernel.md),
[04 §9](04-content-infrastructure.md).

**Where:** `IdentityModule.cs:249` seeds via `.GetAwaiter().GetResult()`, `EnableSeeding =
!Testing` (on in Production). `VisitorRoleSeeder` is check-then-act with no transaction against a
unique index; a racing instance throws and the process fails to start. `SuperAdminConfiguration`
hardcodes `superadmin@116.com` / `sigmacool`, password from `DEFAULT_USER_PASSWORD`.

**Problem/why.** Rolling deploys / replicas > 1 make startup nondeterministic — the loser crashes on
a unique-constraint violation and CrashLoopBackOffs. The SuperAdmin login is public knowledge with a
password from a single env var typically shared across environments, and nothing rotates or expires
it — that one variable is the whole admin boundary. `VisitorRoleSeeder` also skips entirely if the
role exists, so a 29th permission needs a manual migration.

**Solution.** Move seeding to an `IHostedService`, `await`-ed, under a `pg_advisory_xact_lock`. Make
the seeder converge (upsert each permission) rather than skip. Source the SuperAdmin identity from
config, gate it on non-Production (ship a one-shot `seed-superadmin` command for Production), and add
a `MustChangePasswordAt` flag.

---

## A4 — Identity is an older generation of the slice pattern than Content — four concrete divergences

**Severity: Medium**

**Where:** (a) 15 single-caller `I*AuthFactory` pass-throughs registered individually (the
`Public`/`Admin` OTP/sign-out factory pairs are byte-identical); (b) two error styles — the
`IdentityI18n` facade (73 files) and raw `UserErrors` (19), including inside Domain
(`UserEntity.Create(..., UserErrors errors)`, `VisitorPermissions` imports `Application.Shared.Errors`);
(c) repository interfaces scattered (`Auth/Repositories/`, `Session/Repositories/`) vs Content's
`Shared/Repositories/`; (d) value objects with a throwing `implicit operator T(string)` that surfaces
as a 500 (`ArgumentException` is unmapped).

**Problem/why.** The pass-throughs double the file count per use case and drift — the `Public`/`Admin`
OTP pairs must be edited in lockstep, which is how the S7 asymmetry survived. The Domain→Application
dependency prevents unit-testing the domain without the localization stack. The throwing implicit
operators convert validation failures into 500s.

**Solution.** Delete the pass-through factories (fold into handlers). Collapse identical `Public`/
`Admin` variants to one scope-parameterised class. Replace `UserErrors` in the domain with domain
exceptions ([03 §6](03-content-domain.md) pattern). Move the repository interfaces to
`Shared/Repositories/`. Remove the throwing implicit operators; parse in the validator so failures
are 400s.

---

## A5 — OTP purpose is honoured in the lookup but not in the effect — verifying a reset code marks the account email-verified

**Severity: Medium**

**Where:** `PublicVerifyOtpHandler`/`AdminVerifyOtpHandler` call `user.MarkAsVerified()`
**unconditionally**, for every purpose. `Purpose` is a free-text string from the body.
`TwoFactorAuthentication`/`AccountRecovery` are reachable enum values with no flow (resend mints
codes for them; verify accepts one). (The lookup itself is correctly purpose-scoped.)

**Problem/why.** Completing a password reset silently marks the account email-verified even if the
address was never confirmed — which matters because verification is the module's only legitimacy gate
(S8). The two flowless purposes can be minted and verified to set `IsVerified = true`.

**Solution.** Split `verify-otp` into purpose-specific use cases (`VerifyEmailOtp` marks verified;
`ValidatePasswordResetOtp` returns a short-lived single-use ticket the reset flow consumes — which
also removes S4's "must be already used" contract). Delete the two flowless enum values.

---

## What is done well here

- **Refresh-token handling is genuinely correct** — 256-byte CSPRNG tokens, SHA-256 at rest with the
  raw value never stored, rotated on every use, with reuse detection that revokes every session and
  emails the owner. The replay-recording is wrapped so a failure to record can't turn rejection into
  a 500.
- **OTP codes are hashed at rest** with a per-code salt; generation uses `RandomNumberGenerator`.
- **Password verification is timing-safe** and version-prefixed, so the iteration upgrade (S10) needs
  no forced reset.
- **Credential changes revoke sessions in the same transaction** — change/reset/set-password, email
  change, and role grant/revoke each pair the write with session revocation before one commit, with
  `exemptSessionId` correctly sparing the acting session where appropriate. The reasoning is written
  down.
- **JWT validation is strict** — issuer, audience, lifetime and key all validated with `ClockSkew =
  Zero`.
- **Ownership checks are consistent — no IDOR found.** All 98 `GetUserIdFromClaims` sites take the id
  from the authenticated principal; no endpoint accepts a user id from body/route for an own-resource
  operation; session lookups return "not found" on a mismatch to avoid an enumeration oracle.
- **No password hash, token, or refresh token appears in any response DTO.**
- **Token delivery is client-aware** — HttpOnly + SameSite=Strict + Secure cookies for web, body
  tokens for mobile, with the refresh cookie path scoped and the reasoning documented.
- **Domain invariants are well kept** — private setters, guarded factories, no-op-returning
  `MarkAsVerified`/`Activate`/`Deactivate`.
- **Specifications are composed, not duplicated** — `SessionIsActive` = `NotRevoked.And(NotExpired)`.
- **Doc comments carry design rationale** — several seams explain what is deliberately *not* wired yet
  ("the audit-ready slot"). Unusually high-signal for scaffolding.
