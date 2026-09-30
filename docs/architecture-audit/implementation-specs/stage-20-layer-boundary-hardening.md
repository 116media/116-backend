# Stage 20 — Layer boundary hardening

Closes **[15 §15.1]** and **[15 §15.2]** — the two dependency-rule violations found by the
post-Stage-9 verification that docs 01–14 did not cover.

Two files still carry the driver, plus their tests. No behaviour change except where
Stage 11 already intends one.

> **Re-measured against the tree on 2026-09-30, after the project restructure.** Three of the six
> items are already closed and the paths in this document were all rewritten: every module is now
> `src/modules/<M>/<M>/src/<M>.{Domain,Application,Infrastructure}`, the shared kernel is
> `src/shared/src/BuildingBlocks.*`, and the suites are per module rather than `tests/Unit` and
> `tests/Integration`. What is left is two files and their tests.

**Numbered 20, but it did not run last.** §15.2 had to land before the restructure split the
shared kernel, and it did. §15.1 still belongs with Stage 11, which rewrites the same method.
The stage number records where the finding entered the audit, not the execution slot.

> Draft — 15.1 is finalized against whatever Stage 11 D3 lands.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Where the SQLSTATE knowledge lives | keep in the handler, or an Infrastructure-owned detector | **Detector interface in Application, Npgsql implementation in Infrastructure.** The nine connectivity SQLSTATEs are driver knowledge; the handler's question is "was the store unreachable?". This also makes them testable — today no test asserts a single one of them. |
| D2 | One detector or two | one `IDatabaseFaultDetector`, or split by concern | **Two.** Unique-constraint detection lives in the shared kernel (`BuildingBlocks`, since every module reaches `DbUpdateExceptionStrategy`); transient-fault detection lives in `Identity` (one consumer, and Stage 11 may delete the fallback entirely). Merging them puts an Identity concern in the shared kernel. |
| D3 | Newsletter token encoding | keep `WebEncoders`, add a Mailer helper, or use `System.Buffers.Text` | **`Base64Url.EncodeToString`.** Ships in the `net9.0` reference assemblies, identical alphabet and padding, so existing tokens stay valid. A hand-rolled helper would be three lines of transform to maintain for no gain. |
| D4 | Enforcement | trust review, or an architecture rule | **Architecture rule.** The harness exists in `tests/Architecture.Tests` and already carries the Domain rule. This stage adds the one remaining rule, no `Npgsql` in `*.Application`, and it can only be added once the two files below are fixed, since it ships with an empty allowlist. |

---

## Checklist

- [x] 20.1 — `Mailer.Domain` off `Microsoft.AspNetCore.WebUtilities` — **done.**
      `NewsletterSubscriberEntity.GenerateToken` uses .NET 9's `System.Buffers.Text.Base64Url`
      (`Entities/NewsletterSubscriberEntity.cs:85`); both emit unpadded RFC 4648, so existing
      tokens stay valid
- [x] 20.2 — `IUniqueConstraintDetector` + Npgsql implementation; `DbUpdateExceptionStrategy` off
      `Npgsql` — **done.** Contract in `BuildingBlocks.Application/Persistence/`, implementation in
      `BuildingBlocks.Infrastructure/Persistence/`, registered with `TryAddSingleton` in `BaseModule`
      beside `TimeProvider`, resolved from `context.RequestServices` like `SharedExceptionMessage`
- [x] 20.3 — `ITransientFaultDetector` + Npgsql implementation; `AccountStatusRequirementHandler`
      off `Npgsql` — **done.** The handler already failed closed, so this was a pure extraction apart
      from the cancellation change below. Registered in `IdentityModule`; the handler takes the
      detector as a third constructor parameter
- [x] 20.4 — Unit tests for both detectors — **done.** `PostgresTransientFaultDetectorTests`
      (9 SQLSTATEs, 3 constraint violations, timeout, 2 cancellations, 1 unrelated failure) and
      `PostgresUniqueConstraintDetectorTests` (`23505`, 3 other states, no inner exception,
      unrelated failure)
