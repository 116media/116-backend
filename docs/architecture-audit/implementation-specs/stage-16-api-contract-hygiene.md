# Stage 16 — API contract & authorization hygiene

The HTTP-surface debt no earlier stage owned.

**Closed:** **[06 §10]** / **[08 §14]** (routes), **[06 §11]** (rate limits), **[06 §13]**
(validators), **[08 §5]** and **[08 §11]** (problem details), **[08 §13]** (v2), **[08 §19]**
(DELETE semantics), **[07 S3]** (permissions decision), **[01 §1.4]** (dispatcher),
**[01 §1.6]** (Mapster).

**Partly closed:** **[06 §3]** / **[04 §16]** — the four bounded reference lists are capped;
three derived reads remain, see the PR note at the end.

**Still open:** **[06 §12]** — per-resource authorization
tiers; 16.5 needs an owner decision. **[06 §8]** — admin/public dedup, opportunistic only and
nothing was touched.

(`pageSize` clamping `[06 §2]` / `[08 §6]` shipped in Stage 1 — `PaginatedRequest.MaxPageSize
= 100` with constructor clamp, verified.)

Re-verified against the tree after Stage 15 landed (develop `be2eb195e`):

- `Dispatcher.Send` caches the handler **type** but calls
  `handlerType.GetMethod("Handle", …)` and `handleMethod.Invoke(handler, parameters)` **per
  request** — `Shared/Application/Services/Dispatcher.cs:35,43,68,76` `[01 §1.4]`. Still true.
- `ValidationDecorator` throws `ValidationException(failures)` carrying raw
  `ValidationFailure` objects, and `ValidationExceptionHandler:26` assigns them straight to
  `problemDetails.Extensions["errors"]`, which `ExceptionHandler` writes with
  `WriteAsJsonAsync`. The only serializer configuration is a `JsonStringEnumConverter`
  (`Program.cs:149`), so nothing reshapes or filters the value `[08 §5]`. Still true, and
  wider than the audit recorded — the serialized shape, measured against FluentValidation
  12.0.0 with a real validator, is:

  ```json
  "errors": [{
      "PropertyName": "Password",
      "ErrorMessage": "The length of 'Password' must be at least 8 characters. You entered 7 characters.",
      "AttemptedValue": "hunter2",
      "CustomState": null,
      "Severity": 0,
      "ErrorCode": "MinimumLengthValidator",
      "FormattedMessagePlaceholderValues": {
          "MinLength": 8, "MaxLength": -1, "TotalLength": 7,
          "PropertyName": "Password", "PropertyValue": "hunter2", "PropertyPath": "Password"
      }
  }]
  ```

  Three separate facts follow, and 16.7 has to close all three:

  1. `FormattedMessagePlaceholderValues.PropertyValue` is a verbatim **second copy** of
     `AttemptedValue`. Stripping `AttemptedValue` alone leaves the password in the response.
  2. `detail` is a **third copy** of the same information: the handler passes
     `exception.Message`, and FluentValidation builds it by concatenating every failure
     message, newlines and a trailing `Severity: Error` included. Even where the message
     omits the value it leaks around it — `You entered 7 characters` is a password-length
     disclosure.
  3. `CustomState`, `Severity` and `ErrorCode` are not disclosures. `WithState`,
     `WithSeverity` and `WithErrorCode` are called **0 times** in `src/`, so the first two are
     permanently `null` and `0`, and `ErrorCode` is whatever FluentValidation names its own
     validator class. They are a contract defect, not a security one: three structurally dead
     fields a client can still bind to, keyed in PascalCase inside a camelCase document.
- Interaction DELETEs return `Results.Ok(new …Response(IsSuccess: true))` `[08 §19]`. Still
  true; 28 `MapDelete` endpoints.
- **Resolved before this stage.** The two competing `TypeAdapterConfig` singletons are gone.
  All three modules now call `services.AddModuleMappings(new MappingRegistration())`, which
  registers each `IRegister` and builds one config from `GetServices<IRegister>()` behind
  `TryAddSingleton` (`Shared/Infrastructure/ModuleMappings.cs:22-32`). 16.9b has nothing left
  to do `[01 §1.6]`.
- **Resolved before this stage.** No `MapToApiVersion(2)` exists anywhere in `src/`. The
  phantom version is already gone, so the v2 half of 16.8 and decision D2 are moot `[08 §13]`.
