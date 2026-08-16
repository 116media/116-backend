# Stage 3 — Rate-limit partitioning & trusted proxies

Closes **[01 §1.1 / 07 S6]** (every rate-limit policy is one global bucket, so any single caller can
exhaust the limit for everyone, and login/OTP/password have no per-account throttle),
**[08 §20]** (forwarded headers are cleared, so behind a load balancer every request collapses to the
proxy's IP), and **[08 §8]** (CORS fails *open* when no origins are configured, and `UseCors` runs
after the exception handler so error responses miss the CORS headers).

These are one PR because they interlock: correct per-caller rate limiting needs the **real** client
IP (trusted forwarded headers) and the **authenticated subject** (rate limiter after authentication).

> **No breaking API change.** Behaviour changes for callers behind the wrong proxy config or a
> mis-set CORS origin list — see [Rollout](#rollout).

> **Depends on Stage 2** (branch stacks on the tree Stage 2 lands on). Finalize this spec's code
> against that tree.

---

## Checklist

- [ ] 3.1 — `AppEnvironment.TrustedProxyNetworks()` reads `TRUSTED_PROXY_NETWORKS` (CIDR list) + `.env.template`
- [ ] 3.2 — `ForwardedHeadersOptions`: populate `KnownNetworks` from config, `ForwardLimit = 1`; keep cleared when unset
- [ ] 3.3 — `RateLimitPartitioning.ResolvePartitionKey` (authenticated subject → client IP)
- [ ] 3.4 — Partition all three builders (`SlidingWindow`, `TokenBucket`, `FixedWindow`) by that key
- [ ] 3.5 — Middleware order: `UseCors` above the exception handler; `UseRateLimiter` **after** `UseAuthentication`
- [ ] 3.6 — `IAccountRateLimiter` (in-process, per-account) applied to the login/OTP/password handlers
- [ ] 3.7 — CORS fails **closed** when origins are empty outside Development, with a startup warning
- [ ] 3.8 — Unit + integration tests
- [ ] 3.9 — Verify (build 0/0, unit green; run integration locally)

---

## Part A — Trust only known proxies `[08 §20]`

Today [`Program.cs`](../../../src/Api/Program.cs) clears both trust lists:

```csharp
options.KnownNetworks.Clear();
options.KnownProxies.Clear();
```

With the lists empty, `UseForwardedHeaders` ignores `X-Forwarded-For` entirely, so
`HttpContext.Connection.RemoteIpAddress` is the **proxy's** address in every deployment that sits
behind a load balancer. That both defeats IP-based rate limiting (Part B) and hides the real client
from logs. The fix trusts exactly the proxy network(s) the operator declares — nothing more — and
reads only **one** hop.

### 3.1 Config & env

`src/Shared/Shared/Application/Configurations/Environment.cs` — add alongside `CorsAllowedOrigins()`
(and `using System.Net;` at the top if absent):

```csharp
/// <summary>
/// The proxy networks whose <c>X-Forwarded-*</c> headers are trusted, parsed from
/// <c>TRUSTED_PROXY_NETWORKS</c> (a comma-separated CIDR list, e.g.
/// <c>10.0.0.0/8,172.18.0.0/16</c>). An unset or empty value yields an empty array, which keeps
/// forwarded headers untrusted — correct for direct-connection local development.
/// </summary>
public static IReadOnlyList<IPNetwork> TrustedProxyNetworks()
{
    string? raw = Environment.GetEnvironmentVariable("TRUSTED_PROXY_NETWORKS");
    if (string.IsNullOrWhiteSpace(raw))
    {
        return [];
    }

    return
    [
        .. raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseCidr)
            .Where(network => network is not null)
            .Select(network => network!.Value),
    ];
}

/// <summary>
/// Parses a single CIDR block (<c>address/prefix</c>) into an <see cref="IPNetwork"/>, returning
/// null for a malformed entry so one bad value cannot take down startup.
/// </summary>
private static IPNetwork? ParseCidr(string cidr)
{
    string[] parts = cidr.Split('/', 2);
    if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out IPAddress? address))
    {
        return null;
    }

    return int.TryParse(parts[1], out int prefix) ? new IPNetwork(address, prefix) : null;
}
```

`.env.template` — add:

```
# Trusted reverse-proxy networks (comma-separated CIDR). Leave empty for direct connections.
TRUSTED_PROXY_NETWORKS=
```

### 3.2 Wire it into `ForwardedHeadersOptions`

`Program.cs` — replace the `Configure<ForwardedHeadersOptions>` block:

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Only trust the operator-declared proxy networks; read exactly one hop so a client cannot spoof
    // its address by pre-seeding X-Forwarded-For. With no networks configured the lists stay empty
    // and forwarded headers are ignored (direct-connection / local development).
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 1;

    foreach (IPNetwork network in AppEnvironment.TrustedProxyNetworks())
    {
        options.KnownNetworks.Add(network);
    }
});
```

`app.UseForwardedHeaders()` already runs first, so once configured every downstream component — the
rate limiter included — sees the real client IP.

---

## Part B — Partition rate limits per caller `[01 §1.1 / 07 S6]`

Every builder currently registers a **non-partitioned** limiter, e.g.
[`SlidingWindowBuilder`](../../../src/Shared/Shared/Application/Builders/RateLimit/SlidingWindowBuilder.cs):

```csharp
options.AddSlidingWindowLimiter(policyName, limiterOptions => { ... });
```

A non-partitioned limiter is a **single global bucket** per policy: one script hitting
`/public/auth/login` exhausts the `Authentication` window for every user at once. The fix partitions
each policy by caller — the authenticated subject when there is one, else the (now-real) client IP —
and adds a second, per-target-account throttle on the pre-auth security endpoints (§3.6).

### 3.3 The partition-key resolver

`src/Shared/Shared/Application/Builders/RateLimit/RateLimitPartitioning.cs` (new)

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace _116.Shared.Application.Builders.RateLimit;

/// <summary>
/// Resolves the partition key every rate-limit policy is bucketed by: the authenticated subject when
/// the request carries one, otherwise the client IP. Anonymous, pre-auth endpoints (login, OTP,
/// password reset) therefore partition by IP, so one caller can no longer drain a policy for everyone.
/// </summary>
public static class RateLimitPartitioning
{
    private const string AnonymousPartition = "anonymous";

    // The "sub" claim type, spelled out to avoid a JwtBearer package reference from Shared.
    private const string SubjectClaim = "sub";

    /// <summary>
    /// Returns a stable partition key for <paramref name="httpContext"/>. Prefers the subject claim so
    /// an authenticated caller is limited across IPs; falls back to the connection's remote IP, which
    /// is the real client once forwarded headers are honoured (Part A). Requires the rate limiter to
    /// run after authentication (§3.5) — otherwise the principal is empty and every request keys by IP.
    /// </summary>
    public static string ResolvePartitionKey(HttpContext httpContext)
    {
        string? subject =
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue(SubjectClaim);

        if (!string.IsNullOrWhiteSpace(subject))
        {
            return $"user:{subject}";
        }

        string? ip = httpContext.Connection.RemoteIpAddress?.ToString();
        return $"ip:{ip ?? AnonymousPartition}";
    }
}
```

