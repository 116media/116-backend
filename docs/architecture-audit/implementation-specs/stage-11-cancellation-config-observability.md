# Stage 11 — CancellationToken, validated configuration & observability

Closes **[01 §1.5]** / **[06 §1]** (High), **[01 §1.10]** / **[08 §10]** (High), **[05 §8]**,
**[05 §9]**, **[08 §4]** (file-size strategy), **[08 §7]** / **[01 §1.11]** (remaining half),
**[08 §12]**, **[01 §1.14]**, **[07 S12]**.

Every endpoint drops the request's `CancellationToken` — an abandoned request runs its queries to
completion. Configuration is a static `Environment.GetEnvironmentVariable` tuple factory
(`src/Shared/Shared/Application/Configurations/Environment.cs`) with silent `null`s that surface
as a `JwtService` throw on first use, not at boot. There are no health checks, no traces, and
Swagger UI ships unconditionally.

> Draft — finalized against the tree Stage 10 lands on. All types below verified against the
> current tree.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | How to find every token dropper | annotate by hand, or make the compiler do it | **Compiler.** `IDispatcher.Send` currently declares `CancellationToken cancellationToken = default`; deleting the default turns all ~293 call sites into build errors, each fixed by threading the endpoint's token. No grep-based sweep can miss a site the compiler flags. |
| D2 | Configuration | typed `IOptions` bound from `IConfiguration`, or self-validating env-var descriptors | **Env-var descriptors validated at boot.** `Environment.GetEnvironmentVariable` stays the source — no `IConfiguration` binding, no `__` renames. Each variable is declared once as an `EnvVar<T>` descriptor (name, parser, default, validators) in a small per-concern schema class; construction registers the descriptor, so `EnvSchema.ValidateAtBoot()` walks every declaration and throws one error naming all failures. Consumers read typed non-nullable `Value`s; `AppEnvironment`'s nullable-tuple accessors and their consumer-side guards are deleted. |
| D3 | What replaces the fallback in `AccountStatusRequirementHandler` | keep the DB-with-claims-fallback, or fail closed | **Fail closed, cache per request.** Today a DB error silently degrades to trusting JWT claims `[07 S12]` — an attacker who forces DB pressure gets stale-claims authorization. The DB check stays (Stage 4 put the fast path in the token), but on failure the requirement **fails**; `HttpContext.Items` caches the result so N policies on one request cost one lookup. |
| D4 | Observability stack | Serilog-only, or add OpenTelemetry | **OTel for traces/metrics, Serilog stays for logs.** Health checks via `AspNetCore.HealthChecks.*` for Npgsql + Redis. Seq URL and environment labels become boot-validated descriptors (`[08 §12]` — the sink is hardcoded `localhost` today). |

---

## Checklist

- [x] 11.1 — `IDispatcher.Send` loses `= default`; build breaks enumerate the sweep
- [x] 11.2 — Endpoint lambdas take `CancellationToken` and pass it; handlers/repositories complete the chain
- [x] 11.3 — `EnvVar<T>` descriptors + per-concern schemas (Database/Jwt/Cloudinary/SocialAuth/Web/Mail/Security); `EnvSchema.ValidateAtBoot()` gates startup
- [x] 11.4 — Call sites read descriptors directly; `Environment.cs` tuple accessors deleted with their null guards
- [x] 11.5 — Health checks (`/health/live`, `/health/ready`) + OpenTelemetry + correlation middleware
- [x] 11.6 — Kestrel/form limits sized for the 350 MB upload; Swagger gated; security headers + HSTS
- [x] 11.7 — `FileSizeLimitExceeded` exception strategy; `_rootVersionedGroup` static removed; account-status fail-closed + per-request cache
- [x] 11.8 — Tests: cancelled request aborts; boot fails on missing `JWT_SECRET`; `/health/ready` degrades
- [x] 11.9 — Verify (build 0/0, csharpier, unit, integration)

---

## Part A — CancellationToken

### 11.1–11.2 The sweep

`src/Shared/Shared.Contracts/Application/CQRS/IDispatcher.cs:16` today:

```csharp
Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
```

Delete the `= default`. Every endpoint then fails to compile — e.g.
`PublicLikeArticleEndpointV1.cs`, whose lambda gains the token Carter already binds:

```csharp
group
    .MapPost(
        $"/{{id}}/{InteractionsRouteConstants.Likes}",
        async (
            string id,
            ClaimsPrincipal user,
            IClaimsProvider claimsProvider,
            IDispatcher dispatcher,
            CancellationToken cancellationToken
        ) =>
        {
            Guid articleId = Guid.Parse(id);
            Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

            var command = new PublicLikeArticleCommand(ArticleId: articleId, UserId: userId);

            PublicLikeArticleResult result = await dispatcher.Send(
                request: command,
                cancellationToken: cancellationToken
            );

            var response = new PublicLikeArticleResponse(IsSuccess: result.IsSuccess);
            return Results.Ok(response);
        }
    )
```

