# 15 — Layer dependency verification

A re-run of the inward dependency rule against the tree **as it stands after Stage 9**, rather
than against the tree the original audit read. Every import edge in `src/` was enumerated in both
directions: `Domain → *`, `Application → Infrastructure`, and cross-module.

Two findings are new. The rest were already caught in docs 01–14, and three of them are now
**closed by completed stages** — recorded here so the audit stops reporting them.

---

## Verification result

| Rule | Status |
| --- | --- |
| `Domain` imports no `Application` or `Infrastructure` type | **Clean** in Content, Identity, Core, Shared — `Mailer` violates once (§15.2) |
| `Domain` imports no infrastructure package | **Clean** except `Mailer` (§15.2) |
| `Application` imports no module `Infrastructure` type | **9 imports across 8 files** — all the query builders `[06 §6.14]` |
| `Application` imports no database driver | **2 files** (§15.1) — both introduced or prescribed *after* the audit |
| No module imports another module's `Infrastructure` | **Clean** — zero occurrences |
| No `Application` file constructs an `Infrastructure` type | **Clean** — zero occurrences |

Method: `grep` over every `using` in `src/`, plus a fully-qualified-reference sweep to catch
imports written without a `using`. Counts below are from that sweep, not estimates.

---

## 15.1 The Postgres driver is referenced from the Application layer in two files

**Severity: Medium** · not covered by 01–14; both call sites postdate the audit.

**Where:**

```csharp
// Shared/Application/Exceptions/Handlers/Strategies/DbUpdateExceptionStrategy.cs:7,30
using Npgsql;
private const string UniqueViolation = PostgresErrorCodes.UniqueViolation;
bool isUniqueViolation = exception.InnerException is PostgresException { SqlState: UniqueViolation };

// Identity/Application/Shared/Authorizations/Handlers/AccountStatusRequirementHandler.cs:7,115
using Npgsql;
NpgsqlException npgsqlEx => npgsqlEx.SqlState switch { "08000" => true, … "57P03" => true, _ => false },
```

**Problem/why.** This is a layer below EF: not "the application knows it is persisted", but "the
application knows it is persisted **in PostgreSQL**". `PostgresErrorCodes` and `NpgsqlException`
are vendor types in the same assembly as the use cases, so the Application layer cannot compile
without the Npgsql driver, and swapping providers is a change to authorization logic. It also makes
the transient-fault rule untestable without a real Npgsql exception — the nine SQLSTATEs are
asserted nowhere.

Note both were **introduced by remediation, not found by it**: `DbUpdateExceptionStrategy` was
created by Stage 6 (6.10) at this exact path, and `[07 §S12]` explicitly prescribes *"narrow
`IsDbConnectivityError` to the Npgsql SQLSTATEs"*, which Stage 11 D3 carries forward. The audit's
own fixes put the driver there, which is precisely what an unenforced boundary allows `[02 §2.3]`.

**Solution.** Two abstractions owned by Infrastructure, injected as interfaces:

1. `IUniqueConstraintDetector` (Shared.Application contract, Npgsql implementation in
   Shared.Infrastructure) — `DbUpdateExceptionStrategy` asks *"is this a duplicate?"*, not *"is
   `SqlState == 23505`?"*.
2. `ITransientFaultDetector` (Identity.Application contract, Npgsql implementation in
   Identity.Infrastructure) — `AccountStatusRequirementHandler` asks *"was the store
   unreachable?"*. The nine SQLSTATEs move into the implementation and become unit-testable
   against a constructed `NpgsqlException`.

Sequence this **with** Stage 11 D3, not before it: that stage rewrites the fallback to fail closed,
and doing both edits to the same method twice is wasted work.

---

## 15.2 `Mailer.Domain` imports `Microsoft.AspNetCore.WebUtilities`

**Severity: Low** · not covered by 01–14.

**Where:** `Mailer/Domain/Entities/NewsletterSubscriberEntity.cs:6`, one call site at line 139:

```csharp
using Microsoft.AspNetCore.WebUtilities;

return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(MailerConstants.NewsletterTokenBytes));
```

**Problem/why.** This is the **only** non-`System` framework import in any domain layer in the
solution — Content, Identity, Core and Shared domains are otherwise clean. A domain entity that
cannot be constructed without the ASP.NET Core web stack cannot be exercised from a background job,
a replay, or a console tool, and it is the single line standing between "all four domains are pure"
and a rule that has to carry an exception.

It is invisible today because `Shared.csproj` already drags the web stack into every domain's
transitive graph `[01 §1.9]` — so the compiler never objects. When Stage 18 splits
`Shared.Kernel` out with zero packages, this file stops compiling.

**Solution.** `System.Buffers.Text.Base64Url.EncodeToString(...)`, which ships in the `net9.0`
reference assemblies and produces byte-identical output:

```csharp
using System.Buffers.Text;

return Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(MailerConstants.NewsletterTokenBytes));
```

Two lines, no behaviour change. Existing newsletter tokens remain valid because the encoding is the
same alphabet and padding rule. Do it before Stage 18 so the project split does not have to stop
and fix it mid-move.

---

## Already covered by 01–14

Recorded so these are not re-reported. Status is against the current tree, not the audit's.

| Finding | Audit reference | Stage | Status now |
| --- | --- | --- | --- |
| Query builders take `ContentDbContext`; even the `I*` interfaces leak it | `[06 §6.14]`, `[04 §4.11]` | 15 | **Open** — 4 builders + 4 interfaces. The 6 sibling builders returning `Specification<T>` are the target shape |
| No architecture tests, no banned-namespace rule | `[02 §2.3]` | 14.7 | **Open** — confirmed zero `NetArchTest`/`ArchUnit` references in `tests/` |
| One project per module; the dependency rule is folder names only | `[01 §1.9]`, `[11]` | 18 | **Open** — `Content.csproj` still contains all three layers |
| `Shared` drags Carter/Npgsql/Quartz/Bogus into every domain's graph | `[01 §1.9]` | 18.2 | **Open** |
| Core has no contracts project; 115 files bind its aggregate | `[02 §2.1]`, `[02 §2.9]` | 14.2, 14.4 | **Open** — 147 imports counted today (Content 122, Identity 25) |
| Domain takes `Errors` as a method parameter; 17 entity files import `Application.Shared.Errors` | `[03 §3.6]` | 7 | **Closed** — 0 occurrences; domains raise `DomainRuleException` |
| `UserEntity.Create(..., UserErrors errors)`; `VisitorPermissions` imports `Application.Shared` | `[07 §A4]` | 7 | **Closed** — `VisitorPermissions` imports only `Identity.Domain.Entities` |
| `EF.Functions.ILike` in specifications (17 files, 45 uses) | `[04 §4.12]` | 15 | **Open, accepted** — a specification is a query object; the cost is that predicates cannot be evaluated in memory |
| Carter/ASP.NET in `Application` (293 endpoints) | `[11]`, `[10]` | 18 D1 | **Open, accepted by decision** — endpoints stay in `<M>.Application`; a presentation project would shred the vertical slices |

## Explicitly not treated as violations

- **`IFormFile` in 29 commands and handlers.** ASP.NET is already in the Application layer by the
  Stage 18 D1 decision; a command carrying the transport's file abstraction is consistent with that
  choice, not a separate leak. It reaches `Domain` in **0** files.
- **`IMemoryCache` in 4 query handlers.** `Microsoft.Extensions.Caching.Abstractions` is an
  abstractions package, so the dependency direction holds. The real defect is coupling to an
  in-process, token-evicted strategy — tracked in
  [repository-and-caching/03-current-state.md](../repository-and-caching/03-current-state.md), not here.
- **`Microsoft.Extensions.Logging` / `Localization` in Application.** Abstractions packages,
  dependency-inverted by design.
