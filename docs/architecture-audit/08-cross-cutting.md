# 08 — Cross-Cutting Concerns

Scope: concerns that cut across all of `src` — localization/i18n, the error/HTTP contract,
logging & observability, API design & versioning, configuration, and non-auth security posture.

Some of this overlaps the composition-root file ([01](01-composition-root-and-shared-kernel.md))
and the Identity file ([07](07-identity-and-security.md)) — the same rate-limit, logging, and
config findings surface from more than one angle. This file is the cross-module view: the error
pipeline, the localization system end to end, observability, and API-surface consistency. Several
strengths are real: resource-key completeness is exact (515/515/515), mass assignment is genuinely
controlled, SQL is parameterized throughout, and no exception is swallowed.

---

## 8.1 Rate-limit policies are global buckets, not per-caller partitions

**Severity: Critical** · see [01 §1.1](01-composition-root-and-shared-kernel.md),
[07 S6](07-identity-and-security.md). All 293 endpoints share one counter per policy —
`ContentBrowsing` caps the whole deployment at 100 req/min, login at 5/min globally. Fix: partition
on subject-then-IP (after §8.20 fixes proxy trust).

---

## 8.2 `LoggingDecorator` writes plaintext passwords, OTP codes and refresh tokens to Seq

**Severity: Critical** · see [01 §1.2](01-composition-root-and-shared-kernel.md). 24 of 203
command records carry a secret and are serialized in full at Information level, before validation.
Fix: stop logging payloads; mask; rotate and purge.

---

## 8.3 `DefaultExceptionHandler` returns the raw exception message and .NET type name to the client

**Severity: Critical** · AREA: error handling.

**Where:** `DefaultExceptionHandler.cs:17` sets `Detail = exception.Message` at 500 with no
environment check; it is the fallback for every unmapped exception. There are 998 `IsUnique()`
indexes.

**Problem/why.** A concurrent signup produces `DbUpdateException` → this handler → HTTP 500 whose
body contains `23505: duplicate key value violates unique constraint "ix_users_email" DETAIL:
Key (email)=(victim@example.com) already exists` — a working enumeration oracle disclosing schema,
index names, and the probed value. The same path leaks connection strings and file paths.

**Solution.** Inject `IHostEnvironment`; outside Development return a fixed localized `Detail` +
`traceId`. Add a `DbUpdateExceptionStrategy` mapping `23505` → 409 with a value-free message. Apply
the same sanitization to `InternalServerExceptionHandler`.

---

## 8.4 Whole classes of exception have no strategy and fall through to 500 — including a file-size limit the app cannot honour

**Severity: High** · AREA: error handling.

**Where:** exception strategies are discovered from the **Shared assembly only**; Content, Core, and
Mailer register zero. Unmapped and thrown: `StreamingLinkResolutionException` (echoes the upstream
error), `EmailDeliveryException`, `DbUpdateException`, `BadHttpRequestException`. Body-size mismatch:
`MaxVideoFileSizeBytes = 350 MB` but no Kestrel/form limits configured, so the 30 MB default applies.
Separately, 7 exception types and 6 strategies are never thrown/used — pure ceremony that makes the
table look complete.

**Problem/why.** Any video between 30 and 350 MB is rejected by Kestrel with a non-ProblemDetails 413
the clients can't parse. Odesli being down turns an admin click into a 500 whose body carries the
upstream text.

**Solution.** Configure Kestrel/form body limits (scoped per upload endpoint). Add
`BadHttpRequestExceptionStrategy` + `DbUpdateExceptionStrategy`. Give Content/Mailer a
`Register*ExceptionStrategies` block mapping their exceptions to 502/429. Delete the 7 dead exceptions
and 6 dead strategies. Change strategy discovery to span the module assemblies.

---

## 8.5 Validation failures return raw `ValidationFailure` objects (echoing the submitted value) and don't match the OpenAPI contract

**Severity: High** · AREA: error handling · overlaps [06 §13](06-content-application.md).

**Where:** `ValidationExceptionHandler.cs:24` puts `exception.Errors` (a list of `ValidationFailure`)
straight into `Extensions["errors"]`. `ValidationFailure` serializes `AttemptedValue`. 124 endpoints
declare `.ProducesValidationProblem()` (the RFC 7807 `{field: [string]}` shape).

**Problem/why.** A signup with a weak password returns a 400 whose body contains `"attemptedValue":
"<the plaintext password>"` — landing in client error telemetry and proxy logs. And `errors` is an
array of objects while every generated client expects `Dictionary<string,string[]>`, so field-level
error display is broken on all 124 endpoints.