Handlers already receive `CancellationToken` from the dispatcher and (post-Stage 8/9) pass it to
repositories — the chain below the dispatcher is largely done; the endpoints were the gap. The
sweep lands as three commit series: contract change, endpoints (mechanical), stragglers the
build still flags.

> **Finding at implementation time — the token changed how the hybrid cache runs factories.**
> With `CancellationToken.None`, `HybridCache.GetOrCreateAsync` runs the miss factory inline on
> the caller's execution context. With a real cancelable token — what every endpoint passes after
> this sweep — stampede protection runs the factory detached, so one caller's cancellation cannot
> kill the computation other callers share. Detached means the request's `CultureInfo` no longer
> flows into the handler, and every i18n message inside a cacheable query resolved to the process
> default (English) regardless of `Accept-Language`. Four integration tests caught it. The fix
> lives in `CachingDecorator`: the request culture rides in the factory state and is restored
> at the top of the factory. Cache keys deliberately exclude culture — cached payloads are data,
> not localized strings; error paths (where i18n lives) are never cached.

## Part B — Validated configuration

### 11.3–11.4 Env-var descriptors with boot validation

`Environment.GetEnvironmentVariable` stays the source of every value — the problem is not where
configuration comes from but that `AppEnvironment` returns nullable tuples (`Jwt()` → five
nullable strings) and every consumer re-validates or forgets to. The fix: each variable is
declared exactly once as a self-registering descriptor, and boot validation walks the registry.
Declaring is registering, so "added a variable but forgot to validate it" cannot exist.

The mechanism is three fixed-size files that never grow with the variable count:

```csharp
// src/Shared/Shared/Application/Configurations/EnvVar.cs
/// <summary>
/// Contract every declared environment variable satisfies, so boot validation can walk
/// all declarations without knowing their value types.
/// </summary>
public interface IEnvVar
{
    string Name { get; }

    /// <summary>
    /// Evaluates the variable, returning an error message or null when valid.
    /// </summary>
    string? Error();
}

/// <summary>
/// A single environment variable: name, parser, default and validators. Construction
/// registers the instance, so a declared variable can never be missed by boot validation.
/// </summary>
public sealed class EnvVar<T> : IEnvVar
{
    /// <summary>
    /// The parsed value. Guaranteed valid and non-null for required variables once
    /// <see cref="EnvSchema.ValidateAtBoot" /> has passed.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Adds a validator returning an error message, or null when the value passes.
    /// </summary>
    public EnvVar<T> Rule(Func<T, string?> validator);
}

/// <summary>
/// Factory methods for the common variable shapes.
/// </summary>
public static class EnvVar
{
    public static EnvVar<string> Required(string name);
    public static EnvVar<string?> Optional(string name);
    public static EnvVar<string> Optional(string name, string @default);
    public static EnvVar<int> Int(string name, int @default);
}
```

```csharp
// src/Shared/Shared/Application/Configurations/EnvVarValidators.cs
/// <summary>
/// Reusable validators for environment variable declarations.
/// </summary>
public static class EnvVarValidators
{
    public static EnvVar<string> MinLength(this EnvVar<string> envVar, int length) =>
        envVar.Rule(v => v.Length >= length ? null : $"must be at least {length} characters.");

    public static EnvVar<int> InRange(this EnvVar<int> envVar, int min, int max) =>
        envVar.Rule(v => v >= min && v <= max ? null : $"must be between {min} and {max}.");

    public static EnvVar<string> AbsoluteUrl(this EnvVar<string> envVar) =>
        envVar.Rule(v => Uri.IsWellFormedUriString(v, UriKind.Absolute) ? null : "must be an absolute URL.");
}
```

```csharp
// src/Shared/Shared/Application/Configurations/EnvSchema.cs
/// <summary>
/// Registry of every declared environment variable. Validation walks all declarations and
/// reports every failure at once, so a misconfigured instance refuses to boot with the
/// complete list instead of failing variable by variable.
/// </summary>
public static class EnvSchema
{
    internal static void Register(IEnvVar envVar);

    /// <summary>
    /// Forces every schema class to declare its variables, then validates all of them.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown listing every invalid variable.</exception>
    public static void ValidateAtBoot()
    {
        // Static classes initialize lazily; touching each type materializes its declarations.
        RuntimeHelpers.RunClassConstructor(typeof(DatabaseEnv).TypeHandle);
        // ... JwtEnv, CloudinaryEnv, SocialAuthEnv, WebEnv, MailEnv, SecurityEnv ...

        List<string> errors = Declared.Select(v => v.Error()).Where(e => e is not null).ToList()!;
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Invalid environment configuration:{Environment.NewLine}  "
                    + string.Join($"{Environment.NewLine}  ", errors)
            );
        }
    }
}
```

