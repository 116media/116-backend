# Stage 1 — Critical Security Quick Wins

**Goal:** close three critical, isolated issues with no cross-module surgery — plaintext credentials in
logs, internal error details leaked to clients, and an unbounded page size. All three are small, low-risk,
and independently testable.

**Findings closed:** `[01 §1.2]` / `[08 §2]` (credential logging), `[08 §3]` (error leakage),
`[06 §2]` / `[08 §6]` (page-size DoS).

**Checklist**

- [x] 1.1 — `LoggingDecorator` no longer serializes request payloads
- [x] 1.2 — Localized `UnexpectedError`/`RequestCancelled` messages; `DefaultExceptionHandler` sanitized (env-gated)
- [x] 1.3 — `OperationCanceledExceptionHandler` (client-disconnect → 499, not 500)
- [x] 1.4 — `PaginatedRequest` clamps page size in its constructor
- [x] 1.5 — Verified (build 0/0, 7,706 unit tests green). **Ops still pending:** purge historical logs + rotate `JWT_SECRET` on deploy note

---

## 1.1 — Stop logging request payloads

**File:** `src/Shared/Shared/Application/Decorators/LoggingDecorator.cs`

The `{RequestData}` template has no `@` destructuring, so Serilog calls the record's `ToString()`, which
prints every property — including `Password`, `Code`, `RefreshToken`. Log only the type name; drop the
`[END]` line to `Debug`.

Replace the three log methods:

```csharp
    /// <summary>
    /// Logs the start of the request processing. The request payload is intentionally NOT logged:
    /// command records include credentials/OTP/tokens and Serilog would serialize them in full.
    /// </summary>
    private void LogStart()
    {
        logger.LogInformation("[START] Handling {Request}", typeof(TRequest).Name);
    }

    /// <summary>
    /// Logs a performance warning if the elapsed time exceeds the threshold.
    /// </summary>
    private void LogPerformanceWarning(TimeSpan elapsed)
    {
        if (elapsed.TotalSeconds > 3)
        {
            logger.LogWarning(
                "[PERFORMANCE] Request {Request} took {ElapsedSeconds:N2} seconds.",
                typeof(TRequest).Name,
                elapsed.TotalSeconds
            );
        }
    }

    /// <summary>
    /// Logs the end of request processing along with the response type.
    /// </summary>
    private void LogEnd()
    {
        logger.LogDebug("[END] Handled {Request} - Response={Response}", typeof(TRequest).Name, typeof(TResponse).Name);
    }
```

And update the call in `Handle` (remove the argument):

```csharp
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        LogStart();

        long startTimestamp = timeProvider.GetTimestamp();

        TResponse response = await handler.Handle(request, cancellationToken);

        LogPerformanceWarning(timeProvider.GetElapsedTime(startTimestamp));
        LogEnd();

        return response;
    }
```

> The `request` parameter is still passed to the inner handler; it is simply no longer written to a log
> sink.

---

## 1.2 — Localized messages + sanitized fallback

The fallback returns `exception.Message` verbatim at 500 — leaking Npgsql connection details, EF SQL, and
constraint names to anonymous callers. Fix it the way every other strategy works: **title =
`nameof(TException)`, detail = a localized string from `SharedExceptionMessage`** (resolved from
`context.RequestServices`), never a hardcoded literal. Two new messages are needed (`UnexpectedError`,
`RequestCancelled` — the latter used by 1.3).

### 1.2a — Add two accessors to the message facade

**File:** `src/Shared/Shared/Application/Exceptions/Messages/SharedExceptionMessage.cs` — add, matching the
existing `ResourceNotFound()` / `InvalidIdentifier()` shape:

```csharp
    /// <summary>
    /// Generic, localized message for an unhandled/unexpected server error. Used outside Development so
    /// the raw exception text (which may carry connection strings, SQL or schema detail) never reaches
    /// the client.
    /// </summary>
    public string UnexpectedError() => localizer["UnexpectedError"];

    /// <summary>
    /// Localized message for a request the client cancelled (disconnect).
    /// </summary>
    public string RequestCancelled() => localizer["RequestCancelled"];
```

