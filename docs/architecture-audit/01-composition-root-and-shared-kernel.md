# 01 — Composition Root & Shared Kernel

Scope: `src/Api`, `src/BuildingBlocks`, `src/Shared`, the CQRS dispatch pipeline, DI
registration, rate limiting, configuration, and container/deployment wiring.

The mechanisms here are, in several places, unusually well thought through — the
domain-event interceptor timing, the exception-strategy registry, and the `TimeProvider`
seam are all genuinely good. The problems are concentrated in three areas: request
security (rate limiting, credential logging, error leakage), the reflection-based
dispatcher, and the shared kernel carrying the entire web stack into every domain layer.

---

## 1.1 Rate-limit policies are global, not partitioned — 3 OTP requests/min for the whole site

**Severity: Critical**

**Where:** `src/Shared/Shared/Application/Builders/RateLimit/FixedWindowBuilder.cs:23`,
`SlidingWindowBuilder.cs:26`, `TokenBucketBuilder.cs:31`; limits in
`src/BuildingBlocks/Constants/RateLimit/OtpRateLimitConstants.cs:13` (`PermitLimit = 3`).

**Problem.** All three builders call the non-partitioned
`options.AddFixedWindowLimiter(name, …)` overload, which creates **one limiter instance
shared by every request** to that policy. There is no partition key (IP, user, account)
anywhere in the codebase.

**Why it's a problem.** The OTP policy permits 3 requests per minute *across all users
combined*; `Authentication` permits 5 logins/min site-wide; `ContentBrowsing` caps the
whole public site at 100 reads/min. So the limiter simultaneously (a) guarantees
self-inflicted denial of service the moment a handful of users are active, and (b) gives
zero per-attacker protection — an attacker spreading 3 guesses/min across accounts still
gets unlimited attempts against any single account. The doc comments claim brute-force
and enumeration protection that the implementation does not provide.

**Solution.** Switch to partitioned limiters keyed on the authenticated subject with a
client-IP fallback.

1. Add a partition-key resolver in `_116.Shared.Application.Builders.RateLimit`:
   authenticated `ClaimTypes.NameIdentifier`, else `RemoteIpAddress`.
2. Rewrite each builder to `options.AddPolicy(name, ctx => RateLimitPartition.GetFixedWindowLimiter(ResolveKey(ctx), …))`.
3. For `Authentication`/`Otp`/`PasswordManagement`, partition on the **submitted account
   identifier** as well as IP, otherwise a botnet defeats the IP partition.
4. Before running more than one replica, back the limiters with the Redis package already
   referenced in `Shared.csproj:31` — in-process partitions are per-pod.

No endpoint edits — the 293 `RequireRateLimiting(policyName)` call sites are unchanged.
Also fix [8.x forwarded-headers trust](08-cross-cutting.md) first, or an IP partition is
spoofable.

---

## 1.2 `LoggingDecorator` writes plaintext passwords, OTP codes and tokens to logs

**Severity: Critical**

**Where:** `src/Shared/Shared/Application/Decorators/LoggingDecorator.cs:43`; applied to
every handler at `src/Shared/Shared/Application/Extensions/CqrsExtension.cs:62`. Secret-
bearing commands e.g. `PublicLoginCommand.cs:17` (`Credentials`, `Password`),
`PublicResetPasswordCommand.cs:15` (`Email`, `Code`, `NewPassword`).

**Problem.** `logger.LogInformation("… RequestData={RequestData}", …, request)` logs the
request with no `@` destructuring, so Serilog calls the record's compiler-generated
`ToString()`, which prints every member:

```
[START] Handling PublicLoginCommand - RequestData=PublicLoginCommand { Credentials = a@b.com, Password = hunter2 }
```

at Information level (`appsettings.json:5`), shipped to Console and Seq.

**Why it's a problem.** Every login, signup, password reset and OTP verification deposits
live credentials into a log store whose access boundary differs from the password-hash
table. This is a reportable credential-disclosure incident; remediation means purging log
history and rotating any secret that transited these endpoints, not just a code change.

**Solution.**
1. Stop logging payloads by default — log `typeof(TRequest).Name` plus a correlation id
   only. Do this first and immediately.
2. If diagnostic payload logging is wanted, add an `ISensitiveRequest` marker and only
   `LogDebug("{@RequestData}", request)` for non-sensitive requests, gated on
   `IsEnabled(Debug)`.
3. Add a Serilog `Destructure.ByTransforming<T>` masking policy as belt-and-braces.
4. Purge/redact existing Seq data; rotate exposed credentials.

---