The declarations live in one small schema class per concern — adding a variable touches exactly
one domain file with one line:

```csharp
// src/Shared/Shared/Application/Configurations/Schemas/DatabaseEnv.cs
/// <summary>
/// Postgres connection variables.
/// </summary>
public static class DatabaseEnv
{
    public static readonly EnvVar<string> Host = EnvVar.Required("POSTGRES_HOST");
    public static readonly EnvVar<int> Port = EnvVar.Int("POSTGRES_PORT", @default: 5432).InRange(1, 65535);
    public static readonly EnvVar<string> Name = EnvVar.Required("POSTGRES_DB");
    public static readonly EnvVar<string> User = EnvVar.Required("POSTGRES_USER");
    public static readonly EnvVar<string> Password = EnvVar.Required("POSTGRES_PASSWORD");
}
```

```csharp
// Schemas/JwtEnv.cs
/// <summary>
/// JWT signing and lifetime variables.
/// </summary>
public static class JwtEnv
{
    public static readonly EnvVar<string> Secret = EnvVar.Required("JWT_SECRET").MinLength(32);
    public static readonly EnvVar<string> Issuer = EnvVar.Required("JWT_ISSUER");
    public static readonly EnvVar<string> Audience = EnvVar.Required("JWT_AUDIENCE");

    public static readonly EnvVar<int> AccessTokenExpirationMinutes =
        EnvVar.Int("JWT_ACCESS_TOKEN_EXPIRATION", @default: 60).InRange(1, 24 * 60);

    public static readonly EnvVar<int> RefreshTokenExpirationDays =
        EnvVar.Int("JWT_REFRESH_TOKEN_EXPIRATION", @default: 30).InRange(1, 365);

    public static readonly EnvVar<int> SessionAbsoluteLifetimeDays =
        EnvVar.Int("JWT_SESSION_ABSOLUTE_LIFETIME_IN_DAYS", @default: 90).InRange(1, 365);
}
```

```csharp
// Schemas/CloudinaryEnv.cs — closes [05 §8]: bad credentials fail at boot, IsValid() dead code deleted
public static class CloudinaryEnv
{
    public static readonly EnvVar<string> CloudName = EnvVar.Required("CLOUDINARY_CLOUD_NAME");
    public static readonly EnvVar<string> ApiKey = EnvVar.Required("CLOUDINARY_API_KEY");
    public static readonly EnvVar<string> ApiSecret = EnvVar.Required("CLOUDINARY_API_SECRET");
}
```

```csharp
// Schemas/SocialAuthEnv.cs
public static class SocialAuthEnv
{
    public static readonly EnvVar<string> FacebookAppId = EnvVar.Required("FACEBOOK_APP_ID");
    public static readonly EnvVar<string> FacebookAppSecret = EnvVar.Required("FACEBOOK_APP_SECRET");
    public static readonly EnvVar<string> GoogleClientId = EnvVar.Required("GOOGLE_CLIENT_ID");
}
```

```csharp
// Schemas/WebEnv.cs
public static class WebEnv
{
    public static readonly EnvVar<string> WebAppOrigin = EnvVar.Required("WEBAPP_ORIGIN").AbsoluteUrl();
    public static readonly EnvVar<string> DashboardOrigin = EnvVar.Required("DASHBOARD_ORIGIN").AbsoluteUrl();
    public static readonly EnvVar<string> FrontendBaseUrl = EnvVar.Required("FRONTEND_BASE_URL").AbsoluteUrl();
    public static readonly EnvVar<string?> TrustedProxyNetworks = EnvVar.Optional("TRUSTED_PROXY_NETWORKS");
}
```

```csharp
// Schemas/MailEnv.cs
public static class MailEnv
{
    // Absent value keeps local development on the Mailpit SMTP default.
    public static readonly EnvVar<string> Provider = EnvVar.Optional("EMAIL_PROVIDER", @default: "smtp");
}
```

```csharp
// Schemas/SecurityEnv.cs
public static class SecurityEnv
{
    public static readonly EnvVar<string> OtpPepper = EnvVar.Required("OTP_PEPPER").MinLength(16);
    public static readonly EnvVar<string> DefaultUserPassword = EnvVar.Required("DEFAULT_USER_PASSWORD").MinLength(8);
    public static readonly EnvVar<string?> RedisUrl = EnvVar.Optional("REDIS_URL");
}
```

```csharp
// src/Api/Program.cs — first statement, before the builder is configured
EnvSchema.ValidateAtBoot();
```

Boot output on a bad deploy:

```
Invalid environment configuration:
  JWT_SECRET must be at least 32 characters.
  POSTGRES_PASSWORD is missing or empty.
  CLOUDINARY_API_KEY is missing or empty.
```