- [x] 20.5 — Architecture rules: no `Npgsql`/`Microsoft.AspNetCore` in `*.Domain` — **done.**
      `LayerDependencyTests.DomainDependsOnNoPersistenceOrWebFramework` covers it and
      `KnownViolations.txt` is empty. `ApplicationNamesNoDatabaseDriver` now ships too, verified by
      reintroducing `Npgsql` into `Identity.Application` and watching it fail
- [x] 20.6 — Verified: build 0/0 with no warnings, csharpier clean over 4,180 files, and every
      suite green (Content 3741 + 1525, Identity 2912 + 394, Mailer 333 + 47, Storage 354 + 18,
      Shared 1125 + 60, Architecture 15, EndToEnd 190)

---

## What the tree shows today

`*.Domain` is clean in all four modules: no `Npgsql`, no `Microsoft.AspNetCore`, no EF Core.
Two files still name the driver, and both reach it transitively rather than by declaring it:

| File | Layer | What it reads |
| --- | --- | --- |
| `AccountStatusRequirementHandler.cs:138` | `Identity.Application` | nine connection-class SQLSTATEs |
| `DbUpdateExceptionStrategy.cs:31` | `BuildingBlocks.Presentation` | `PostgresException` with SQLSTATE 23505 |

Neither project declares `Npgsql`. It arrives through `BuildingBlocks.Infrastructure`, which
declares `Npgsql.EntityFrameworkCore.PostgreSQL`. That matters for enforcement: a package-level
rule cannot catch this, so the rule has to be type-level, which is what `LayerDependencyTests`
already does for `*.Domain`.

## 20.1 — `Mailer.Domain` off the web stack

**Before** — `src/modules/Mailer/Mailer/src/Mailer.Domain/Entities/NewsletterSubscriberEntity.cs`:

```csharp
using Microsoft.AspNetCore.WebUtilities;

/// <summary>
/// Generates a URL-safe confirmation token.
/// </summary>
private static string GenerateToken()
{
    return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(MailerConstants.NewsletterTokenBytes));
}
```

**After:**

```csharp
using System.Buffers.Text;

/// <summary>
/// Generates a URL-safe confirmation token.
/// </summary>
private static string GenerateToken()
{
    return Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(MailerConstants.NewsletterTokenBytes));
}
```

Both produce unpadded RFC 4648 §5 output over the same alphabet, so tokens already issued to
subscribers still validate. With this, all four domain layers import nothing outside `System.*`,
`_116.*.Domain` and `_116.BuildingBlocks.Constants`.

---

## 20.2 — Unique-constraint detection behind an interface

The strategy currently reaches for the driver's error code table:

```csharp
// src/shared/src/BuildingBlocks.Presentation/Exceptions/Handlers/Strategies/DbUpdateExceptionStrategy.cs — before
using Npgsql;

private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;

bool isUniqueViolation = exception.InnerException is PostgresException { SqlState: UniqueViolation };
```

**The contract**, in `src/shared/src/BuildingBlocks.Application/Persistence/`, beside `IUnitOfWork`:

```csharp
namespace _116.BuildingBlocks.Application.Persistence;

/// <summary>
/// Classifies a persistence failure without exposing the database provider to callers.
/// </summary>
public interface IUniqueConstraintDetector
{
    /// <summary>
    /// Reports whether the supplied failure is a unique-constraint violation — a lost
    /// check-then-act race rather than a defect.
    /// </summary>
    /// <param name="exception">The failure raised by the persistence layer.</param>
    /// <returns>True when the write lost a uniqueness race.</returns>
    bool IsUniqueConstraintViolation(Exception exception);
}
```

**The implementation**, in `src/shared/src/BuildingBlocks.Infrastructure/Persistence/`:

```csharp
using _116.BuildingBlocks.Application.Persistence;
using Npgsql;

namespace _116.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL <see cref="IUniqueConstraintDetector" />, matching SQLSTATE 23505.
/// </summary>
public sealed class PostgresUniqueConstraintDetector : IUniqueConstraintDetector
{
    /// <inheritdoc />
    public bool IsUniqueConstraintViolation(Exception exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
        };
    }
}
```