### 3.4 Partition every builder

Each builder swaps its non-partitioned call for `options.AddPolicy(name, partitioner)`. Signatures
and call sites in
[`RateLimitingExtension`](../../../src/Shared/Shared/Application/Extensions/RateLimitingExtension.cs)
are unchanged.

`SlidingWindowBuilder.cs`:

```csharp
public SlidingWindowBuilder AddPolicy(string policyName, int permitLimit, int windowSeconds, int segmentsPerWindow)
{
    options.AddPolicy(
        policyName,
        httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: RateLimitPartitioning.ResolvePartitionKey(httpContext),
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromSeconds(windowSeconds),
                    SegmentsPerWindow = segmentsPerWindow,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                }
            )
    );
    return this;
}
```

`TokenBucketBuilder.cs`:

```csharp
public TokenBucketBuilder AddPolicy(string policyName, int tokenLimit, int tokensPerPeriod, int replenishmentPeriodSeconds)
{
    options.AddPolicy(
        policyName,
        httpContext =>
            RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: RateLimitPartitioning.ResolvePartitionKey(httpContext),
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = tokenLimit,
                    TokensPerPeriod = tokensPerPeriod,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(replenishmentPeriodSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }
            )
    );
    return this;
}
```

`FixedWindowBuilder.cs`:

```csharp
public FixedWindowBuilder AddPolicy(string policyName, int permitLimit, int windowSeconds)
{
    options.AddPolicy(
        policyName,
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: RateLimitPartitioning.ResolvePartitionKey(httpContext),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromSeconds(windowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                }
            )
    );
    return this;
}
```

Keep each builder's existing constructor and XML docs — only the registration call changes.

### 3.5 Middleware order

Two moves, so the pieces above actually take effect:

1. `UseRateLimiter` must run **after** `UseAuthentication` — otherwise `HttpContext.User` is empty at
   partition time and §3.3 silently degrades to IP-only for every request. It stays **before**
   `UseAuthorization`, so a request that would be 403 still counts against the limit. JWT validation
   is cheap and the pre-auth endpoints carry no token (they key by IP regardless), so this is safe.
2. `UseCors` must run **above** the exception handler (Part C).

`Program.cs` — the pipeline becomes:

```csharp
app.UseForwardedHeaders();
app.UseSwaggerFormatting();
app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseAppLocalization();
app.UseCors();               // moved above the exception handler (Part C)
app.UseAppExceptionHandler();
app.UseAuthentication();
app.UseRateLimiter();        // moved after authentication so the partition key sees the subject
app.UseAuthorization();

app.UseApiVersioning();
```

### 3.6 Per-account throttle for the pre-auth security endpoints `[07 S6]`

The middleware partition keys anonymous login/OTP/password requests by **IP**, which stops one IP
from draining the policy. It does **not** stop credential stuffing against a single account from many
IPs, because the target account (the email) lives in the request body — invisible to the limiter
partitioner. That throttle belongs where the email is known: inside the handlers, via a small
in-process per-account limiter. (Stage 9 swaps the in-process store for Redis so it holds across
instances; until then it is per-instance, which still meaningfully raises the cost of an attack.)

`src/Shared/Shared/Application/Builders/RateLimit/IAccountRateLimiter.cs` (new)

```csharp
namespace _116.Shared.Application.Builders.RateLimit;

/// <summary>
/// A per-account rate limiter for the pre-auth security endpoints, keyed by a stable account
/// identifier (normalized email) rather than by caller IP. Complements the middleware limiter so a
/// single account cannot be brute-forced from many IPs. Throws <c>RateLimitExceededException</c> when
/// the account's window is exhausted, matching the middleware's 429 contract.
/// </summary>
public interface IAccountRateLimiter
{
    /// <summary>
    /// Consumes one permit for <paramref name="accountKey"/> under <paramref name="policyName"/>,
    /// throwing when the account has exceeded that policy's window. Policies without a per-account
    /// limiter are a no-op.
    /// </summary>
    Task EnsureWithinLimitAsync(string policyName, string accountKey, CancellationToken cancellationToken);
}
```

`src/Shared/Shared/Application/Builders/RateLimit/AccountRateLimiter.cs` (new)