- **Not a defect.** Route parameters bound as `string` and parsed with an unguarded
  `Guid.Parse` do **not** produce a 500: `FormatExceptionStrategy` catches `FormatException`
  and returns 400 with a localized "invalid identifier" message. Any validator work below is
  about payload rules, not id parsing.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Unbounded lists | paginate all 22, or cap the small ones | **Paginate the growing ones, cap the bounded-by-nature ones.** Tags/categories/content-types are small reference lists — a server-side `Take(500)` + doc note suffices. User-generated collections (comments already paginated; playlists, shares) get real pagination. `[04 §16]`'s unbounded reads join whichever bucket their table's growth implies. |
| D2 | The v2 declaration | build v2, or delete it | **Moot — already deleted.** The decision was to delete, and no `MapToApiVersion(2)` remains anywhere in `src/`. Kept for the record; re-introduce a version when a real v2 consumer exists `[08 §13]`. |
| D3 | S3 (28 permissions, checked nowhere) | enforce permissions per endpoint, or cut to roles | **Cut to roles, keep the tables.** The three-role model is what every endpoint actually checks (`UserRolePolicies.*`); enforcing 28 permissions retroactively would need a product decision per endpoint that nobody has asked for. The seeded data stays (it is correct) and the decision is recorded so the model is deliberate, not dead `[07 S3]`. **The JWT keeps the permissions claim — user decision, overriding this row's original "stop carrying it".** It was removed and then restored: permission-based guards are planned, and the claim is the substrate they will read, so dropping it would have to be undone. Token size is the accepted cost. |
| D4 | Envelope removal | sweep all `{isSuccess}` responses, or DELETEs only | **DELETEs → 204 now; the rest stays.** The full envelope sweep breaks every consumer for cosmetic gain; DELETE-returns-body is the semantically wrong case, and the 28 DELETE endpoints are a bounded, coordinated break `[08 §19]`. |
| D5 | Dispatcher | compiled delegates, or typed wrapper resolution | **Typed wrapper.** A cached `RequestHandlerWrapper<TResponse>` resolved per request type dispatches through a virtual call — no `MethodInfo`, no `Invoke`, no boxing, same DI semantics. |
| D6 | Mapster | merge the two configs, or scan both into one | **Moot — already done.** `AddModuleMappings` registers each module's `IRegister` and builds a single `TypeAdapterConfig` from `GetServices<IRegister>()` under `TryAddSingleton`, so all three modules' mappings coexist. No competing singletons remain `[01 §1.6]`. |

---

## Checklist

- [ ] 16.1 — Census commit: unpaginated lists, misplaced routes, envelope DELETEs, validator-less commands
- [~] 16.2 — Reference lists capped; three derived reads still open `[06 §3 / 04 §16]`
- [x] 16.3 — The 7 misplaced public endpoints moved under `/public` `[06 §10 / 08 §14]`
- [x] 16.4 — Write endpoints off `ContentBrowsing` onto write policies `[06 §11]`
- [ ] 16.5 — Per-resource authorization unified (one admin tier per lifecycle) `[06 §12]`; S3 decision executed
- [x] 16.6 — Validators for the 6 free-text commands; `ProducesValidationProblem` on mutations `[06 §13]`
- [x] 16.7 — RFC 7807 completion: validation errors reshaped (no raw `ValidationFailure`, no echoed input in `detail`), `type` URIs `[08 §5 / 08 §11]`
- [x] 16.8 — DELETEs → 204 (**22** of 28; 6 keep 200 + payload) `[08 §19]` — the v2 half was already done
- [x] 16.9 — Dispatcher wrapper `[01 §1.4]` — the single-Mapster-config half was already done
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

**Landed, with two deviations from the sketch above.**

`GetRequiredService` is not used. The dispatcher unit tests mock `IServiceProvider` with Moq,
which does not implement `ISupportRequiredService`, so `GetRequiredService` would surface
"No service for type … has been registered" in place of the asserted
`No handler registered for {requestType.Name}`. The wrapper calls `GetService<T>()` and keeps
the explicit null check, so the thrown message is unchanged from the reflection version.

The cache is **not** keyed by request type alone. `IRequest<out TResponse> : IRequest`, so every
request with a response also satisfies the void path, and one shared `ConcurrentDictionary<Type,
Type>` was a live collision: dispatching a request through `Send<TResponse>` first poisoned the
key, and the later `Send(IRequest)` resolved the *response* handler type. There are now two
caches — `(RequestType, ResponseType)` for the response path, `RequestType` for the void path.
`DispatcherTests.Send_WhenOneRequestTypeReachesBothDispatchPaths_ShouldResolveEachPathsOwnHandler`
is the regression test; it was run against the previous `Dispatcher` and fails there, so it
covers the defect rather than restating the new shape.

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

