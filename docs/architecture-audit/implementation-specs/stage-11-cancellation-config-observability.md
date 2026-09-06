# Stage 11 — CancellationToken, typed configuration & observability

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
| D2 | Configuration | keep the static `Environment` class, or typed `IOptions` | **Typed options bound from `IConfiguration`.** The static class returns nullable tuples (`Jwt()` → 5 nullable strings) and every consumer re-validates or forgets to. `ValidateOnStart` moves the failure to boot with the variable's name in the message. Env-var names stay identical — `AddEnvironmentVariables` maps them, so deploys don't change. |
| D3 | What replaces the fallback in `AccountStatusRequirementHandler` | keep the DB-with-claims-fallback, or fail closed | **Fail closed, cache per request.** Today a DB error silently degrades to trusting JWT claims `[07 S12]` — an attacker who forces DB pressure gets stale-claims authorization. The DB check stays (Stage 4 put the fast path in the token), but on failure the requirement **fails**; `HttpContext.Items` caches the result so N policies on one request cost one lookup. |
| D4 | Observability stack | Serilog-only, or add OpenTelemetry | **OTel for traces/metrics, Serilog stays for logs.** Health checks via `AspNetCore.HealthChecks.*` for Npgsql + Redis. Seq URL and environment labels move into the typed options (`[08 §12]` — the sink is hardcoded `localhost` today). |

---

## Checklist

- [ ] 11.1 — `IDispatcher.Send` loses `= default`; build breaks enumerate the sweep
- [ ] 11.2 — Endpoint lambdas take `CancellationToken` and pass it; handlers/repositories complete the chain
- [ ] 11.3 — Typed options: `JwtOptions`, `DatabaseOptions`, `CloudinaryOptions`, `CorsOptions`, `MailOptions`, `OtpOptions` — all `ValidateOnStart`
- [ ] 11.4 — `Environment.cs` deleted; `JwtService` et al. take `IOptions<JwtOptions>`
- [ ] 11.5 — Health checks (`/health/live`, `/health/ready`) + OpenTelemetry + correlation middleware
- [ ] 11.6 — Kestrel/form limits sized for the 350 MB upload; Swagger gated; security headers + HSTS
- [ ] 11.7 — `FileSizeLimitExceeded` exception strategy; `_rootVersionedGroup` static removed; account-status fail-closed + per-request cache
- [ ] 11.8 — Tests: cancelled request aborts; boot fails on missing `JWT_SECRET`; `/health/ready` degrades
- [ ] 11.9 — Verify (build 0/0, csharpier, unit, integration)

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

## Part B — Typed configuration

### 11.3–11.4 Options with `ValidateOnStart`

Representative — `JwtOptions` replacing `Environment.Jwt()`'s five-nullable-string tuple:

```csharp
namespace _116.Shared.Application.Configurations;

/// <summary>
/// JWT signing and lifetime configuration, bound from the JWT_* environment variables.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public required string Secret { get; init; }

    [Required]
    public required string Issuer { get; init; }

    [Required]
    public required string Audience { get; init; }

    [Range(1, 24 * 60)]
    public int AccessTokenExpirationMinutes { get; init; } = 60;

    [Range(1, 365 * 24 * 60)]
    public int RefreshTokenExpirationMinutes { get; init; } = 43_200;

    [Range(1, 365)]
    public int SessionAbsoluteLifetimeDays { get; init; } = 90;
}
```

```csharp
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

Env-var names are preserved through the double-underscore convention (`JWT__SECRET`); a small
`ConfigurationManager` mapping keeps the existing flat names (`JWT_SECRET`) working during the
transition so deploys don't have to change the same day.

Consumers switch from the static read to constructor injection — `JwtService.cs:38`'s
first-use throw (`"JWT_SECRET env variable is missing or empty."`) is deleted because boot
already proved the value exists:

```csharp
public class JwtService(IOptions<JwtOptions> jwtOptions) : IJwtService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    // _jwt.Secret is non-null by construction; the guard clause goes away.
}
```

Same shape for `DatabaseOptions` (connection string + pool cap for Stage 10's resilience knobs),
`CloudinaryOptions` (closes `[05 §8]` — bad credentials fail at boot, and `IsValid()` dead code
is deleted), `CorsOptions`, `MailOptions` (SMTP/Resend), `OtpOptions` (the pepper).
`Environment.cs` is deleted when its last reader is gone.

## Part C — Observability & hardening

### 11.5 Health + OTel

```csharp
builder.Services
    .AddHealthChecks()
    .AddNpgSql(sp => sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString, name: "postgres")
    .AddRedis(sp => sp.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString, name: "redis");

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
