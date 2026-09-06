# Stage 16 — API contract & authorization hygiene

Closes **[06 §3]**, **[06 §8]**, **[06 §10]** / **[08 §14]**, **[06 §11]**, **[06 §12]**,
**[06 §13]**, **[08 §5]**, **[08 §11]**, **[08 §13]**, **[08 §19]**, **[07 S3]**, **[01 §1.4]**,
**[01 §1.6]**, **[04 §16]**. The HTTP-surface debt no earlier stage owned.

(`pageSize` clamping `[06 §2]` / `[08 §6]` shipped in Stage 1 — `PaginatedRequest.MaxPageSize
= 100` with constructor clamp, verified.)

Verified in the current tree:

- `Dispatcher.SendAsync` caches the handler **type** but calls
  `handlerType.GetMethod("Handle", …)` and `handleMethod.Invoke(handler, parameters)` **per
  request** `[01 §1.4]`.
- `IdentityModule.cs:135` and `ContentModule.cs:136` both call
  `services.AddSingleton(MappingRegistration.CreateConfiguration())` — two `TypeAdapterConfig`
  singletons in one container; `GetRequiredService<TypeAdapterConfig>` resolves the last
  registration, so the other module's mappings are silently discarded `[01 §1.6]`.
- `ValidationDecorator` throws `ValidationException(failures)` carrying raw
  `ValidationFailure` objects — which echo `AttemptedValue` (the submitted password, for a
  password rule) back to the client `[08 §5]`.
- Interaction DELETEs return `Results.Ok(new …Response(IsSuccess: true))` `[08 §19]`.

> Draft — finalized against the tree Stage 15 lands on. Every census below re-runs at
> finalization; Stages 9–15 move endpoints.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Unbounded lists | paginate all 22, or cap the small ones | **Paginate the growing ones, cap the bounded-by-nature ones.** Tags/categories/content-types are small reference lists — a server-side `Take(500)` + doc note suffices. User-generated collections (comments already paginated; playlists, shares) get real pagination. `[04 §16]`'s unbounded reads join whichever bucket their table's growth implies. |
| D2 | The v2 declaration | build v2, or delete it | **Delete.** One `MapToApiVersion(2)` exists in the codebase and nothing exercises it; carrying a phantom version costs every endpoint's `.WithApiVersionSet` ceremony. Re-introduce when a real v2 consumer exists `[08 §13]`. |
| D3 | S3 (28 permissions, checked nowhere) | enforce permissions per endpoint, or cut to roles | **Cut to roles, keep the tables.** The three-role model is what every endpoint actually checks (`UserRolePolicies.*`); enforcing 28 permissions retroactively would need a product decision per endpoint that nobody has asked for. The seeded data stays (it is correct), the JWT **stops carrying the permissions claim** (it bloats every token for zero checks), and the decision is recorded so the model is deliberate, not dead `[07 S3]`. |
| D4 | Envelope removal | sweep all `{isSuccess}` responses, or DELETEs only | **DELETEs → 204 now; the rest stays.** The full envelope sweep breaks every consumer for cosmetic gain; DELETE-returns-body is the semantically wrong case, and the 28 DELETE endpoints are a bounded, coordinated break `[08 §19]`. |
| D5 | Dispatcher | compiled delegates, or typed wrapper resolution | **Typed wrapper.** A cached `RequestHandlerWrapper<TResponse>` resolved per request type dispatches through a virtual call — no `MethodInfo`, no `Invoke`, no boxing, same DI semantics. |
| D6 | Mapster | merge the two configs, or scan both into one | **One config, both registrations scan into it.** `TypeAdapterConfig` is registered once in the host; each module contributes via `MappingRegistration.Apply(config)`. Both modules' mappings finally coexist `[01 §1.6]`. |

---

## Checklist

- [ ] 16.1 — Census commit: unpaginated lists, misplaced routes, envelope DELETEs, validator-less commands
- [ ] 16.2 — Unbounded lists paged/capped `[06 §3 / 04 §16]`
- [ ] 16.3 — The 7 misplaced public endpoints moved under `/public`; scope route-group helpers `[06 §10 / 08 §14]`
- [ ] 16.4 — Write endpoints off `ContentBrowsing` onto write policies `[06 §11]`
- [ ] 16.5 — Per-resource authorization unified (one admin tier per lifecycle) `[06 §12]`; S3 decision executed
- [ ] 16.6 — Validators for the validator-less commands; `ProducesValidationProblem` on mutations `[06 §13]`
- [ ] 16.7 — RFC 7807 completion: validation errors without `AttemptedValue`, `type` URIs, `traceId` `[08 §5 / 08 §11]`
- [ ] 16.8 — DELETEs → 204; v2 declaration deleted `[08 §19 / 08 §13]`
- [ ] 16.9 — Dispatcher wrapper; single Mapster config `[01 §1.4 / 01 §1.6]`
- [ ] 16.10 — Verify (build 0/0, csharpier, unit, integration; censuses clean)

---

## Part A — The dispatcher and the mapper (do these first: invisible, everything else sits on them)

### 16.9a Dispatcher without per-request reflection

```csharp
namespace _116.Shared.Application.Services;

/// <summary>
/// Non-generic dispatch seam; one concrete wrapper per request type, cached forever.
/// </summary>
internal abstract class RequestHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    );
}

/// <summary>
/// Bridges the untyped dispatch path onto the typed handler with a virtual call instead of
/// <see cref="MethodInfo.Invoke" />.
/// </summary>
internal sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public override Task<TResponse> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    )
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return handler.Handle((TRequest)request, cancellationToken);
    }
}
```