```csharp
using System.Threading.RateLimiting;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Shared.Application.Exceptions;

namespace _116.Shared.Application.Builders.RateLimit;

/// <summary>
/// In-process <see cref="IAccountRateLimiter"/>. Holds one sliding-window limiter per pre-auth
/// security policy, partitioned by normalized account key. Registered as a singleton so the windows
/// persist across requests for the process lifetime.
/// </summary>
public sealed class AccountRateLimiter : IAccountRateLimiter, IAsyncDisposable
{
    private readonly IReadOnlyDictionary<string, PartitionedRateLimiter<string>> _limiters;

    /// <summary>
    /// Builds the per-account limiters from the same policy constants as the middleware limiters.
    /// </summary>
    public AccountRateLimiter()
    {
        _limiters = new Dictionary<string, PartitionedRateLimiter<string>>
        {
            [RateLimitPolicies.Authentication] = BuildSliding(
                AuthenticationRateLimitConstants.PermitLimit,
                AuthenticationRateLimitConstants.WindowSeconds,
                AuthenticationRateLimitConstants.SegmentsPerWindow
            ),
            [RateLimitPolicies.Otp] = BuildSliding(
                OtpRateLimitConstants.PermitLimit,
                OtpRateLimitConstants.WindowSeconds,
                OtpRateLimitConstants.SegmentsPerWindow
            ),
            [RateLimitPolicies.PasswordManagement] = BuildSliding(
                PasswordManagementRateLimitConstants.PermitLimit,
                PasswordManagementRateLimitConstants.WindowSeconds,
                PasswordManagementRateLimitConstants.SegmentsPerWindow
            ),
        };
    }

    /// <inheritdoc />
    public async Task EnsureWithinLimitAsync(string policyName, string accountKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accountKey) || !_limiters.TryGetValue(policyName, out PartitionedRateLimiter<string>? limiter))
        {
            return;
        }

        string key = accountKey.Trim().ToLowerInvariant();

        using RateLimitLease lease = await limiter.AcquireAsync(key, permitCount: 1, cancellationToken);
        if (lease.IsAcquired)
        {
            return;
        }

        TimeSpan retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retry) ? retry : TimeSpan.Zero;
        throw new RateLimitExceededException(retryAfter);
    }

    private static PartitionedRateLimiter<string> BuildSliding(int permitLimit, int windowSeconds, int segmentsPerWindow) =>
        PartitionedRateLimiter.Create<string, string>(key =>
            RateLimitPartition.GetSlidingWindowLimiter(
                key,
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromSeconds(windowSeconds),
                    SegmentsPerWindow = segmentsPerWindow,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                }
            )
        );

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (PartitionedRateLimiter<string> limiter in _limiters.Values)
        {
            await limiter.DisposeAsync();
        }
    }
}
```

Register it in `AddRateLimiting` (same extension), so it lives beside the middleware config:

```csharp
services.AddSingleton<IAccountRateLimiter, AccountRateLimiter>();
```

**Apply it in each pre-auth handler**, keyed by the request email, before any credential work.
`PublicLoginHandler` is the exemplar (inject `IAccountRateLimiter accountRateLimiter`):

```csharp
public async Task<PublicLoginResult> Handle(PublicLoginCommand command, CancellationToken cancellationToken)
{
    await accountRateLimiter.EnsureWithinLimitAsync(
        RateLimitPolicies.Authentication,
        command.Email,
        cancellationToken
    );

    // ... existing credential verification ...
}
```

Wire the same call into the other handlers behind these policies, using each command's email:

| Policy | Handlers | Key |
| --- | --- | --- |
| `Authentication` | `PublicLoginHandler`, `AdminLoginHandler` | `command.Email` |
| `Otp` | `PublicVerifyOtpHandler`, `PublicResendOtpHandler`, `AdminVerifyOtpHandler`, `AdminResendOtpHandler` (whichever exist) | `command.Email` |
| `PasswordManagement` | `PublicForgotPasswordHandler`, `PublicResetPasswordHandler`, `AdminForgotPasswordHandler`, `AdminResetPasswordHandler`, change-password | `command.Email` |

> The account-enumeration-safe forms (Stage 5 `[07 S7]`) still call this for **every** email,
> including unknown accounts, so the throttle itself never reveals whether an account exists.

---