**Solution.** Project to the RFC 7807 shape (`GroupBy PropertyName → string[]`), drop `AttemptedValue`,
emit `ValidationProblemDetails`, and replace `detail` with a localized message.

---

## 8.6 `pageSize` is unbounded on 103 of 106 list endpoints

**Severity: High** · same as [06 §2](06-content-application.md). The `[Range]` annotation on
`PaginatedRequest` is never executed; `?pageSize=100000000` materializes the whole table.
`?pageIndex=-1000` yields a negative `Skip` and an unmapped 500. Fix: enforce the clamp inside the
`PaginatedRequest` constructor; also cap the 4 unbounded `limit` query builders.

---

## 8.7 No security headers, no HSTS, no HTTPS redirection, and Swagger UI is published unconditionally

**Severity: High** · AREA: security · overlaps [01 §1.11](01-composition-root-and-shared-kernel.md).

**Where:** `Program.cs:96-114` — `UseSwagger`/`UseSwaggerUI` with no env check; no `UseHsts`/
`UseHttpsRedirection`/security-header middleware anywhere; `appsettings.json:43` `"AllowedHosts":
"*"`; no `appsettings.Production.json`.

**Problem/why.** `/swagger` publishes all 293 routes and every schema (with a live request console)
to unauthenticated traffic — a reconnaissance map. Absent `nosniff`, a proxied user upload can be
MIME-sniffed into script. Absent HSTS, the cookie-based token delivery is strippable on first
contact.

**Solution.** Gate Swagger behind `IsDevelopment()` (or SuperAdmin auth for staging). Add a
`SecurityHeadersMiddleware` (`nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy`, a restrictive
CSP) plus `UseHsts`/`UseHttpsRedirection` outside Development. Replace `AllowedHosts: "*"` and add
`appsettings.Production.json`.

---

## 8.8 CORS fails open to `AllowAnyOrigin` when the origin env vars are unset

**Severity: High** · AREA: security.

**Where:** `Program.cs:57-71` — when `allowedOrigins.Length == 0` the policy becomes
`AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()`. The origin vars are unvalidated (§8.10).

**Problem/why.** A production deploy that forgets one env var silently serves the whole API to every
origin — indistinguishable from correct operation in logs. Bearer-token clients returning tokens in
the body are then cross-origin readable from any attacker page holding a stolen token.

**Solution.** Fail closed — throw at startup when origins are empty outside Development. Also fix the
ordering: `UseCors()` sits *after* `UseAppExceptionHandler()`, so error responses are written before
CORS headers attach and browsers see a network error instead of the 4xx body — move `UseCors()`
above the exception handler.

---

## 8.9 Localization has leaked into the domain: 74 domain-entity method signatures take an i18n error factory

**Severity: High** · AREA: i18n · same root as [03 §6](03-content-domain.md), [07 A4](07-identity-and-security.md).

**Where:** 74 signatures across 22 `Domain/` files take an `Errors`/`CoreI18n` parameter; those
factories are `IStringLocalizer`-backed. So `ContentOrderEntity.Submit` transitively depends on
`Microsoft.Extensions.Localization` and request-scoped culture.

**Problem/why.** `ContentOrderEntity.Submit` can't be invoked from a Quartz job without manufacturing
a localizer and a culture; every invariant threads a presentation parameter; and the message a domain
rule produces is decided by whoever happens to make the HTTP call.

**Solution.** Domain throws a code-only `DomainException`; a `DomainExceptionStrategy` translates at
the edge, reusing the existing `.resx` keys renamed to the code. Application-layer error throws
(handlers) stay. Roll out Core → Identity → Content. See [03 §6](03-content-domain.md).

---

## 8.10 Configuration is read ad hoc from env vars and `IConfiguration` indexers with silent defaults; no options binding, no startup validation

**Severity: High** · AREA: config · same as [01 §1.10](01-composition-root-and-shared-kernel.md).

**Where:** 19 `Environment.GetEnvironmentVariable` + 13 raw `IConfiguration[...]` reads; one
`IOptions` in the whole codebase (framework plumbing); `ValidateOnStart` 0 hits. Every accessor
returns `string?` with a fallback (`SMTP_HOST ?? "localhost"`, `SMTP_PORT → 1025`).

**Problem/why.** A production deploy missing `SMTP_HOST` starts cleanly, points at `localhost:1025`,
and drops every OTP/reset email into the outbox retry loop. Missing `RESEND_API_KEY` authenticates
with `""`. Missing `JWT_SECRET` reaches the signer as `null`. None fail process health, and there is
no health check to notice (§8.12).