## 1.3 Unhandled exception messages are returned verbatim to the client

**Severity: High**

**Where:** `src/Shared/Shared/Application/Exceptions/Handlers/Strategies/DefaultExceptionHandler.cs:17`
(`Detail = exception.Message`, status 500); it is the registry fallback
(`ExceptionStrategyRegistry.cs:69`) and is written unconditionally
(`ExceptionHandler.cs:55`) with no environment check.

**Problem.** Any exception with no registered strategy — `NpgsqlException`,
`DbUpdateException`, EF translation failures, SDK errors — has its `.Message` shipped to
anonymous callers in production.

**Why it's a problem.** Npgsql messages embed host/database/username; `DbUpdateException`
embeds table and constraint names; EF embeds the failing LINQ. Free reconnaissance on
public endpoints. Separately, `OperationCanceledException` (client disconnect) also lands
here → logged at Error and answered 500, poisoning error-rate alerting.

**Solution.**
1. Inject `IHostEnvironment`; outside Development return a fixed `Detail` plus the
   existing `traceId`.
2. Add an `OperationCanceledExceptionHandler` returning 499 and mapped to `Debug`
   (auto-discovered, no registration edit).
3. Return `false` from `TryHandleAsync` when `Response.HasStarted` instead of throwing on
   a partially written response.

---

## 1.4 The `Dispatcher` uses per-request reflection on the hot path, with an ambiguous cache

**Severity: High**

**Where:** `src/Shared/Shared/Application/Services/Dispatcher.cs:25` (`GetMethod` +
`MethodInfo.Invoke` per request), `:15` (cache keyed on request type only), `:32`
(missing handler is a runtime throw).

**Problem.** Three defects: (a) only the handler *type* is cached; the `MethodInfo`
lookup and `Invoke` (with argument boxing) run on **every request**; (b) the static
`HandlerTypeCache` is keyed on request type alone while two `GetHandlerType` overloads
write into it, so the response type is not part of the key — silent ambiguity; (c)
handler resolution fails at runtime, not build time.

**Why it's a problem.** The one code path every HTTP request traverses is the slow,
allocation-heavy one — while `DomainEventPublisher.BuildDispatchPlan` right next door
already does this correctly with a cached compiled expression. The void-command branch
(`Dispatcher.cs:56`) also bypasses `ValidationDecorator`/`LoggingDecorator` entirely
because `Decorate` only targets `IRequestHandler<,>` — a landmine for the first
`ICommand` (void) anyone adds.

**Solution.** Delete the reflection. Resolve the closed handler via
`typeof(IRequestHandler<,>).MakeGenericType(...)` and invoke through a compiled
`Expression.Lambda` cached on `(requestType, responseType)` — copy
`DomainEventPublisher.BuildDispatchPlan`. Add a boot-time guard that scans for every
`IRequest<>` without a registered handler and throws at startup. Decorate the void path
too.

---

## 1.5 No `CancellationToken` reaches any handler — all 293 endpoints drop it

**Severity: High**

**Where:** every `*EndpointV*.cs` under `src/Modules/` — 293 of 293 pass no token to
`dispatcher.Send(...)` (e.g. `PublicLoginEndpointV1.cs:70`). The plumbing accepts one
throughout; it is never supplied.

**Problem.** The token defaults to `CancellationToken.None`, so `None` flows to EF Core,
`HttpClient`, MailKit and Cloudinary.

**Why it's a problem.** When a client aborts, the server keeps the DbContext, the pooled
Npgsql connection and any outbound call alive to completion. Under load, aborted requests
accumulate and exhaust the shared Npgsql pool, taking down healthy traffic — while every
caller has already given up.

**Solution.** Minimal APIs bind `CancellationToken` for free. Mechanical per endpoint:
add `CancellationToken ct` to the delegate, pass it to `Send(...)`. Scripted rewrite, one
module at a time, verifying with `grep -L CancellationToken`. Add a CI grep gate for new
endpoints.

---

## 1.6 Two modules register `TypeAdapterConfig` as a singleton — Identity's mappings are discarded

**Severity: High**

**Where:** `src/Modules/Identity/Identity/IdentityModule.cs:129` and
`src/Modules/Content/Content/ContentModule.cs:130` both
`services.AddSingleton(MappingRegistration.CreateConfiguration())`; Identity is registered
first (`Program.cs:78`).

**Problem.** Two `AddSingleton<TypeAdapterConfig>` calls mean
`GetRequiredService<TypeAdapterConfig>()` returns the **last** one — Content's. Every
`IMapper` in the app, including those injected into Identity handlers, is built over a
config containing none of Identity's `NewConfig` rules (`UserMapper.cs:24`), which then
fall through to convention mapping — the exact thing those rules exist to override.

