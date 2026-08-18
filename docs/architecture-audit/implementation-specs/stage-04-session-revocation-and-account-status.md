# Stage 4 — Token invalidation, session revocation, verified signup & account status

Closes **[07 S2]** (a revoked session's access token keeps working until it expires),
**[07 S8]** (signup hands a live session to an unverified account, and the 34 Visitor interaction
endpoints check only the *role* claim), and **[07 S11]** (refresh never re-checks account state and a
session can be slid forward forever). It also closes the broader **stale-claim** hole: a token keeps a
role/permission/active flag after it has been changed server-side, until it expires.

> **Breaking change.** `POST /api/v1/public/auth/signup` stops returning tokens (returns a
> "verify your email" result), and every access token now carries two new claims (`sstamp`, `tver`) —
> tokens minted before this ships are rejected and must re-authenticate. See [Rollout](#rollout).

> **Depends on Stage 3** (branch stacks on the tree Stage 3 lands on).

---

## Design — trust the token, invalidate on change

Steady-state authorization reads **only the signed JWT claims** (role, permissions, `is_active`,
`is_verified`) — **no per-request DB lookup** on hot paths (likes, comments, admin reads). Claims are
tamper-proof but stale; freshness is guaranteed by **three invalidation markers** checked once per
request in the JWT `OnTokenValidated` event:

| Marker | Scope | Claim | Bumped when | Effect on mismatch |
| --- | --- | --- | --- | --- |
| session denylist | one **session** | `ref` (session id) | sign-out-this-device, admin revoke one session | that session's token rejected → 401 |
| `TokenVersion` (long) | whole **user** | `tver` | role / permission change | access token rejected, **but refresh still works** → silent re-issue with fresh claims |
| `SecurityStamp` (guid) | whole **user** | `sstamp` | password reset, deactivate, email change | rejected **and** sessions revoked → **forced re-login** |

The difference between the two user-level markers is the remediation: `tver` is *refresh-recoverable*
(the client silently mints a new token with the current permissions), while `sstamp` also revokes the
sessions, so the user must log in again. Enforcement is cache-backed, so the common case is an O(1)
memory lookup, not a query.

> **In-process for now.** Both the denylist and the per-user security-state are `IMemoryCache`, so on
> a multi-instance deployment a change on node A isn't seen by node B until Stage 9 swaps them for the
> Redis-backed implementations behind the same interfaces. Single-instance today, this closes every
> window fully.

---

## Checklist

- [x] 4.1 — `UserTokenStateEntity` table (guid `SecurityStamp` + long `TokenVersion`) + repo with atomic bumps; row created with the user; migration
- [x] 4.2 — `JwtService` emits `sstamp` + `tver` from the loaded state; `JwtClaimsConstants.SecurityStamp`/`TokenVersion`
- [x] 4.3 — `ISessionRevocationCache` (per-session denylist) + `IUserSecurityStateCache` (reads through the token-state repo)
- [x] 4.4 — `OnTokenValidated` rejects a revoked session, an `sstamp` mismatch, or a `tver` mismatch
- [x] 4.5 — Bump wiring: role/permission change → `tver`; password/deactivate/email → `sstamp` + revoke sessions; sign-out → denylist
- [x] 4.6 — Verified signup: `PublicSignUpHandler` stops issuing tokens; endpoint returns a verify-email result
- [x] 4.7 — Interactions: claim-based `is_active` + `is_verified` (no per-request DB); freshness via §4.5
- [x] 4.8 — Refresh re-reads current stamp/version into the new token; deactivated → refused; `AbsoluteExpiresAt` cap + config + migration
- [x] 4.9 — Unit + integration tests
- [x] 4.10 — Verify (build 0/0, unit green; run integration locally)

---

## Part A — Token invalidation markers & enforcement `[07 S2]` + stale-claim

### 4.1 `UserTokenStateEntity` — a dedicated per-user invalidation record

The two markers exist only to invalidate tokens, so they live in their **own 1:1 table** rather than
widening the `UserEntity` aggregate. This keeps a bump a cheap, isolated, **atomic** write (no
read-modify-write on the user row, so concurrent role changes can't lose an increment), and makes the
`OnTokenValidated` cache-miss read a narrow two-column query.

Entity — `src/Modules/Identity/Identity/Domain/Entities/UserTokenStateEntity.cs` (new); its `Id` **is** the
user id (1:1):

```csharp
public class UserTokenStateEntity : Aggregate<Guid>
{
    /// <summary>
    /// Rotated on identity/credential changes (password reset, deactivation, email change). A token
    /// whose <c>sstamp</c> no longer matches is rejected; rotation also revokes sessions, so the user
    /// must log in again.
    /// </summary>
    public Guid SecurityStamp { get; private set; }

    /// <summary>
    /// Incremented on authorization changes (role/permission grant or revoke). A token whose
    /// <c>tver</c> is older than the current value is rejected, but a refresh silently mints one with
    /// the current value and claims.
    /// </summary>
    public long TokenVersion { get; private set; }

    private UserTokenStateEntity() { }

    /// <summary>
    /// Creates the invalidation record for a user; call in the same unit of work as the user.
    /// </summary>
    public static UserTokenStateEntity Create(Guid userId) =>
        new()
        {
            Id = userId,
            SecurityStamp = Guid.NewGuid(),
            TokenVersion = 0,
        };
}
```

Because bumps are atomic SQL updates (below), the entity is otherwise read-only — no
`Rotate`/`Bump` methods that would require loading it first.

Repository — `IUserTokenStateRepository` (Application) + implementation (Infrastructure):

```csharp
public interface IUserTokenStateRepository
{
    /// <summary>
    /// Adds the record for a newly created user (same unit of work).
    /// </summary>
    Task AddAsync(UserTokenStateEntity state, CancellationToken cancellationToken);

    /// <summary>
    /// The current stamp/version projection, or null if the row is missing.
    /// </summary>
    Task<UserSecurityState?> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically increments the token version (no read-modify-write).
    /// </summary>
    Task BumpTokenVersionAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically rotates the security stamp to a fresh value.
    /// </summary>
    Task<Guid> RotateSecurityStampAsync(Guid userId, CancellationToken cancellationToken);
}
```

- `BumpTokenVersionAsync` → `ExecuteUpdateAsync(s => s.SetProperty(x => x.TokenVersion, x => x.TokenVersion + 1))`.
- `RotateSecurityStampAsync` → generates a new guid and `ExecuteUpdateAsync(s => s.SetProperty(x => x.SecurityStamp, newStamp))`, returning it.
- `GetAsync` → `SELECT security_stamp, token_version` by primary key.

Create the record wherever a `UserEntity` is persisted — the signup (`PublicSignUpAuthFactory.RegisterAsync`),
social-login (`GetOrCreateExternalUserAsync`) and SuperAdmin-seeding (`SuperAdminSeedingStrategy`) paths all
add `UserTokenStateEntity.Create(user.Id)` in the same commit. `GetOrCreateAsync` (below) then stays a
safety net for rows the backfill missed rather than the mechanism a creation path relies on.

**Migration** `AddUserTokenState` — creates `user_token_state` (`user_id uuid` PK + FK → `users`,
`security_stamp uuid NOT NULL`, `token_version bigint NOT NULL DEFAULT 0`, auditable columns), and
backfills one row per existing user (`INSERT ... SELECT id, gen_random_uuid(), 0 FROM users`). Add
`UserTokenStateConfiguration` and the `DbSet<UserTokenStateEntity>` to `IdentityDbContext`. Leave unapplied.

### 4.2 Emit the claims

`JwtClaimsConstants` gains `SecurityStamp = "sstamp"` and `TokenVersion = "tver"`. The session and
refresh factories load the user's `UserTokenStateEntity` (via `IUserTokenStateRepository.GetAsync`) and pass
the `(SecurityStamp, TokenVersion)` into `JwtService.GenerateToken(...)`, which adds both to the
access-token claims it already builds (next to `is_active`, `is_verified`, `ref`):

```csharp
new(JwtClaimsConstants.SecurityStamp, tokenState.SecurityStamp.ToString()),
new(JwtClaimsConstants.TokenVersion, tokenState.TokenVersion.ToString()),
```

### 4.3 The caches

**Session denylist** — `ISessionRevocationCache` (Application) + in-process `SessionRevocationCache`
(Infrastructure), a presence set of revoked session ids, TTL = access-token lifetime, self-trimming:

```csharp
public interface ISessionRevocationCache
{
    void Revoke(Guid sessionId, TimeSpan ttl);
    bool IsRevoked(Guid sessionId);
}
```

```csharp
public sealed class SessionRevocationCache(IMemoryCache cache) : ISessionRevocationCache
{
    private static string Key(Guid sessionId) => $"session-revoked:{sessionId}";
    public void Revoke(Guid sessionId, TimeSpan ttl) => cache.Set(Key(sessionId), true, ttl);
    public bool IsRevoked(Guid sessionId) => cache.TryGetValue(Key(sessionId), out _);
}
```

**Per-user security state** — `IUserSecurityStateCache`: the current `(SecurityStamp, TokenVersion)`
for a user. Read-through (loads from the DB on a miss, caches for a short TTL) and evicted the
moment a marker is bumped, so a single-instance host is immediately consistent:

```csharp
namespace _116.Identity.Application.Shared.Cache;

public readonly record struct UserSecurityState(Guid SecurityStamp, long TokenVersion);

public interface IUserSecurityStateCache
{
    /// <summary>
    /// Current stamp/version for the user, loaded from the DB on a cache miss.
    /// </summary>
    Task<UserSecurityState> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Overwrites the cached state after a marker is bumped.
    /// </summary>
    void Set(Guid userId, UserSecurityState state);
}
```

The `IMemoryCache`-backed implementation keys on `user-security:{userId}`, uses a short sliding TTL
(a few minutes), and calls `IUserTokenStateRepository.GetAsync(userId)` (the narrow
`SELECT security_stamp, token_version` from §4.1) on the miss path.

Register in `IdentityModule.cs`: `AddSingleton<ISessionRevocationCache, SessionRevocationCache>()`,
`AddSingleton<IUserSecurityStateCache, UserSecurityStateCache>()`, and
`AddScoped<IUserTokenStateRepository, UserTokenStateRepository>()`.

### 4.4 Enforce in `OnTokenValidated`

One hook, at authentication time, before authorization — covers every authenticated request
(including bare `[Authorize]`) without touching any policy. Add it to the existing
`ConfigureJwtBearerEvents` in `AuthorizationExtensions`:

```csharp
OnTokenValidated = async context =>
{
    ClaimsPrincipal? principal = context.Principal;

    if (
        !Guid.TryParse(principal?.FindFirst(JwtClaimsConstants.SessionId)?.Value, out Guid sessionId)
        || !Guid.TryParse(principal.FindFirst(JwtClaimsConstants.UserId)?.Value, out Guid userId)
        || !Guid.TryParse(principal.FindFirst(JwtClaimsConstants.SecurityStamp)?.Value, out Guid tokenStamp)
        || !long.TryParse(principal.FindFirst(JwtClaimsConstants.TokenVersion)?.Value, out long tokenVersion)
    )
    {
        // Missing/garbled markers → a pre-migration or tampered token. Treat as unauthenticated.
        context.Fail("The token is missing required session/security claims.");
        return;
    }

    IServiceProvider services = context.HttpContext.RequestServices;

    if (services.GetRequiredService<ISessionRevocationCache>().IsRevoked(sessionId))
    {
        context.Fail("The session has been revoked.");
        return;
    }

    UserSecurityState current = await services
        .GetRequiredService<IUserSecurityStateCache>()
        .GetAsync(userId, context.HttpContext.RequestAborted);

    if (tokenStamp != current.SecurityStamp)
    {
        context.Fail("Credentials changed; re-authentication required.");
        return;
    }

    if (tokenVersion < current.TokenVersion)
    {
        context.Fail("Permissions changed; token refresh required.");
    }
},
```

`context.Fail(...)` makes the request unauthenticated, so `OnChallenge` returns the standard 401
ProblemDetails. Use `<` for `tver` (older = stale) so a value can never accidentally re-match. The
denylist is a singleton; the security-state cache is a mostly-hit memory lookup — the DB is touched
only on a cold miss (once per user per TTL window), never per request.

> The `tver` and `sstamp` mismatches both return 401. The client distinguishes them by outcome: a
> `tver`-stale token refreshes silently (the refresh token is a session credential, not a bearer
> token, so it isn't subject to this hook), whereas an `sstamp`-stale token also has its session
> revoked (§4.5), so the refresh is rejected and the user must log in.

### 4.5 Bump wiring

Each security-relevant mutation bumps the right marker through the repo's atomic update, which
evicts the affected users from the cache:

| Change | Marker | Also |
| --- | --- | --- |
| Grant/revoke a **role** or **permission** on a user | `BumpTokenVersionAsync(userId)` | evict via `IUserSecurityStateCache.Remove` |
| **Password** reset/change, **deactivate**, **email** change | `RotateSecurityStampAsync(userId)` | revoke the user's sessions; evict the cached state |
| **Sign out** one device / admin revoke one session | — | `session.Revoke(...)` (denylist via §4.3, already wired by `SessionRevokedLogHandler`) |

Concretely:
- Role/permission handlers (`AssignRoleToUser`, `RemoveRoleFromUser`, role↔permission changes that
  affect a user's effective permissions) call `IUserTokenStateRepository.BumpTokenVersionAsync` for
  each affected user.
- Password (`ResetPassword`/`ChangePassword`/`SetPassword`), deactivate-user, and email-change flows
  call `RotateSecurityStampAsync` and revoke the user's sessions (each raises `SessionRevokedEvent`
  → denylist).
- `SessionRevokedLogHandler` (already the `SessionRevokedEvent` slot) adds the session id to the
  denylist with `ttl = access-token lifetime`:

```csharp
revocationCache.Revoke(
    domainEvent.SessionId,
    ttl: TimeSpan.FromMinutes(AppEnvironment.Jwt().accessTokenExpiration ?? JwtClaimsConstants.DefaultExpiration)
);
```

> Because the bumps are atomic `ExecuteUpdateAsync` calls (no tracked entity, so no domain event),
> the invalidation lives in the repository methods themselves: each bump evicts the affected users via
> `IUserSecurityStateCache.Remove`, so every caller stays consistent without remembering to do it.
> Eviction rather than a write-back keeps the bump to a single statement — the read-through in §4.3
> reloads the row on the next request that actually needs it, and a missed eviction costs one extra DB
> read, never a stale allow.

---

## Part B — Verified signup `[07 S8]`

`PublicSignUpHandler` currently calls `sessionFactory.CreateSessionAsync(...)` and returns full tokens
for an account created **unverified**. Stop issuing a session at signup: create the account, send the
verification OTP (the existing `RegisterAsync` factory already does this), and return "verify your
email". The user verifies via the existing `verify-otp` flow, then logs in — and login already
enforces verified/active.

### 4.6 Handler, result & endpoint

`PublicSignUpHandler.cs` — drop `ISessionFactory`; return the created user without tokens:

```csharp
public class PublicSignUpHandler(IPublicSignUpAuthFactory authFactory, IMapper mapper)
    : ICommandHandler<PublicSignUpCommand, PublicSignUpResult>
{
    public async Task<PublicSignUpResult> Handle(PublicSignUpCommand command, CancellationToken cancellationToken)
    {
        PublicSignUpAuthData authData = await authFactory.RegisterAsync(
            email: command.Email,
            userName: command.UserName,
            password: command.Password,
            cancellationToken: cancellationToken
        );

        UserResponseDto userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper),
            avatar: null
        );

        return new PublicSignUpResult(User: userDto, VerificationRequired: true);
    }
}
```

`PublicSignUpCommand.cs` — the result no longer carries an `AuthenticationResult`:

```csharp
public record PublicSignUpResult(UserResponseDto User, bool VerificationRequired);
```

`V1/PublicSignUpEndpointV1.cs` — one response for both clients (no tokens, no cookies), still
`201 Created`:

```csharp
public record PublicSignUpResponse(UserResponseDto User, bool VerificationRequired);
```

The handler no longer needs `ISessionFactory`; the endpoint no longer branches on
`tokenDelivery.IsWebClient()` or sets cookies.

---

## Part C — Claim-based active/verified on interactions `[06 §5 / 07 S8]`

The 34 Visitor mutation endpoints authorize with `RequireVisitorOnly` → `UserRoleRequirement(["Visitor"])`
— a **role-claim check only**. With the invalidation layer in place, the fix is *not* a per-request DB
lookup; it is to also require the `is_active` and `is_verified` **claims** (which the token already
carries), and let §4.5 keep them honest: a deactivation rotates the security stamp and revokes
sessions, so a deactivated user's token is rejected by §4.4 within a request, not by a query.

### 4.7 Require the claims (no DB) on `RequireVisitorOnly`

In `AuthorizationExtensions.ConfigureUserRolePolicies`, add claim requirements to the Visitor policy:

```csharp
foreach (var (policyName, roles) in policies)
{
    authBuilder.AddPolicy(
        name: policyName,
        policy =>
        {
            policy.Requirements.Add(new UserRoleRequirement(allowedRoles: roles));

            if (policyName == UserRolePolicies.RequireVisitorOnly)
            {
                policy.RequireClaim(JwtClaimsConstants.IsActive, "true");
                policy.RequireClaim(JwtClaimsConstants.IsVerified, "true");
            }
        }
    );
}
```

`RequireClaim` is a pure claim check — zero DB. A user deactivated mid-session still holds a token
with `is_active=true`, but §4.5 revoked their sessions on deactivation, so §4.4 rejects the token
first. The existing DB-backed `AccountStatusRequirement`/`RequireActiveUser`/`RequireVerifiedUser`
policies remain available for any endpoint that genuinely wants a live DB check, but the hot
interaction paths use claims.

> **Scope:** anonymous interaction endpoints (`GetArticleComments`, the `Share*`/`Record*View`
> endpoints) are unchanged. Admin/super-admin policies stay role-claim only; their freshness is
> covered by the same `tver`/`sstamp`/denylist layer.

---

## Part D — Refresh hardening `[07 S11]`

`RefreshTokenFactory` already rejects a revoked/expired session (`ValidRefreshTokenSessionSpecification`
= token-hash AND not-revoked AND not-expired). Remaining gaps: it never re-checks the account, it
mints the new access token from stale entity state, and a session can be slid forward forever.

### 4.8 Re-check state, refresh markers, absolute cap

On refresh, after the session is found and before rotating:

```csharp
// deactivated account → refuse and revoke (raises SessionRevokedEvent → denylist)
if (!session.User.IsActive)
{
    session.Revoke(EnumSessionRevokeReason.SecurityInvalidation);
    await sessionRepository.SaveChangesAsync(cancellationToken);
    throw sessionErrors.InvalidRefreshToken();
}

// absolute lifetime ceiling reached → force a fresh login
if (session.HasReachedAbsoluteExpiry())
{
    session.Revoke(EnumSessionRevokeReason.Expiry);
    await sessionRepository.SaveChangesAsync(cancellationToken);
    throw sessionErrors.InvalidRefreshToken();
}
```

Because refresh re-reads `session.User` **and** the user's `UserTokenStateEntity` (via
`IUserTokenStateRepository.GetAsync`), the **new** access token is minted with the *current*
`SecurityStamp`, `TokenVersion`, roles, permissions, `is_active`, `is_verified` — this is what makes a
`tver` bump silently self-heal on the next refresh.

**Absolute cap** — add `SessionEntity.AbsoluteExpiresAt` (set once at `Create` from a new parameter;
`UpdateRefreshToken`/`Reactivate` leave it untouched) and `HasReachedAbsoluteExpiry() => DateTime.UtcNow >= AbsoluteExpiresAt`.
`SessionFactory.CreateSessionAsync` computes `DateTime.UtcNow.AddDays(AppEnvironment.SessionAbsoluteLifetimeDays())`.

**Config** — new env var `JWT_SESSION_ABSOLUTE_LIFETIME_IN_DAYS` (e.g. `30`), read by
`AppEnvironment.SessionAbsoluteLifetimeDays()` (fallback `SessionConstants.DefaultAbsoluteLifetimeDays = 30`);
add to `.env.template`.

**Migration** — `AddSessionAbsoluteExpiresAt` adds `absolute_expires_at` (+ `SessionConfiguration`),
backfilling live rows to `created_at + INTERVAL 'N days'`. Leave unapplied.

---

## Tests

- **Unit**
  - `UserTokenStateEntity.Create` seeds a non-empty `SecurityStamp` and `TokenVersion = 0`.
  - `SessionEntity`: `AbsoluteExpiresAt` set on `Create`; `HasReachedAbsoluteExpiry` boundary;
    `UpdateRefreshToken`/`Reactivate` don't move it.
  - `SessionRevocationCache`: `IsRevoked` false→true after `Revoke`; expires after the TTL.
  - `UserSecurityStateCache`: read-through loads from the repo on a miss and caches; `Set` is observed
    by the next `GetAsync`; `Remove` forces a reload.
  - `SessionRevokedLogHandler`: a `SessionRevokedEvent` calls `revocationCache.Revoke` with the id.
  - `JwtService`: the minted token carries `sstamp` and `tver` matching the supplied token state.
  - `PublicSignUpHandler`: returns `VerificationRequired: true`, no `AuthenticationResult`, no session
    factory dependency.
  - `RefreshTokenFactory` (mocked repo): deactivated user → revoke + `InvalidRefreshToken`; past
    absolute expiry → revoke + `InvalidRefreshToken`; happy path rotates and the new token reflects the
    user's current stamp/version.
  - `AppEnvironment.SessionAbsoluteLifetimeDays`: parses / falls back.
- **Integration** (real HTTP)
  - **Session revocation:** log in → protected call 200 → sign out → the *same still-unexpired token*
    → 401.
  - **Token-version invalidation:** log in as an admin with permission A; revoke that permission
    (bumps `tver`) → the old token 401s; a refresh then succeeds and the new token no longer has A.
  - **Security-stamp invalidation:** change the user's password (rotates `sstamp`, revokes sessions) →
    the old token 401s **and** refresh is rejected.
  - **Verified signup:** `POST /public/auth/signup` → 201 with `VerificationRequired: true`, no tokens,
    no cookies; the user row exists, unverified.
  - **Interactions:** an active verified Visitor likes an article (200); a Visitor deactivated in the DB
    (which revokes sessions) is refused on the next like.
  - **Refresh refuses deactivated account:** deactivate → refresh rejected and session revoked.
  - **`IUserTokenStateRepository`** (real repo): `BumpTokenVersionAsync` increments the row; two bumps
    in a row land on `+2` (proves the atomic SQL increment, not a lost read-modify-write);
    `RotateSecurityStampAsync` changes the stamp; `GetAsync` returns the projection.

---

## Rollout

1. Provision `JWT_SESSION_ABSOLUTE_LIFETIME_IN_DAYS` (e.g. `30`) in every environment.
2. Ship both migrations (`AddUserTokenState`, `AddSessionAbsoluteExpiresAt`) — both backfill.
3. Deploy backend + clients together:
   - Signup no longer returns tokens — route new users to verify-email, then login.
   - Access tokens now carry `sstamp`/`tver`; tokens minted before this ships lack them and are
     rejected, so **all users re-authenticate once** at deploy. Because access tokens are short-lived
     this is a one-time blip; communicate it.
   - Clients must treat a 401 on an otherwise-valid token as "try one refresh, then send to login" so a
     `tver` bump self-heals without a visible logout.
4. The invalidation caches are per-instance until Stage 9 makes them distributed.

## Verification

1. `dotnet build 116_backend.sln` — 0 warnings / 0 errors.
2. `dotnet test tests/Unit` — green.
3. Run `tests/Integration` locally.
4. Confirm each migration adds only its columns (+ backfill) and nothing else.

**PR title:** `feat(auth): token invalidation, session revocation and verified signup`