**Solution.** Option records with data annotations, bound via
`AddOptions<T>().Bind(...).ValidateDataAnnotations().Validate(...).ValidateOnStart()` — turning every
one of these into a named startup crash. Replace the static calls and indexer reads. Add a
`.env.template` ↔ options drift check to CI.

---

## 8.11 Error responses are not RFC 7807: no `type`, wrong `Content-Type`, and `traceId` correlates to nothing

**Severity: Medium** · AREA: error handling + logging.

**Where:** `ProblemDetails.Type` is never assigned; `Title` is a .NET class name. `AddProblemDetails()`
is registered but bypassed — `ExceptionHandler.cs:56` uses `WriteAsJsonAsync`, setting
`application/json` not `application/problem+json` (the two hand-written JWT paths get it right, so the
API emits two content types for one shape). `traceId` is `TraceIdentifier`, which is pushed into no
`LogContext`, so no other log line for the request carries it.

**Problem/why.** Clients must string-match a class name that changes on rename. Strict RFC 7807
consumers reject `application/json`. The support workflow is broken: a user's `traceId` appears on
exactly one Seq event (the exception), so you can't reconstruct what the request did.

**Solution.** Add a stable `Type` + `errorCode` from a const table; route the write through the
registered `IProblemDetailsService` so the content type is correct; add a correlation middleware that
sets `TraceIdentifier`, echoes `X-Correlation-Id`, and pushes it into `LogContext` for the request
scope.

---

## 8.12 No health checks, no tracing, no metrics; Serilog ships to a hardcoded `localhost` Seq labelled "Development"

**Severity: Medium** · AREA: logging/observability.

**Where:** `grep AddHealthChecks|OpenTelemetry|AddMetrics` → 0. `appsettings.json` ships to
`http://localhost:5341` and stamps `Application: "EShop ASP.NET App"`, `Environment: "Development"` on
every event; no `appsettings.Production.json`; no `116_api` healthcheck. `LoggingDecorator` emits 2
Information events per handler on top of the request-completed event.

**Problem/why.** Orchestrators can't determine liveness/readiness — a pod with an exhausted
connection pool keeps receiving traffic. Production and staging logs are indistinguishable in Seq.
Outbound calls propagate no `traceparent`, so a slow Odesli/Resend/Cloudinary call is invisible as a
cause.