### 1.2b — Add the resource keys (all three `.resx`)

Add to `SharedExceptionMessage.resx` **and** `SharedExceptionMessage.en.resx`:

```xml
  <data name="UnexpectedError" xml:space="preserve">
    <value>An unexpected error occurred. Please try again later.</value>
  </data>
  <data name="RequestCancelled" xml:space="preserve">
    <value>The request was cancelled.</value>
  </data>
```

Add to `SharedExceptionMessage.fr.resx`:

```xml
  <data name="UnexpectedError" xml:space="preserve">
    <value>Une erreur inattendue s'est produite. Veuillez réessayer plus tard.</value>
  </data>
  <data name="RequestCancelled" xml:space="preserve">
    <value>La requête a été annulée.</value>
  </data>
```

### 1.2c — Sanitize `DefaultExceptionHandler`

**File:** `...Strategies/DefaultExceptionHandler.cs` — resolve `IHostEnvironment` + `SharedExceptionMessage`
from `context.RequestServices` (the `NotFoundExceptionHandler` pattern); reveal the real message only in
Development; otherwise a stable non-leaking title (`nameof(InternalServerException)`, which already exists
in `Shared/Application/Exceptions/`) and the localized detail. The `traceId`/`timestamp` extensions mirror
`BaseExceptionStrategy.CreateStandardProblemDetails`. Full file:

```csharp
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Default strategy for unregistered exception types (the base <see cref="Exception"/> fallback).
/// Outside Development the exception message is withheld — unmapped exceptions (Npgsql, EF, SDK errors)
/// carry connection strings, SQL and schema detail that must never reach a client — and the detail is
/// taken from the localized <see cref="SharedExceptionMessage"/>, matching every other strategy.
/// </summary>
public sealed class DefaultExceptionHandler : IExceptionStrategy
{
    /// <inheritdoc />
    public Type ExceptionType => typeof(Exception);

    /// <inheritdoc />
    public ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        IHostEnvironment environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
        bool isDevelopment = environment.IsDevelopment();

        string detail = isDevelopment
            ? exception.Message
            : context.RequestServices.GetRequiredService<SharedExceptionMessage>().UnexpectedError();

        return new ProblemDetails
        {
            Title = isDevelopment ? exception.GetType().Name : nameof(InternalServerException),
            Detail = detail,
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path,
            Extensions = { ["traceId"] = context.TraceIdentifier, ["timestamp"] = DateTime.UtcNow },
        };
    }
}
```

> If `InternalServerExceptionHandler` also echoes `exception.Message` at 500 (`grep -n "exception.Message"`
> in that file), apply the same env-gate + `msg.UnexpectedError()` detail there.

---

## 1.3 — Add an `OperationCanceledException` strategy

**New file:** `src/Shared/Shared/Application/Exceptions/Handlers/Strategies/OperationCanceledExceptionHandler.cs`

Client disconnects currently fall through to the default handler → logged at Error and answered 500,
poisoning error-rate alerting. Map them to 499 (client closed request). It is auto-discovered by
`RegisterExceptionStrategies`, so no registration edit is needed.

```csharp
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Strategy for client-cancelled requests. A disconnect is not a server error, so it is mapped to
/// 499 (client closed request) rather than a logged 500. Title follows the <c>nameof(TException)</c>
/// convention; the detail comes from the localized <see cref="SharedExceptionMessage"/>.
/// </summary>
public sealed class OperationCanceledExceptionHandler : BaseExceptionStrategy<OperationCanceledException>
{
    private const int StatusClientClosedRequest = 499;

    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(OperationCanceledException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();

        return CreateStandardProblemDetails(
            title: nameof(OperationCanceledException),
            detail: msg.RequestCancelled(),
            statusCode: StatusClientClosedRequest,
            context: context
        );
    }
}
```

