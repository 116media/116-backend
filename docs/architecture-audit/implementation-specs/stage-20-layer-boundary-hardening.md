# Stage 20 — Layer boundary hardening

Closes **[15 §15.1]** and **[15 §15.2]** — the two dependency-rule violations found by the
post-Stage-9 verification that docs 01–14 did not cover.

Eleven files. No behaviour change except where Stage 11 already intends one.

**Numbered 20, but it does not run last.** Both fixes are prerequisites, not follow-ups:
§15.2 must land before Stage 18 splits `Shared.Kernel` (the file stops compiling at that
moment), and §15.1 must be folded into Stage 11 (which rewrites the same method). The stage
number records where the finding entered the audit, not the execution slot.

> Draft — 15.1 is finalized against whatever Stage 11 D3 lands.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Where the SQLSTATE knowledge lives | keep in the handler, or an Infrastructure-owned detector | **Detector interface in Application, Npgsql implementation in Infrastructure.** The nine connectivity SQLSTATEs are driver knowledge; the handler's question is "was the store unreachable?". This also makes them testable — today no test asserts a single one of them. |
| D2 | One detector or two | one `IDatabaseFaultDetector`, or split by concern | **Two.** Unique-constraint detection lives in `Shared` (every module's `DbUpdateExceptionStrategy` path); transient-fault detection lives in `Identity` (one consumer, and Stage 11 may delete the fallback entirely). Merging them puts an Identity concern in Shared. |
| D3 | Newsletter token encoding | keep `WebEncoders`, add a Mailer helper, or use `System.Buffers.Text` | **`Base64Url.EncodeToString`.** Ships in the `net9.0` reference assemblies, identical alphabet and padding, so existing tokens stay valid. A hand-rolled helper would be three lines of transform to maintain for no gain. |
| D4 | Enforcement | trust review, or an architecture rule | **Architecture rule, owned by Stage 14.7.** This stage contributes the two rules; it does not build the harness. If Stage 14 has not landed, the rules ship here and 14.7 absorbs them. |

---

## Checklist

- [ ] 20.1 — `Mailer.Domain` off `Microsoft.AspNetCore.WebUtilities`
- [ ] 20.2 — `IUniqueConstraintDetector` + Npgsql implementation; `DbUpdateExceptionStrategy` off `Npgsql`
- [ ] 20.3 — `ITransientFaultDetector` + Npgsql implementation; `AccountStatusRequirementHandler` off `Npgsql`
- [ ] 20.4 — Unit tests for both detectors (the nine SQLSTATEs, `23505`, and the negative cases)
- [ ] 20.5 — Architecture rules: no `Npgsql`/`Microsoft.AspNetCore` in `*.Domain`; no `Npgsql` in `*.Application`
- [ ] 20.6 — Verify (build 0/0, csharpier, unit, integration, architecture)

---

## 20.1 — `Mailer.Domain` off the web stack

**Before** — `Mailer/Domain/Entities/NewsletterSubscriberEntity.cs`:

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
// Shared/Application/Exceptions/Handlers/Strategies/DbUpdateExceptionStrategy.cs — before
using Npgsql;

private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;

bool isUniqueViolation = exception.InnerException is PostgresException { SqlState: UniqueViolation };
```

**The contract**, in `Shared/Application/Persistence/`:

```csharp
namespace _116.Shared.Application.Persistence;

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

**The implementation**, in `Shared/Infrastructure/Persistence/`:

```csharp
using _116.Shared.Application.Persistence;
using Npgsql;

namespace _116.Shared.Infrastructure.Persistence;

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
// Identity/Application/Shared/Authorizations/Contracts/ITransientFaultDetector.cs
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
// Identity/Infrastructure/Persistence/PostgresTransientFaultDetector.cs
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
Cancellation propagates.

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
// tests/Unit/Modules/Identity/Infrastructure/Persistence/PostgresTransientFaultDetectorTests.cs
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

Contributed to `tests/Architecture` (harness owned by **[Stage 14.7]**):

```csharp
// tests/Architecture/LayerDependencyTests.cs
[Fact]
public void Domain_DoesNotDependOnFrameworkPackages()
{
    TestResult result = Types.InAssembly(typeof(ArticleEntity).Assembly)
        .That()
        .ResideInNamespaceMatching(@"_116\.\w+\.Domain")
        .ShouldNot()
        .HaveDependencyOnAny("Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "Npgsql", "Carter")
        .GetResult();

    result.IsSuccessful.ShouldBeTrue(result.FailingTypeNames.JoinOrEmpty());
}

[Fact]
public void Application_DoesNotDependOnTheDatabaseDriver()
{
    // EF is tolerated in specifications [04 §4.12]; the vendor driver is not.
    TestResult result = Types.InAssembly(typeof(PublicGetPopularArticlesHandler).Assembly)
        .That()
        .ResideInNamespaceMatching(@"_116\.\w+\.Application")
        .ShouldNot()
        .HaveDependencyOn("Npgsql")
        .GetResult();

    result.IsSuccessful.ShouldBeTrue(result.FailingTypeNames.JoinOrEmpty());
}
```

Both rules ship at **zero allowlist entries** — unlike the boundary rules in 14.7, which start with
enumerated debt. There are only eleven files to fix, so there is no debt to grandfather.

The third rule — `*.Application` must not depend on `*.Infrastructure` — cannot pass until the
query builders move `[06 §6.14]`, and belongs to Stage 15. Add it there with the four builders as
its allowlist, ratcheting to zero when they convert.

---

## Verify

```bash
dotnet build --no-incremental          # 0 errors, 0 warnings
dotnet csharpier check .
dotnet test tests/Unit
dotnet test tests/Integration
dotnet test tests/Architecture
```

Plus the two greps this stage exists to make return nothing:

```bash
grep -rn --include='*.cs' "using Npgsql" src/Modules/*/*/Application src/Shared/Shared/Application
grep -rn --include='*.cs' "Microsoft.AspNetCore" src/Modules/*/*/Domain
```