**Solution.** Add health checks (`AddNpgSql` + a Cloudinary check), map `/health/live` and
`/health/ready`, wire a container healthcheck. Add OpenTelemetry tracing/metrics (which also closes
§8.11's correlation gap via `Serilog.Enrichers.Span`). Parameterize the Seq URL, fix the `Application`/
`Environment` properties, drop the decorator logging to Debug.

---

## 8.13 Versioning declares v2 but nothing exercises it, and the `V1` folder versions only the endpoint

**Severity: Medium** · AREA: API design.

**Where:** `ApiVersionExtension.cs:26` declares `HasApiVersion(2, 0)`; `MapApiVersionGroup(2)` × 0,
`V2` folders × 0. Only the endpoint lives under `V1/` — the command, handler, and the shared DTO sit
outside any version folder. `_rootVersionedGroup` is `private static` mutable state (breaks under
parallel test hosts — [01 §1.14](01-composition-root-and-shared-kernel.md)).

**Problem/why.** `GET /api/v2/...` matches the root group but no endpoint declares v2, so it returns
`UnsupportedApiVersion` — a version advertised in the response header that never works. When v2 is
actually needed, the only versioned artefact is the endpoint file; a response-shape change means
mutating the shared DTO (breaking v1) or forking by hand with no convention.

**Solution.** Remove the `HasApiVersion(2)` declaration until a v2 endpoint exists. Before the first
v2, mandate that response contracts live in `V{n}/` and never reference a `Shared/DTOs` type directly.
Replace the static field ([01 §1.14](01-composition-root-and-shared-kernel.md)). Document the v2
procedure.

---

## 8.14 7 endpoints violate the `/api/v{version}/{scope}/{resource}` pattern, and 101 endpoints defer GUID parsing into the handler

**Severity: Medium** · AREA: API design · overlaps [06 §10/§13](06-content-application.md).

**Where:** 3 `MapGroup` prefixes omit the scope segment, producing 7 unscoped public routes
(`/api/v1/translations/...`, `/api/v1/lyrics/submissions`, `/api/v1/artists/{id}/claim`), two of which
shadow a differently-scoped resource of the same name. `{id:guid}` appears 45 times, bare `{id}` 327;
101 handlers take `string id` and `Guid.Parse` in the application layer.

**Problem/why.** The scope segment is what the proxy/WAF/gateway uses to separate the authenticated
admin surface from the anonymous one — 7 endpoints outside it can't be covered by a `/admin/*` or
`/public/*` rule. A malformed id on 101 endpoints consumes the rate-limit budget, runs auth, is logged
in full, and only then throws `FormatException` — where `{id:guid}` would reject it at routing for
free.

**Solution.** Move the 7 endpoints under `Public` (301 alias for one release). Sweep bare `{id}` →
`{id:guid}` and `string id` → `Guid id`, deleting the handler `Guid.Parse`. Split the `promotion/feed`
constant.

---

## 8.15 Resource keys are duplicated across 32 files with no compile-time guarantee any key exists

**Severity: Medium** · AREA: i18n.

**Where:** 99 `.resx` files (32 sets × 3), **515/515/515** keys — no missing translations. But 38 keys
are defined in multiple files (`IdRequired`/`IdInvalid` in 13 each, `NameRequired` in 9). Access is by
string literal with no generated accessor, so a typo ships as the literal key in the response
(`IStringLocalizer` returns the key on a miss). Three keys are built dynamically;
`EmailTemplateMessage`'s `$"{template}Subject"` is unguarded, so a new enum member with no resource
mails a subject line of literally `NewTemplateSubject`.

**Problem/why.** The 38 duplicated keys mean a translator fixing `IdInvalid` fixes 1 of 13; the other
12 drift. With no compile-time binding, a renamed key is caught only when a user sees the raw key in a
toast.

**Solution.** Extract the 38 shared keys into a single `CommonErrorMessage` set (~180 fewer entries).
Add a CI check: every `localizer["..."]` literal must have a matching `.resx` entry, and every
`EnumEmailTemplate`/`EnumNotificationType` member must have its three keys. Guard `EmailTemplateMessage`
to throw on `ResourceNotFound`.

---

## 8.16 "Three languages" is two plus an English copy, and a dead second language middleware contradicts the negotiated culture

**Severity: Medium** · AREA: i18n.

**Where:** `LocalizationExtension.cs` supports `["fr","en"]`, default `fr`; the neutral fallback
`.resx` are English (match `.en` for 502/515, `.fr` for 0). `RequestCultureProviders` is replaced
wholesale (no query/cookie override). Then a second hand-rolled middleware writes
`HttpContext.Items["Language"]` — read **nowhere** — and resolves the header without q-values,
disagreeing with the framework provider.

**Problem/why.** Any client sending no `Accept-Language` (server-to-server, mobile defaults, curl)
gets French errors while the resource fallback is English — invisible until a French key is missing and
the user gets an English sentence mid-flow. The dead middleware looks like the language mechanism and
isn't; the next person to "fix" localization will edit it and see no effect.

**Solution.** Delete the dead `app.Use(...)` block. Restore the standard provider chain (query →
cookie → header). Make the default and the neutral resources agree — setting `DefaultCulture = "en"`
is a one-line, zero-translation fix since the neutral files are already English. Move the culture list
into the `IOptions` binding.

---

## 8.17 Emails and notifications render in the *acting caller's* culture, not the recipient's — and there is no per-user language

**Severity: Medium** · AREA: i18n.

**Where:** `EmailCulture.Current()` returns `CultureInfo.CurrentUICulture` (set from the incoming
request) at 18 sites. There is no language column on the user (`grep PreferredLanguage|Locale` in
Identity domain → 0). (Rendering itself correctly persists the culture with the outbox row.)

**Problem/why.** A French admin assigning a role sends the "your role changed" email in French to an
English user. Six notification handlers notify a *third party* whose language is unrelated to the
triggering request; jobs use the process default.

**Solution.** Add `PreferredLanguage` to `UserEntity` (default from the signup culture) and expose it
on `AuthorInfo`. Change the 18 sites to `user.PreferredLanguage ?? EmailCulture.Current()`, keeping the
request culture only for self-directed flows. Persist the subscribe-time culture on newsletter rows.

---

## 8.18 Several client-facing error messages are English string interpolation, bypassing the resource system

**Severity: Medium** · AREA: i18n.

**Where:** `DbSetExtension.cs:97,151,206` throw `new NotFoundException($"Could Not find
{entityName}.")` using the single-arg constructor (so `EntityName` stays null and the handler can't
localize) with the raw EF type name and a typo. `MethodNotAllowedException` and (as a log string)
`RateLimitExceededException` interpolate English; `ValidationExceptionHandler`'s `detail` is
FluentValidation's English frame. (33 factories correctly use the localized `NotFoundException(entityName,
key, value)` path.)

**Problem/why.** A French user hitting a 405 or a `DbSetExtension` fallback gets English mixed into a
French UI, and the fallback discloses internal type names (`ArticleEntity`) — the exact thing
`SharedExceptionMessage` exists to prevent.

**Solution.** Add localized keys for method-not-allowed and generic-not-found; change the
`DbSetExtension` fallbacks to the `(entityName, keyValue: null)` constructor so they route through the
localized path. Add a CI grep gate rejecting `new *Exception($"...")` outside `*Errors.cs`.

---

## 8.19 Response semantics are a `{isSuccess:true}` envelope over HTTP status; 28/28 DELETEs return a body, 0 use 204

**Severity: Low** · AREA: API design.

**Where:** `IsSuccess: true` in 99 endpoint responses (hardcoded — every non-`true` path already
threw). `Results.NoContent` × 0; all 28 DELETEs return `Results.Ok(...)` with a body; 8
resource-creating POSTs return 200 not 201.

**Problem/why.** `isSuccess` is a constant `true` — 99 endpoints' worth of payload and generated-client
surface carrying zero information. DELETE-with-body prevents intermediaries treating deletes as
bodiless. The 8 uploads returning 200 mean clients can't rely on the `Location` header.

**Solution.** Return `NoContent()` from the 99 (or the updated resource where the caller needs state);
convert the 28 DELETEs; give the 8 uploads `Results.Created(url, response)`. If PATCH-as-action stays,
document it as a decision in `CLAUDE.md`.

---

## 8.20 `ForwardedHeaders` trusts every proxy, making the recorded session IP client-controlled

**Severity: Low** · AREA: security · prerequisite for [01 §1.1](01-composition-root-and-shared-kernel.md)/§8.1.

**Where:** `Program.cs:87-92` sets `ForwardedHeaders.XForwardedFor | XForwardedProto` and clears both
`KnownNetworks` and `KnownProxies`, so `RemoteIpAddress` is whatever any caller puts in
`X-Forwarded-For`. That value is persisted to `SessionEntity.IpAddress`, exported, and queried.

**Problem/why.** Session audit records — the artefact for "where did this login come from?" — are
attacker-authored. And once IP-partitioned rate limiting lands, the header becomes a complete
rate-limit bypass by rotating a spoofed value.

**Solution.** Populate `KnownNetworks` from a `TRUSTED_PROXY_NETWORKS` config value with `ForwardLimit
= 1`; require at least one entry outside Development. Do this **before** the rate-limit partitioning
change.

---

## What is done well here

- **Resource-key completeness is exact** — 515/515/515 keys, zero missing translations; all 27 email
  templates have their Subject/Html/Text keys; all 33 thrown entity names have an `Entity_*` label.
- **`SharedExceptionMessage` gets not-found messaging right** — friendly localized labels that
  withhold the raw class name/key/value, with a `ResourceNotFound` guard.
- **Validator messages are fully localized** — 0 literal `WithMessage("...")` across `src`.
- **Rate-limit *coverage* is complete** — all 293 endpoints declare a policy; the taxonomy (sliding/
  token-bucket/fixed) is sound. Only the partitioning is wrong.
- **Mass assignment is genuinely controlled** — no `*Request` record exposes `Status`, `IsActive`,
  `CreatedBy`, `AuthorId`, `ViewCount`, etc.; actor identity comes from `ICurrentActor`, never the
  body.
- **SQL is parameterized throughout** — one raw-SQL site, `FromSqlInterpolated`, correctly bound; the
  `FOR UPDATE SKIP LOCKED` outbox claim is right.
- **No swallowed exceptions** — 38 `catch` blocks, 0 empty, 0 catch-and-return-null; the 6 that don't
  rethrow are deliberate and documented.
- **Secrets are not committed** — `.env` gitignored, `.env.template` placeholders only, `appsettings`
  carries an explicit "never store secrets here" note.
- **The email/notification rendering pipeline is careful** — culture captured at enqueue and re-applied
  at send, HTML token values encoded, a hard fail on any unresolved placeholder, and no recipient/body
  logged.
- **The exception-strategy mechanism is right** — hierarchy walk, memoized per concrete type, open for
  extension. The problems are gaps in *what* is registered/emitted, not the mechanism.
- **`Results.Created` is used correctly where used** — all 26 sites pass a real location.