`ValidationDecorator` keeps throwing. `ValidationExceptionHandler` — the existing class, kept
under its existing name — stops assigning `exception.Errors` and projects the failures instead.
Both copies of the submitted value die with the projection, and `detail` stops being
`exception.Message`:

```csharp
/// <summary>
/// Renders FluentValidation failures as one entry per field, carrying messages only.
/// The submitted value is never echoed, in the errors or in the detail.
/// </summary>
public sealed class ValidationExceptionHandler : BaseExceptionStrategy<ValidationException>
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(ValidationException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();

        ProblemDetails problemDetails = CreateStandardProblemDetails(
            title: nameof(ValidationException),
            detail: msg.ValidationFailed(),
            statusCode: StatusCodes.Status400BadRequest,
            context: context
        );

        problemDetails.Extensions["errors"] = exception
            .Errors.GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => JsonNamingPolicy.CamelCase.ConvertName(group.Key),
                group => group.Select(failure => failure.ErrorMessage).ToArray()
            );

        return problemDetails;
    }
}
```

`SharedExceptionMessage.ValidationFailed()` is new and joins the three resx files beside
`InvalidIdentifier()`, which `FormatExceptionStrategy` already uses the same way. A fixed
localized sentence replaces the concatenated failure dump; the per-field detail is in
`errors`, where a client can actually use it.

**Test blast radius, measured — this is the real cost of 16.7 and it is not small.**
`ValidationExceptionHandlerTests` asserts the defect directly
(`Extensions["errors"].Should().BeEquivalentTo(failures)` at `:58`, and casts to
`IEnumerable<ValidationFailure>` at `:115`); both are rewritten to the projected dictionary
rather than kept. Bigger: **63 integration test files each declare their own private
`ValidationDetail(...)` helper**, every one of them
`new ValidationException(failures.Select(...)).Message` — the concatenated string — feeding
**118 `ShouldBeProblem` assertions**. They pass today only because they rebuild the exact
string the handler leaks. Changing `detail` breaks all 118 at once.

Sequence that keeps it mechanical:

1. Replace the 63 private helpers with one shared helper on the integration test base, and add
   a companion `ShouldHaveFieldErrors((property, message), …)` that asserts the `errors`
   dictionary instead of the detail string. The per-field expectations the 118 call sites
   already pass move across unchanged — they are the right assertion, they were simply being
   made against the wrong field.
2. Only then change the handler. The suite now asserts the fixed `detail` once and the field
   messages per case.

A regression test submits a failing password and asserts the response body contains neither
the submitted value nor `AttemptedValue`, `PropertyValue`, `CustomState`, `Severity`,
`ErrorCode` or `FormattedMessagePlaceholderValues` as keys.

**Landed.** The payload a failing request now returns is
`{"name":["Le nom du rôle est obligatoire."],"description":[…]}` — camelCase keys, messages
only, no submitted values anywhere. `detail` is the fixed localized
`SharedExceptionMessage.ValidationFailed()`.

The test consolidation went the way the sequence above describes, with one correction: rather
than a helper returning the new detail, the 63 private `ValidationDetail` helpers were deleted
outright and the 118 call sites moved to a new
`HttpResponseExtensions.ShouldBeValidationProblem(...)`, which asserts the status, the title,
the per-field camelCase entries **and** that none of `AttemptedValue`, `PropertyValue`,
`CustomState` or `FormattedMessagePlaceholderValues` appears anywhere in the raw body. The
leak assertion therefore runs on all 118 sites rather than in one regression test, so the
suite now fails if any future change reintroduces the raw failures. Coverage went up, not
down: the old assertion compared one concatenated string, the new one pins each field.

`type` is now set on every problem: `about:blank` from
`BaseExceptionStrategy.CreateStandardProblemDetails`, and `urn:116:problem:{code}` from all
three modules' `DomainRuleExceptionStrategy` (via `ProblemTypes.ForRule`). `ExceptionHandler`
writes with `contentType: "application/problem+json"`.

Problem completion (`[08 §11]`) rides the Stage 7 vocabulary: `type` becomes
`urn:116:problem:{RuleCode}` (or `about:blank` for non-rule problems) and `Content-Type`
asserts `application/problem+json`. `traceId` is **already stamped** for every strategy —
`BaseExceptionStrategy.CreateStandardProblemDetails:46` sets it from `context.TraceIdentifier`
alongside a `timestamp` — so that half is done; Stage 11's correlation id only has to replace
the source, not add the field. The `ShouldBeProblem<TException>` test helper grows the matching
assertions once, which re-verifies every endpoint suite.

### 16.6 The validator gap, measured