Consumers read the descriptor directly — typed, non-nullable, no destructuring of tuple
positions they don't want:

```csharp
// before: var (secret, issuer, audience, _, _) = AppEnvironment.Jwt(); + null guard
new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtEnv.Secret.Value));
tokenDescriptor.Issuer = JwtEnv.Issuer.Value;
```

`JwtService.cs:38`'s first-use throw (`"JWT_SECRET env variable is missing or empty."`) is
deleted because boot already proved the value exists. Migration is group by group: add the
descriptor files, gate boot, move call sites one schema at a time (jwt, database, cloudinary,
…), delete the matching `AppEnvironment` tuple accessor as each group's last caller moves, and
delete `Environment.cs` when it is empty. The only
`Environment.GetEnvironmentVariable` call left in the codebase is the one inside
`EnvVar<T>.Value`.

Two behavioural decisions this encodes: `FRONTEND_BASE_URL`, `WEBAPP_ORIGIN`,
`DASHBOARD_ORIGIN` and the three social-auth keys become required (today they are silently
nullable and fail at first use), and `EMAIL_PROVIDER` defaults to `smtp` so local development
against Mailpit needs no configuration.

## Part C — Observability & hardening

### 11.5 Health + OTel

```csharp
builder.Services
    .AddHealthChecks()
    .AddNpgSql(_ => DatabaseEnv.ConnectionString(), name: "postgres")
    .AddRedis(_ => SecurityEnv.RedisUrl.Value!, name: "redis");

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddNpgsql().AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation().AddOtlpExporter());

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");
```

Correlation: a middleware stamping `X-Correlation-Id` (inbound or generated) into
`LogContext.PushProperty` so Serilog lines and OTel traces join on one id.

### 11.6 Kestrel limits, Swagger gate, headers

`Program.cs:117-119` runs Swagger unconditionally today. It becomes:

```csharp
builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.Limits.MaxRequestBodySize = 400 * 1024 * 1024; // the documented 350 MB video + headroom
});

builder.Services.Configure<FormOptions>(form =>
{
    form.MultipartBodyLengthLimit = 400 * 1024 * 1024;
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerFormatting();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});
```

The per-endpoint upload limit stays tighter via `.WithMetadata(new RequestSizeLimitAttribute(...))`
on the upload routes; the global cap is the ceiling, not the norm `[05 §9]`.

### 11.7 The three leftovers

**File-size strategy `[08 §4]`** — a `BadHttpRequestException` from the body-size limit currently
falls through to 500. One strategy in the existing pipeline shape:

```csharp
/// <summary>
/// Maps request-body-too-large failures to 413 instead of the generic 500.
/// </summary>
public class RequestBodyLimitExceptionStrategy : BaseExceptionStrategy<BadHttpRequestException>
{
    /// <inheritdoc />
    protected override int StatusCode => StatusCodes.Status413PayloadTooLarge;
}
```

**Versioning static `[01 §1.14]`** — `ApiVersionExtension.cs:14` holds
`private static RouteGroupBuilder? _rootVersionedGroup;`: process-wide mutable state that breaks
the second `WebApplicationFactory` in one process (the integration fixture works around it
today). The group moves to an instance held in DI (a small `RootVersionedGroupHolder` singleton
registered per host) and the static field is deleted.

**Account status `[07 S12]`** — in `AccountStatusRequirementHandler`, the
`catch { /* fall back to claims */ }` arm becomes `context.Fail(...)`, and the resolved status is
cached in `HttpContext.Items["account-status"]` for the request's remaining policy evaluations.

---

## Tests

- **Integration:** a request cancelled mid-flight does not complete its write (drive with a
  `CancellationTokenSource` on the client, assert no row). Boot the `ApiFixture` with `JWT__SECRET`
  removed → host start throws `OptionsValidationException` naming the member. `/health/ready`
  returns `Unhealthy` with the Postgres container stopped.
- **Unit:** each options class's annotations (missing/short/out-of-range); the body-limit
  strategy maps to 413; the account-status handler fails closed when the lookup throws.

---

## Rollout

`ValidateOnStart` makes missing configuration a boot failure: `.env.template`, compose, CI and
production must have every required variable **before** this deploys. The flat→sectioned env-var
mapping keeps old names alive one release; remove it in the next.

---

## Verification

1. Build/format/unit/integration green.
2. `grep -rn "Environment.GetEnvironmentVariable" src/` → empty.
3. `grep -rn "= default" src/Shared/Shared.Contracts/Application/CQRS/IDispatcher.cs` → empty.
4. `curl -s localhost:5025/swagger` in a Production-env container → 404.

---

**PR:** `feat(platform): cancellation tokens, validated config, health checks and headers`