## Part C — Fail CORS closed `[08 §8]`

Today the default policy opens CORS to the entire internet when no origins are configured:

```csharp
else
{
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // <-- fails OPEN
}
```

A missing `WEBAPP_ORIGIN`/`DASHBOARD_ORIGIN` in production silently turns the API into an any-origin
API. It must fail **closed** everywhere except Development, and say so loudly at startup.

### 3.7 Fail-closed policy + startup warning

`Program.cs` — replace the `AddCors` block (the environment is available as `builder.Environment`):

```csharp
string[] allowedOrigins = AppEnvironment.CorsAllowedOrigins();
bool isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
        else if (isDevelopment)
        {
            // Local convenience only: no origins configured in Development means allow any.
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }

        // Outside Development with no configured origins the policy is left empty — CORS fails closed,
        // so a misconfigured deploy rejects cross-origin calls instead of allowing every origin.
    });
});
```

After `WebApplication app = builder.Build();`, warn loudly so the misconfiguration is visible rather
than a silent cross-origin outage:

```csharp
if (!app.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    app.Logger.LogWarning(
        "CORS: no allowed origins configured outside Development — cross-origin browser requests are "
            + "blocked (fail-closed). Set WEBAPP_ORIGIN / DASHBOARD_ORIGIN."
    );
}
```

The `UseCors`-above-the-exception-handler move is in §3.5.

---

## Tests

- **Unit**
  - `RateLimitPartitioning.ResolvePartitionKey`: `user:{sub}` for a principal with a
    `NameIdentifier`/`sub` claim; `ip:{addr}` for an anonymous principal; `ip:anonymous` when
    `RemoteIpAddress` is null.
  - `AppEnvironment.TrustedProxyNetworks`: parses a multi-entry CIDR list; drops malformed entries;
    empty for unset/blank (scoped env-var fixture).
  - `AccountRateLimiter.EnsureWithinLimitAsync`: allows up to the policy limit for one key, then
    throws `RateLimitExceededException`; a **different** key is unaffected; an unknown policy is a
    no-op; the key is normalized so a mixed-case, whitespace-padded email shares one bucket with its
    trimmed lowercase form.
- **Integration** (real HTTP, under the `RateLimitedApiFixture` collection)
  - **Per-subject partition:** authenticate as subject A and exhaust a policy to 429; subject B still
    gets 200 on the same policy — proving buckets no longer collide (and that the limiter sees the
    authenticated principal after the §3.5 reorder).
  - **Per-account throttle:** repeated failed logins for one email 429 after the limit, while a
    different email still reaches the handler — even from the same connection/IP.
  - **Forwarded headers:** with `TRUSTED_PROXY_NETWORKS` covering the loopback test connection, two
    distinct `X-Forwarded-For` client IPs partition separately; with no trusted network the header is
    ignored (both share the connection-IP bucket).
  - **CORS fail-closed:** an allowed origin gets `Access-Control-Allow-Origin`; a disallowed origin
    does not; an error response (e.g. a 400) from an allowed origin still carries the CORS header
    (proves `UseCors` wraps the exception handler).

---

## Rollout

1. Set `TRUSTED_PROXY_NETWORKS` in every environment behind a load balancer / ingress (the LB's
   egress network as CIDR). Leave empty for direct-connection setups.
2. Confirm `WEBAPP_ORIGIN` / `DASHBOARD_ORIGIN` are set in every non-Development environment — an
   empty list now **blocks** all cross-origin browser calls instead of allowing them (a startup
   warning is logged if so).
3. No migration, no client change.

## Verification

1. `dotnet build 116_backend.sln` — 0 warnings / 0 errors.
2. `dotnet test tests/Unit` — green.
3. Run `tests/Integration` locally (rate-limit fixture included).
4. Behind a proxy, confirm request logs show the real client IP and that two different callers — and
   two different target accounts — get independent rate-limit budgets.

**PR title:** `fix(security): partition rate limits per caller and trust only known proxies`