**Why it's a problem.** Order-dependent and invisible: the two `MappingRegistration`
classes share a name, so nothing at the call site reveals the collision. Reordering the
`Program.cs` lines flips which module's mappings work; a fifth module breaks whichever ran
last-but-one.

**Solution.** Give each module its own config and mapper via keyed services
(`AddKeyedSingleton("identity", …)` / `[FromKeyedServices("identity")] IMapper`), or
compose both `CreateConfiguration()` outputs into one config at a single point in
`Program.cs`. Either way add a boot assertion that exactly one `TypeAdapterConfig`
descriptor exists.

---

## 1.7 Domain events are fire-and-forget with no outbox — handler failures are logged and dropped

**Severity: High** · shared root cause with [02 §13](02-module-boundaries.md) and the
Content event handlers.

**Where:** `src/Shared/Shared/Infrastructure/interceptors/DispatchDomainEventsInterceptor.cs:172`
(per-event `catch (Exception) { LogError }`), `DomainEventPublisher.cs:79` (same per
handler), `:28` (buffer is an in-memory `ConditionalWeakTable`).

**Problem.** The commit-then-dispatch ordering and per-event scope are correct, but there
is no durability. A SIGKILL, pod eviction, or transient handler error between commit and
dispatch loses the reaction with only a log line.

**Why it's a problem.** These handlers do real work: `OrderPaidEvent → receipt email`,
`RefreshTokenReplayDetectedEvent → security response`, file cleanup, invoices. The order
is paid and the customer never gets a receipt; a replay is detected and no response fires.
No retry, no dead-letter, no record the reaction was owed. Mailer already has the right
pattern (`OutboxEmailDispatcherJob`) but domain events themselves are not persisted.