Re-run across **all** modules after Stage 15: **205 commands, 158 with a validator, 47
without** (44 Content, 2 Identity, 1 Mailer). The audit's "44" was close but Content-only.

Thirty-five of the 47 take nothing but a `Guid` or an enum. Routing and the type system
already reject anything else, and a malformed id returns 400 through
`FormatExceptionStrategy`, so a validator there is ceremony. **They are deliberately left
alone.**

The remaining **six accept unbounded free text that reaches the database with no length or
format check**, and those are the whole of 16.6:

| Command | Unchecked input |
| --- | --- |
| `AdminUpsertAlbumStreamingLinkCommand` | `string Url` |
| `AdminUpsertSingleStreamingLinkCommand` | `string Url` |
| `PublicVoteOnLyricsRevisionCommand` | `string? Comment` |
| `PublicVoteOnTranslationRevisionCommand` | `string? Comment` |
| `AdminRejectPaymentCommand` | `string? Notes` |
| `PublicRecordShortVideoViewCommand` | `string? DeviceId`, `string? IpAddress`, `string? UserAgent` |

Both `Url` values want a well-formed absolute URL; the rest want a maximum length matching the
column. `ProducesValidationProblem` still goes on every mutation regardless.

**Landed. One of the six was a live 500, not just an unchecked field.**

`PublicRecordShortVideoView` takes `DeviceId` from the caller-supplied `X-Device-Id` header and
the handler builds `dedup_key` as `$"device:{DeviceId}"`. That column is
`HasMaxLength(100)` (`ShortVideoViewEventConfiguration:22`), so any device id over 93
characters overflowed it. The endpoint is anonymous — `Client.ClearAuthentication()` in its own
tests — so any caller could trigger it. Confirmed by sending a 200-character header against the
unvalidated build: **HTTP 500**. `RecordShortVideoView_WithOversizedDeviceIdHeader_ReturnsBadRequestAndRecordsNothing`
sends that same 200-character header and asserts 400 plus no persisted row.

Caps chosen against the columns rather than invented: `DeviceId` 64 (clear of the 93-char
overflow point), `IpAddress` 64 and `UserAgent` 500 (their own columns), vote comments and
payment notes 1000 (`Notes` is `HasMaxLength(1000)`), streaming URLs 500
(`MaxStreamingLinkUrlLength`). `AdminRejectPaymentCommand.OrderId` is a `string` the handler
feeds to `Guid.Parse`, so it also gained `IsValidGuid` — a field-level 400 rather than the
generic invalid-identifier problem.

Every rule is a shared `IRuleBuilder` extension and every validator is a thin wrapper, per the
two-layer standard in `docs/EDITORIAL_VALIDATION_FIX.md`: `ValidStreamingLinkUrl` (two call
sites, requires an absolute `http`/`https` URI so `javascript:` and scheme-less values are
rejected), `ValidLyricsRevisionVoteComment`, `ValidTranslationVoteComment`, `ValidPaymentNotes`,
`ValidViewDeviceId`, `ValidViewIpAddress`, `ValidViewUserAgent`. No inline rule and no magic
number survives in a validator. Tests: 30 unit cases across six new
validator test classes, plus 10 integration cases proving each validator is actually wired
(status 400 and nothing persisted) rather than merely registered.

## Part C — Surface corrections

**Routes `[06 §10]`** — one helper per scope replaces the per-use-case group declaration:

```csharp
public static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder app, string resource) =>
    app.MapApiVersionGroup(1)
        .MapGroup($"{ContentConstants.Public}/{resource}")
        .WithTags($"{ContentConstants.Public}::{resource}");
```

The 7 endpoints outside `/public` move under it; old paths return
`Results.Redirect(permanent: true)` stubs for one release, then die. Re-measured after Stage 15
and still exactly 7, all in Editorial: propose/vote on lyrics revisions, propose/vote on
translation revisions, get translation revisions, submit lyrics, request artist claim.

**DELETE semantics `[08 §19]`** — the interaction unlike/unbookmark endpoints:

```csharp
// Before
return Results.Ok(new PublicUnlikeArticleResponse(IsSuccess: result.IsSuccess));

// After — the status line is the contract; the envelope goes.
return Results.NoContent();
```

`.Produces(StatusCodes.Status204NoContent)` replaces the response-type metadata; the endpoint
tests change their assertion from body to status.

**Landed on 22 of the 28, not all of them — the census assumed every DELETE was an envelope and
six are not.** Measured response shapes:

| Endpoint | Returns | Outcome |
| --- | --- | --- |
| `AdminRemoveCategoryPricing` | `IReadOnlyList<CategoryPricingDto> Pricing` | **kept 200** |
| `AdminRemovePackageSlot` | `PackageDto Package` | **kept 200** |
| `AdminRemovePermissionFromRole` | `RoleWithPermissionsDto Role` | **kept 200** |
| `AdminSoftDeletePermission` | `PermissionDto Permission` | **kept 200** |
| `AdminSoftDeleteRole` | `RoleDto Role` | **kept 200** |
| `AdminRemoveRoleFromUser` | `IReadOnlyCollection<RoleDto> Roles` | **kept 200** |
| the other 22 | `bool IsSuccess` and nothing else | → **204** |

D4's reasoning — "DELETE-returns-body is the semantically wrong case" — holds for the
`{isSuccess: true}` envelope, which carries information the status line already carries. It does
not hold for a response returning the resulting state: the two soft deletes return the updated
resource, and the four removals return the collection the caller would otherwise have to re-GET.
Emptying those to 204 would remove information and force a second round trip, so they stay,
deliberately, and the decision is recorded here rather than left as an inconsistency.

For the 22, the `XResponse` record is deleted outright (it had no field left), the handler result
is no longer captured, and `.Produces<XResponse>(Status200OK)` becomes
`.Produces(Status204NoContent)`. Two endpoints (`AdminHardDeleteRole`,
`AdminHardDeletePermission`) declared `.Produces<T>()` with no status code and needed a separate
pass. Test fallout: 21 integration files dropped their body assertions and 26 status assertions
moved from `OK` to `NoContent`.

**Rate limits `[06 §11]`** — interaction/mutation endpoints move from
`RateLimitPolicies.ContentBrowsing` (a fixed-window read policy) to the appropriate write
policies; the census lists each endpoint's target policy in the PR description. `ContentBrowsing`
is referenced 227 times across 295 endpoint files, so the sweep is mechanical but wide.

**Unbounded lists `[06 §3]`** — re-measured after Stage 15: **22** queries return a collection
with no `Page`/`PageSize` (19 Content, 3 Identity), matching the original count. Per D1 the
reference lists (tags, categories, content types, pricing tiers, promotion levels) take a
server-side cap; the growing ones (playlists, promotion feeds, popular/promoted lists, translation
revisions, session export) take real pagination.

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
2. `grep -rn "MapToApiVersion(2" src/` → empty. **Already empty before the stage starts.**
3. `grep -rn "AddSingleton" src/Modules/*/*/[A-Z]*Module.cs | grep TypeAdapterConfig` → empty.
   **Already empty**; the modules call `AddModuleMappings` instead.
4. `grep -rn "GetMethod(\"Handle\"" src/Shared` → empty. Currently 2 hits in
   `Shared/Application/Services/Dispatcher.cs`; this is the real check for 16.9.
5. Route census: every mapped route matches `/api/v{version}/{scope}/…`. Currently 7 fail.
6. `MapDelete` endpoints returning a body → 0. Currently 28.
7. Commands accepting free text without a validator → 0. Currently 6 (16.6's list).

---

**PR (as landed):** `fix(api): route and rate-limit scoping, 204 deletes and RFC 7807 completion`

The original title read `pagination caps, route/rate-limit/authz hygiene and RFC 7807
completion`. Two of those did not land and the title is corrected rather than left aspirational:

- **Pagination caps (16.2) — partly done.** D1's blanket cap is unsafe as one rule, so the item
  was split rather than skipped. **Capped:** the four bounded reference lists — content types,
  pricing tiers, promotion levels (`.Take(MaxReferenceListSize)` = 500) and tags, where the
  builder's optional caller `limit` is now clamped to the same ceiling and applied even when the
  caller sends none. `CategoryRepository.GetAllAsync` was already paginated and is untouched.
  **Still open:** three derived reads where a cap would corrupt rather than truncate —
  `GetAllRatingsForVideoAsync` feeds the average-rating computation in `VideoEngagementHandler`,
  so a `Take(n)` there averages a subset and persists a wrong number; `GetAllByLyricsIdAsync`
  and `GetAllByTranslationIdAsync` back "all translations/revisions for this item" reads. Those
  need real pagination or a deliberate decision, not a cap.
- **Authorization unification (16.5) — not done**, and it needs an owner decision, not a sweep.
  Measured split: 88 Content endpoints on `RequireAdminOrSuperAdmin`, 54 on `RequireSuperAdminOnly`,
  and the inconsistency is per resource — an Admin can upload an artist's avatar but not edit that
  artist, deactivate a category but not create or update one, edit a video but not publish it.
  Unifying moves production privilege up (locking admins out of work they do today) or down
  (widening access). The S3 half is decided and recorded in D3.