> This mirrors `NotFoundExceptionHandler` exactly (resolve `SharedExceptionMessage`, `nameof` title,
> `CreateStandardProblemDetails` — which already adds `traceId`/`timestamp`). 499 has no `StatusCodes`
> member, so the named `const` is the correct way to avoid an inline magic number. If `ExceptionHandler`
> maps log level by type, also map `OperationCanceledException` → `Debug` there.

---

## 1.4 — Clamp the page size

**File:** `src/Shared/Shared/Application/Pagination/PaginatedRequest.cs`

The `[Range]` attributes never execute (the record is hand-constructed, never model-bound), so
`?pageSize=1000000` materializes a whole table. Enforce the bound in the constructor. Positional call
sites (`new PaginatedRequest(pageIndex, pageSize)`) keep working.

Full file:

```csharp
namespace _116.Shared.Application.Pagination;

/// <summary>
/// A pagination request whose bounds are enforced at construction. `PageIndex` is floored at 0 and
/// `PageSize` is clamped to [1, <see cref="MaxPageSize"/>], so no caller (or query string) can request
/// an unbounded page. The previous DataAnnotations `[Range]` attributes were dead code — minimal APIs
/// never model-bound this record.
/// </summary>
public record PaginatedRequest
{
    /// <summary>
    /// The maximum number of items a single page may return.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// The zero-based index of the page to retrieve (floored at 0).
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// The number of items per page (clamped to [1, <see cref="MaxPageSize"/>]).
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Creates a bounded pagination request.
    /// </summary>
    /// <param name="pageIndex">Requested page index; values below 0 become 0.</param>
    /// <param name="pageSize">Requested page size; clamped to [1, <see cref="MaxPageSize"/>].</param>
    public PaginatedRequest(int pageIndex = 0, int pageSize = 10)
    {
        PageIndex = Math.Max(0, pageIndex);
        PageSize = Math.Clamp(pageSize, 1, MaxPageSize);
    }
}
```

Then fold the one hand-rolled clamp into the shared constant:

- In `PublicGetShortsFeedEndpointV1.cs`, replace the local `Math.Clamp(pageSize, 1, 20)` /
  `MaxPageSize = 20` with a call that keeps its tighter feed cap explicit, e.g.
  `int safePageSize = Math.Min(pageSize, 20);` before constructing the request (the record still
  guarantees the ≤100 ceiling and ≥1 floor).

---

## 1.5 — Verify

```bash
# 1.1 — no request payload is logged anymore
grep -n "RequestData" src/Shared/Shared/Application/Decorators/LoggingDecorator.cs   # → no matches

# 1.4 — the record no longer relies on the dead [Range] attributes, and nothing deconstructs it
grep -rn "var (.*) = .*PaginatedRequest" src   # → no positional deconstruction to break
grep -rn "new PaginatedRequest(" src | wc -l   # call sites still compile (positional ctor)

dotnet build 116_backend.sln
dotnet test tests/Unit
```

Add/adjust unit tests:

- `PaginatedRequest(0, 1000000).PageSize == 100`; `PaginatedRequest(-5, 10).PageIndex == 0`;
  `PaginatedRequest(0, 0).PageSize == 1`.
- An exception-handler test asserting the default handler returns a generic detail + `traceId` when the
  environment is Production, and the real message in Development.

**Secret rotation (operational, outside code):** because credentials transited the logs before 1.1, treat
historical Console/Seq data as compromised — purge it and rotate `JWT_SECRET` (and force a global
sign-out). Note this in the PR description so it is actioned on deploy.

---

## PR

**Title:** `fix(security): stop logging credentials, sanitize errors, clamp page size`

Suggested body checklist:
- stop `LoggingDecorator` serializing command payloads (credentials/OTP/tokens)
- withhold unhandled-exception details outside Development; add 499 for client cancellation
- enforce the `PaginatedRequest` page-size clamp in the constructor
- **ops:** purge historical Seq/console logs and rotate `JWT_SECRET` on deploy

When this PR is merged, tell me and I'll finalize the Stage 2 spec against the updated tree.