**The strategy** resolves it the same way it already resolves `SharedExceptionMessage` — from
`context.RequestServices`, since `IExceptionStrategy` is registered as a singleton:

```csharp
/// <inheritdoc />
public override ProblemDetails CreateProblemDetails(DbUpdateException exception, HttpContext context)
{
    var msg = context.RequestServices.GetRequiredService<SharedExceptionMessage>();
    var detector = context.RequestServices.GetRequiredService<IUniqueConstraintDetector>();

    // The detail never echoes the constraint or column names the driver reports.
    bool isUniqueViolation = detector.IsUniqueConstraintViolation(exception);

    return CreateStandardProblemDetails(
        title: isUniqueViolation ? "ConflictException" : nameof(DbUpdateException),
        detail: isUniqueViolation ? msg.DuplicateResourceConflict() : msg.UnexpectedError(),
        statusCode: isUniqueViolation ? StatusCodes.Status409Conflict : StatusCodes.Status500InternalServerError,
        context: context
    );
}
```

Registration alongside the other Shared infrastructure services:

```csharp
services.AddSingleton<IUniqueConstraintDetector, PostgresUniqueConstraintDetector>();
```

The wire contract is unchanged: `23505` → 409 `ConflictException` with a value-free detail,
everything else → 500. The Stage 6 integration tests must pass untouched.

---

## 20.3 — Transient-fault detection behind an interface

**Fold this into Stage 11.** That stage rewrites `AccountStatusRequirementHandler` to fail closed
`[11 D3]`; doing the extraction first and the rewrite second edits the same method twice.

```csharp
// src/modules/Identity/Identity/src/Identity.Application/Shared/Authorizations/Contracts/ITransientFaultDetector.cs
namespace _116.Identity.Application.Shared.Authorizations.Contracts;

/// <summary>
/// Distinguishes a store that was unreachable from a store that answered. Authorization uses
/// this to decide whether a failed lookup is an outage or a defect.
/// </summary>
public interface ITransientFaultDetector
{
    /// <summary>
    /// Reports whether the failure means the datastore could not be reached.
    /// </summary>
    /// <param name="exception">The failure raised while reading.</param>
    /// <returns>True when the datastore was unreachable.</returns>
    bool IsUnreachable(Exception exception);
}
```

```csharp
// src/modules/Identity/Identity/src/Identity.Infrastructure/Persistence/PostgresTransientFaultDetector.cs
using _116.Identity.Application.Shared.Authorizations.Contracts;
using Npgsql;

namespace _116.Identity.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL <see cref="ITransientFaultDetector" />. Matches the connection-class SQLSTATEs
/// (08xxx) and the three shutdown codes (57P01–57P03).
/// </summary>
public sealed class PostgresTransientFaultDetector : ITransientFaultDetector
{
    private static readonly HashSet<string> UnreachableStates =
    [
        "08000", // connection_exception
        "08001", // sqlclient_unable_to_establish_sqlconnection
        "08003", // connection_does_not_exist
        "08004", // sqlserver_rejected_establishment_of_sqlconnection
        "08006", // connection_failure
        "08007", // transaction_resolution_unknown
        "57P01", // admin_shutdown
        "57P02", // crash_shutdown
        "57P03", // cannot_connect_now
    ];

    /// <inheritdoc />
    public bool IsUnreachable(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            NpgsqlException npgsql => npgsql.SqlState is not null && UnreachableStates.Contains(npgsql.SqlState),
            _ => false,
        };
    }
}
```

`TaskCanceledException` and `OperationCanceledException` are **deliberately dropped** — they are
ordinary client disconnects, and treating them as an outage is the exact defect `[07 §S12]` reports.
Cancellation propagates, and `OperationCanceledExceptionHandler` in `BuildingBlocks.Presentation`
already answers it at the pipeline level.

**This is the one behaviour change in the stage.** `AccountStatusRequirementHandlerTests` asserted
that both cancellation types fail closed; that theory now covers `TimeoutException` only, and a new
one asserts the cancellation propagates instead.

The handler then holds no driver knowledge:

```csharp
private static bool IsDbConnectivityError(Exception exception) // deleted
```