**Solution.** Add a transactional outbox for domain events. Persist serialized events into
a per-module `outbox_messages` table **inside the same `SaveChanges`** (in
`SavingChanges`), keep in-process dispatch as the fast path marking rows processed, and
add an `IScheduledJob` (the `AddScheduledJob`/Quartz mechanism already exists) that
replays unprocessed rows with backoff and dead-letters after N attempts. Do [§1.8](#18)
first so events have a stable identity.

---

## 1.8 `IDomainEvent.EventId` mints a new Guid on every read; `CreatedAt` uses local time

**Severity: High** · prerequisite for [§1.7](#17) and [02 §13](02-module-boundaries.md).

**Where:** `src/Shared/Shared/Domain/IDomainEvent.cs:7`:

```csharp
Guid EventId => Guid.NewGuid();
public DateTime CreatedAt => DateTime.Now;
public string EventType => GetType().AssemblyQualifiedName!;
```

**Problem.** These are default interface members, not stored state. `EventId` is a Guid
*factory* — reading it twice returns different values. `CreatedAt` is server-**local**
time (`Kind = Local`) while everything else is UTC, and Npgsql rejects `Local` for
`timestamptz`. `EventType` bakes in assembly version + public key token.

**Why it's a problem.** Deduplication, idempotency keys, correlation, and any outbox are
impossible to build on an identity that changes per read. Persisting `EventType` makes
stored events unreadable after any version bump.

**Solution.** Make them stored state set once at construction: `EventId { get; }` (a
`Guid.CreateVersion7()`), `OccurredOn { get; }` from `TimeProvider`, `EventType =
GetType().FullName`. Add an `abstract record DomainEvent : IDomainEvent` base, stamp in
`Aggregate.AddDomainEvent`, migrate event records module by module, then make the
interface members abstract last so the compiler lists any missed. This is a small change
with outsized value — land it independently.

---

## 1.9 `Shared` is a dumping ground that drags the web stack (and Bogus) into every domain layer

**Severity: High**

**Where:** `src/Shared/Shared/Shared.csproj:13` references Carter, Swashbuckle,
Npgsql/EF, Quartz, Asp.Versioning, DotNetEnv, **Bogus** (a fake-data generator), and
**Mapster 7.4.2-pre02** (a prerelease). Every module references `Shared` as one unit.
`IRepository<T>` (`Domain/IRepository.cs:7`) is an empty marker; `Entity<T>.Id`
(`Domain/Entity.cs:9`) has a public setter and publicly mutable audit fields.

**Problem.** A `Content.Domain.ArticleEntity` transitively references Npgsql, Carter,
Swashbuckle and Quartz because `Aggregate<Guid>` lives in this project. The dependency
rule exists in folder names only; the build graph does not enforce it. Bogus ships to
production. `IRepository<T>` is inherited by 27 interfaces and adds nothing. `Entity.Id`'s
public setter lets application code rewrite an aggregate's identity or forge `CreatedBy`.

**Why it's a problem.** Nothing stops a domain entity typing a `DbContext` or
`HttpContext` — the compiler allows it. The encapsulation DDD depends on is open at the
base class.

**Solution.** Split `Shared` along the dependency rule into three projects:
`Shared.Domain` (zero packages — `Aggregate`, `Entity`, `IDomainEvent`, specifications;
make `Id` `protected set`, audit fields `internal set`), `Shared.Application`
(FluentValidation + Mapster — decorators, `IUnitOfWork`, pagination, exceptions), and
`Shared.Web` (Carter/EF/Quartz/Swashbuckle host wiring). Namespaces already differ by
folder, so the move is largely mechanical; tighten module `.csproj` references one at a
time and the compile errors are the dependency-rule violations. Move Bogus to the test
fixtures project; pin Mapster to a stable release. Delete `IRepository<T>` or give it real
members.

---

## 1.10 Configuration is read from static `Environment.GetEnvironmentVariable` — no binding, no validation

**Severity: Medium**

**Where:** `src/Shared/Shared/Application/Configurations/Environment.cs:30`;
`BaseModule.cs:106` builds the connection string from nullable tuples; `IdentityModule.cs:199`
reads the JWT secret with `!`; `CloudinaryExtensions.cs:20` coalesces missing secrets to
`""`. `IOptions<T>` appears once in the whole codebase.

**Problem.** Every value is nullable, nothing is validated, `IConfiguration`/user-secrets/
parameter-stores are bypassed, and `CloudinarySettings.IsValid()` is dead code.

**Why it's a problem.** A missing `POSTGRES_HOST` yields `Host=;Port=;…` and an opaque
Npgsql error at first query, not at boot. A missing `JWT_SECRET` throws a bare
`ArgumentNullException` from `GetBytes`. A short secret is accepted, producing forgeable
HS256 tokens. Statics are also untestable without mutating process env vars (racy under
parallel xUnit).

**Solution.** Define option POCOs (`DatabaseOptions`, `JwtOptions`, `CloudinaryOptions`,
…) with data annotations (`[Required]`, `[MinLength(32)]`), bind via
`AddOptions<T>().Bind(...).ValidateDataAnnotations().ValidateOnStart()`.
`ValidateOnStart` turns every misconfiguration into a named boot failure. Delete
`AppEnvironment` and `AddCloudinaryConfiguration`.

---

## 1.11 Swagger UI, fail-open CORS, and unbounded proxy trust are active in production

**Severity: Medium**

**Where:** `src/Api/Program.cs:97` (`UseSwagger`/`UseSwaggerUI`, no env guard), `:57`
(CORS falls back to `AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()` when origins
empty), `:87` (`KnownNetworks.Clear()` + `KnownProxies.Clear()`). No `UseHsts`/
`UseHttpsRedirection` anywhere.

**Problem/why.** Swagger publishes the full admin attack surface and a live request
console to anonymous callers in prod. CORS fails **open** if the origin env vars are unset
or misspelled (plausible given §1.10) — invisibly. `KnownProxies.Clear()` trusts
`X-Forwarded-For` from any source, letting a direct caller assert an arbitrary client IP —
which defeats the IP-partitioned rate limiter proposed in §1.1.

**Solution.** Guard Swagger behind `IsDevelopment()` (or an explicit flag + auth). Make
CORS throw at startup when origins are empty outside Development. Replace the proxy clears
with the real ingress CIDR (or at minimum `ForwardLimit = 1`) — do this before §1.1. Add
`UseHsts` + `UseHttpsRedirection`.

---

## 1.12 Migrations & seeding run inside pipeline construction — no lock, no health gate

**Severity: Medium** · overlaps [04 §9/§10](04-content-infrastructure.md).

**Where:** `src/Api/Program.cs:114`; `ApplicationBuilderExtension.cs:26`
(`MigrateDatabaseAsync(...).GetAwaiter().GetResult()`); `IdentityModule.cs:91`
(`EnableMigrations = !Testing`, i.e. on in Production). No `AddHealthChecks` anywhere; no
`healthcheck` block for `116_api`.

**Problem/why.** Four DbContexts migrate the same database at boot with no advisory lock —
concurrent replicas race the DDL lock and one crash-loops. The blocking
`.GetAwaiter().GetResult()` has no timeout. There is no readiness probe, so traffic routes
to a pod mid-migration.

**Solution.** Add `AddHealthChecks().AddDbContextCheck<…>()` for all four contexts, map
`/health/live` and `/health/ready`. Move migration out of the pipeline into a deployment
job (preferred) or an `IHostedService`, and set `EnableMigrations = false` in Production.
If it must stay in-process, wrap it in `pg_advisory_lock`. See
[04 §10](04-content-infrastructure.md) for the destructive-migration angle.

---

## 1.13 The `IDataSeeder` / `UseSeed` infrastructure is dead

**Severity: Medium**

**Where:** `ApplicationBuilderExtension.cs:71` resolves `GetServices<IDataSeeder>()`; the
three seeders implement `IDataSeeder` but are registered only as concrete types
(`IdentityModule.cs:179`, `ContentModule.cs:245`), then invoked by hand
(`IdentityModule.cs:251`, `ContentModule.cs:263`).

**Problem/why.** `GetServices<IDataSeeder>()` returns empty, so `UseSeed()` and the whole
`ModuleOptions.EnableSeeding` contract are a permanent no-op — seeding works only because
two modules duplicated the logic. The next author will set `EnableSeeding = true`,
register a seeder, and watch it never run.

**Solution.** Register against the interface, delete the manual blocks. First fix the
double-execution hazard this exposes (`UseModuleDatabase` runs per module ×4): filter
seeders by module, e.g. `IEnumerable<IDataSeeder<TDbContext>>`. Thread a
`CancellationToken` and await from the hosted service in §1.12.

---

## 1.14 `ApiVersioningExtensions` holds the root route group in a static mutable field

**Severity: Medium**

**Where:** `src/Shared/Shared/Application/Extensions/ApiVersionExtension.cs:14`
(`private static RouteGroupBuilder? _rootVersionedGroup`), set in `UseApiVersioning`, read
by every endpoint.

**Problem/why.** Process-wide state transitively roots an entire `WebApplication`. Two
`WebApplicationFactory<Program>` instances (xUnit parallelism, or one disposed while
another builds) overwrite the field — endpoints from host B append to host A's group, or a
disposed host's group is mutated → nondeterministic route corruption presenting as flaky
404s. A disposed host is never collected.

**Solution.** Replace with a DI-registered `VersionedRouteGroupProvider` singleton whose
lifetime is the host's; read it via `app.ServiceProvider` in `MapApiVersionGroup`. The 293
call sites are unchanged.

---

## 1.15 The Dockerfile omits the Mailer projects — the container build cannot restore

**Severity: Medium**

**Where:** `Dockerfile:6` copies each `.csproj` individually but has no `COPY` for
`src/Modules/Mailer/Mailer/*.csproj` or `Mailer.Contracts/*.csproj`; `dotnet restore`
(no arg) restores `116_backend.sln`, which lists them.

**Problem/why.** Restore fails with MSB3202 — the image never builds for any commit after
Mailer was added. Nothing in CI builds the image, which is why it went unnoticed. Also:
the cache mounts target non-existent `/src/obj` and `/src/bin`; the test-project copies
pull test deps into the production build for nothing.

**Solution.** Add the two Mailer `COPY` lines. Then make this class of drift impossible:
`RUN dotnet restore src/Api/Api.csproj` (transitive graph only), drop the test-project
copies and the bogus cache mounts, and add a `docker build` step to CI.

---

## What is done well here

- **Domain-event dispatch timing** — `DispatchDomainEventsInterceptor` collects in
  `SavingChanges`, publishes in `SavedChanges`, discards on failure/cancel, buffers
  per-`DbContext`, and gives each handler a fresh DI scope. The transaction caveat is
  documented honestly. The reliability gap (§1.7) is a missing durability layer on an
  otherwise excellent mechanism.
- **`DomainEventPublisher`** compiles and caches dispatch delegates, caps reentrant depth
  with an `AsyncLocal` counter, and isolates one unconstructable handler from its
  siblings.
- **`AuditableEntityInterceptor`** reads one timestamp per save, resolves the actor
  through an injected `ICurrentActor` seam, and distinguishes `Anonymous` from `System`.
- **`Shared.Contracts`** is the one project that models the dependency rule properly: zero
  packages, correct CQRS variance. It is the template the rest should follow.
- **The exception-strategy registry** is a clean Open/Closed design — adding an exception
  type is a new 22-line class, memoized derived→base at O(1).
- **`TimeProvider` is injected everywhere a clock is needed** — the seam is real and
  testable; `IDomainEvent` (§1.8) is the sole place it was skipped.