```csharp
private static readonly ConcurrentDictionary<Type, object> WrapperCache = new();

public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
{
    var wrapper = (RequestHandlerWrapper<TResponse>)WrapperCache.GetOrAdd(
        request.GetType(),
        requestType =>
            Activator.CreateInstance(
                typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse))
            )!
    );

    return wrapper.Handle(request, serviceProvider, cancellationToken);
}
```

Reflection happens once per request *type* for the process lifetime; the per-request path is a
dictionary hit and a virtual call. The decorators (validation/logging) are unaffected — they
wrap `IRequestHandler<,>` via Scrutor as today.

### 16.9b One Mapster config

```csharp
// Program.cs — the only AddSingleton for TypeAdapterConfig in the codebase.
var mapsterConfig = new TypeAdapterConfig();
IdentityMappingRegistration.Apply(mapsterConfig);
ContentMappingRegistration.Apply(mapsterConfig);
builder.Services.AddSingleton(mapsterConfig);
builder.Services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<TypeAdapterConfig>()));
```

Each module's `MappingRegistration.CreateConfiguration()` becomes
`Apply(TypeAdapterConfig config)` mutating the shared instance; the two module-level
`AddSingleton`/`AddScoped<IMapper>` registrations are deleted. A startup
`mapsterConfig.Compile()` validates every mapping at boot instead of first use.

## Part B — Validation and problem details

`ValidationDecorator` keeps throwing, but the strategy that renders it maps to field errors
without echoing input:

```csharp
/// <summary>
/// Renders FluentValidation failures as an RFC 9457 problem with one entry per field —
/// property, message, rule code. The submitted value is deliberately never echoed.
/// </summary>
public class ValidationExceptionStrategy : BaseExceptionStrategy<ValidationException>
{
    /// <inheritdoc />
    protected override int StatusCode => StatusCodes.Status400BadRequest;

    /// <inheritdoc />
    protected override void Enrich(ProblemDetails problem, ValidationException exception)
    {
        problem.Extensions["errors"] = exception
            .Errors.GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => JsonNamingPolicy.CamelCase.ConvertName(group.Key),
                group => group.Select(failure => failure.ErrorMessage).ToArray()
            );
    }
}
```

Problem completion (`[08 §11]`) rides the Stage 7 vocabulary: `type` becomes
`urn:116:problem:{RuleCode}` (or `about:blank` for non-rule problems), `Content-Type` asserts
`application/problem+json`, and the correlation middleware from Stage 11 stamps
`problem.Extensions["traceId"]`. The `ShouldBeProblem<TException>` test helper grows the
matching assertions once, which re-verifies every endpoint suite.

The validator-less commands get validators in the house shape (rule extensions +
`i18n.*.Msg`); the census at finalization decides the exact list (the audit's 44 predates
Stages 6–9).

## Part C — Surface corrections

**Routes `[06 §10]`** — one helper per scope replaces the per-use-case group declaration:

```csharp
public static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder app, string resource) =>
    app.MapApiVersionGroup(1)
        .MapGroup($"{ContentConstants.Public}/{resource}")
        .WithTags($"{ContentConstants.Public}::{resource}");
```

The 7 endpoints outside `/public` move under it; old paths return
`Results.Redirect(permanent: true)` stubs for one release, then die.

**DELETE semantics `[08 §19]`** — the interaction unlike/unbookmark endpoints:

```csharp
// Before
return Results.Ok(new PublicUnlikeArticleResponse(IsSuccess: result.IsSuccess));

// After — the status line is the contract; the envelope goes.
return Results.NoContent();
```

`.Produces(StatusCodes.Status204NoContent)` replaces the response-type metadata; the endpoint
tests change their assertion from body to status.

**Rate limits `[06 §11]`** — interaction/mutation endpoints move from
`RateLimitPolicies.ContentBrowsing` (a fixed-window read policy) to the appropriate write
policies; the census lists each endpoint's target policy in the PR description.

**Authorization `[06 §12]` + S3** — per resource, one admin tier across its lifecycle (the
census tabulates current SuperAdmin/Admin splits and the chosen tier). The JWT permissions
claim is dropped from token generation; `VisitorPermissions` seeding stays; the S3 decision
lands as a doc note in `docs/architecture-audit/07-identity-and-security.md`'s status column.

**Admin/Public dedup `[06 §8]`** — opportunistic only: where 16.2–16.8 already touch a pair,
the shared handler core is extracted; no standalone sweep.

---

## Tests

- **Unit:** dispatcher wrapper (typed dispatch, unknown request throws the same
  `InvalidOperationException` message); `ValidationExceptionStrategy` shape (no
  `attemptedValue` anywhere in the payload); Mapster `Compile()` passes with both modules
  applied.
- **Integration:** capped list returns exactly the cap with more rows seeded; a moved route's
  old path 301s then (next release) 404s; DELETE returns 204 with no body; a validation
  failure payload contains field names and messages but not the submitted value (raw-JSON
  assertion, Stage 12 style); `traceId` present on every problem response.

---

## Rollout

Everything client-visible here is a breaking change with a small blast radius each — this is
the release-notes stage. One PR; the description enumerates: removed fields (none), moved
routes, DELETE status change, validation payload shape, dropped permissions claim (token size
shrinks; no consumer reads it — verified against dashboard/mobile before merge).

---

## Verification

1. Build/format/unit/integration green.
2. `grep -rn "MapToApiVersion(2" src/` → empty.
3. `grep -rn "AddSingleton" src/Modules/*/*/[A-Z]*Module.cs | grep TypeAdapterConfig` → empty.
4. `grep -rn "GetMethod(\"Handle\"" src/Shared` → empty.
5. Route census: every mapped route matches `/api/v{version}/{scope}/…`.

---

**PR:** `fix(api): pagination caps, route/rate-limit/authz hygiene and RFC 7807 completion`