```csharp
catch (Exception exception) when (faultDetector.IsUnreachable(exception))
{
    // Stage 11 D3 decides what happens here: fail closed with 503, or fall back only for
    // RequireActiveUser. Either way the handler no longer asks the driver.
}
```

```csharp
services.AddSingleton<ITransientFaultDetector, PostgresTransientFaultDetector>();
```

---

## 20.4 — Tests

The nine SQLSTATEs have never been asserted. `NpgsqlException`'s `SqlState` is not settable, so
construct a `PostgresException`, whose constructor takes the code:

```csharp
// src/modules/Identity/Identity/tests/Identity.Unit.Tests/Infrastructure/Persistence/PostgresTransientFaultDetectorTests.cs
[Theory]
[InlineData("08000")]
[InlineData("08001")]
[InlineData("08003")]
[InlineData("08004")]
[InlineData("08006")]
[InlineData("08007")]
[InlineData("57P01")]
[InlineData("57P02")]
[InlineData("57P03")]
public void IsUnreachable_ReturnsTrue_ForConnectionClassSqlStates(string sqlState)
{
    var detector = new PostgresTransientFaultDetector();
    var exception = new PostgresException(
        messageText: "connection failure",
        severity: "FATAL",
        invariantSeverity: "FATAL",
        sqlState: sqlState
    );

    detector.IsUnreachable(exception).ShouldBeTrue();
}

[Theory]
[InlineData("23505")] // unique_violation — a defect, not an outage
[InlineData("23503")] // foreign_key_violation
public void IsUnreachable_ReturnsFalse_ForConstraintViolations(string sqlState) { … }

[Fact]
public void IsUnreachable_ReturnsFalse_ForClientDisconnect()
{
    new PostgresTransientFaultDetector().IsUnreachable(new TaskCanceledException()).ShouldBeFalse();
}
```

`PostgresUniqueConstraintDetector` gets the mirror set: `23505` wrapped in a `DbUpdateException`
→ true; `23503` → false; a `DbUpdateException` with no inner exception → false.

**Integration:** unchanged. The Stage 6 duplicate-write test must still return 409 and the
Stage 11 authorization tests must still behave per D3 — that is the whole regression surface.

---

## 20.5 — The rules that would have caught this

**The Domain rule already exists and passes.** `LayerDependencyTests` forbids
`Microsoft.AspNetCore`, `Microsoft.EntityFrameworkCore` and `Npgsql` in every `*.Domain`
namespace, iterating `ArchitectureRule.Modules` rather than naming one assembly, and
`KnownViolations.txt` is empty.

What is still missing is the Application rule. It goes in the same file, in the harness's own
style, and it stays red until 20.2 and 20.3 land:

```csharp
// tests/Architecture.Tests/LayerDependencyTests.cs
[Fact]
public void ApplicationNamesNoDatabaseDriver()
{
    foreach (Module module in ArchitectureRule.Modules)
    {
        Types
            .InAssemblies(module.Assemblies)
            .That()
            .ResideInNamespace($"{module.Root}.Application")
            .ShouldNot()
            .HaveDependencyOn("Npgsql")
            .GetResult()
            .ShouldHold($"{module.Name}.Application must not name the database driver");
    }
}
```

EF Core itself stays tolerated in `Application`: specifications are query objects `[04 §4.12]`.
Only the vendor driver is forbidden.

The rule ships at **zero allowlist entries**, so it must not be added before the two files are
fixed. `ApplicationDoesNotDependOnInfrastructure`, the third rule this section once deferred to
Stage 15, also exists now and passes.

---

## Verify

```bash
dotnet build --no-incremental          # 0 errors, 0 warnings
dotnet csharpier check .
dotnet test tests/unit.slnf
dotnet test tests/integration.slnf
dotnet test tests/Architecture.Tests
```

Plus the two greps this stage exists to make return nothing:

```bash
grep -rn --include='*.cs' "Npgsql" src/modules/*/*/src/*.Application src/shared/src/BuildingBlocks.Application src/shared/src/BuildingBlocks.Presentation
grep -rn --include='*.cs' "Microsoft.AspNetCore" src/modules/*/*/src/*.Domain
```

The second one already returns nothing.
