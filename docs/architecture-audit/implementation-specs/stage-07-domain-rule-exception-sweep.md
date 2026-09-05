# Stage 7 — Invert localization-in-domain (full sweep)

Closes **[03 §3.6]** — 21 domain files across Content, Identity and Core import an Application-layer
`Errors` factory, and 75 domain method signatures take one as a parameter. A domain rule therefore
depends transitively on `IStringLocalizer` and the ambient `CultureInfo`.

Stage 6 built the replacement — `DomainRuleException`, per-module rule codes, and a strategy that
translates them — and proved it on the publication state machine. This stage applies it everywhere
and deletes the parameter.

> **No breaking change to the public API.** Status codes, `ProblemDetails.title` and the localized
> `detail` all stay byte-identical; see [7.2](#72-preserving-the-wire-format).
>
> **Depends on Stage 6** (branch stacks on the tree Stage 6 landed on).

---

## Design — the domain states the rule, the edge states the response

The audit's case is that welding a rule to its wording is what pushed guards out of the domain in
the first place: adding one costs a domain change *plus* an error method *plus* three `.resx` edits
*plus* a signature ripple, so people put the guard in a handler instead. §3.3 and §3.5 were both
symptoms.

The mechanism is already in the tree. What is left is scale, and scale surfaces one problem Stage 6
did not have to solve: **its strategy hardcodes 400, because both of its rules are bad requests.**
The 43 rules in this sweep do not agree:

| Today's exception | Status | Example rule |
| --- | --- | --- |
| `BadRequestException` | 400 | `NameRequired`, `SlugRequired`, `PriceMustBeNonNegative` |
| `ConflictException` | 409 | `AlreadyClaimed`, `PaymentAlreadyVerified`, `AlreadySubmitted` |
| `NotFoundException` | 404 | `PackageEntity.NotFound` |
| `AuthenticationException` | 401 | `InvalidEmailFormat` |
| `AccountNotVerifiedException` | 403 | `UserEntity.ValidateCanLogin` |
| `AccountInactiveException` | 423 | `UserEntity.ValidateCanLogin` |

So the sweep has to carry a status per rule without letting HTTP leak into the domain.

**The domain keeps emitting a bare code.** The status lives in the strategy's table, next to the
message it already resolves — one place that knows both how a rule is phrased and how it is
answered. Nothing in `Domain/` gains an HTTP concept, not even an indirect one.

The obvious risk is a rule whose strategy arm is never written, silently defaulting to 400 when it
should have been 409. That is closed by a test rather than by discipline: every constant declared on
a rule-codes class must have a table entry, asserted by reflection ([7.5](#75-the-completeness-guard)).

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Where the status lives | a `Kind` enum on the exception set at the throw site, or a per-code table in the strategy | **Strategy table.** A `Kind` enum would be HTTP semantics wearing a domain name (`Conflict`, `NotFound`, `Locked`); the domain has no opinion on how a refusal is transported. The completeness test removes the forgot-an-arm hazard that motivated the enum. |
| D2 | One shared strategy or one per module | a single `DomainRuleExceptionStrategy` in `Shared`, or one per module | **One per module, via module subclasses.** `ExceptionStrategyRegistry` keys strategies by concrete exception type, so two strategies cannot both register for `DomainRuleException`. Each module declares `ContentRuleException` / `IdentityRuleException` / `CoreRuleException : DomainRuleException` in its `Domain/Exceptions/`, and its strategy binds to the subclass; the registry's base-type walk dispatches natively. The shared base type stays. |
| D3 | `ProblemDetails.title` | title everything `DomainRuleException`, or keep the title the mapped status implies | **Keep the mapped title.** `ShouldBeProblem<T>` asserts `title == typeof(T).Name`, so preserving it keeps 111 integration files and every client contract untouched. The sweep becomes invisible on the wire. |
| D4 | Value objects | leave them, or convert their bare `ArgumentException` throws | **Convert.** Seven value objects (the audit counted six; `ExportFormat` is the seventh) throw raw `ArgumentException`, which no strategy handles — `new Email("garbage")` is a 500 today. They are domain guards with no localization at all, so they belong in the same mechanism. |
| D5 | `*Errors` factory classes | delete them, or keep them | **Keep.** Application-layer callers (handlers, factories, repositories) legitimately throw `NotFound(id)` and `SlugAlreadyExists`; only the *domain* stops calling them. Methods left with no caller at all are deleted at the end ([7.10](#710-retiring-what-is-left-unused)). |
| D6 | `.resx` keys | re-key by rule code, or leave them | **Leave.** The strategies format through the existing `*ErrorMessage` methods, so every key stays live and no translation is retouched. Re-keying would be a large diff with no behavioural gain. |
| D7 | Sequencing | one PR, or Content and Identity split | **One PR, entity by entity.** The mechanical change is uniform and the completeness test makes a half-done sweep fail loudly; splitting doubles the test churn on shared fixtures. |

---

## Checklist

- [x] 7.1 — `ContentRuleCodes` grown to 52 entity-scoped rules; new `IdentityRuleCodes` (19) and `CoreRuleCodes` (5)
- [x] 7.2 — Three strategies read per-aggregate problem catalogs merged into one code → (status, title, detail) lookup
- [x] 7.3 — 17 Content domain files drop the `errors` parameter
- [x] 7.4 — 4 Identity domain files drop the `errors` parameter
- [x] 7.5 — Completeness test per module: every declared code has a response
- [x] 7.6 — Seven value objects throw coded rules instead of `ArgumentException`
- [x] 7.7 — Core swept in full: `FileEntity.Create` drops `CoreI18n` (the audit's "unused import" was wrong)
- [x] 7.8 — Call sites and test builders stop passing `errors`
- [x] 7.9 — Unit tests assert the module rule exception and its code
- [x] 7.10 — 57 `*Errors` methods retired; 8 kept for their remaining Application callers
- [x] 7.11 — Verify (build 0/0, unit green; run integration locally)

---

## Part A — The mapping

### 7.1 Rule codes

`ContentRuleCodes` already exists with three constants. It grows to 52, one per (entity, rule)
pair, each documenting its args:

```csharp
public static class ContentRuleCodes
{
    // …the three Stage 6 codes stay unchanged…

    /// <summary>A required tag name was blank. Args: none.</summary>
    public const string TagNameRequired = "content.tag.name-required";

    /// <summary>A required album name was blank. Args: none.</summary>
    public const string AlbumNameRequired = "content.album.name-required";

    /// <summary>The order was already paid. Args: none.</summary>
    public const string OrderAlreadyPaid = "content.order.already-paid";

    // …52 total
}
```

**Codes are entity-scoped, not module-flat.** The spec first sketched `content.name-required`, but
"name required" is fifteen different localized sentences (album, artist, tag, …), and the strategy
must resolve each rule to the exact message it produced before the sweep — so the entity belongs in
the code. New `IdentityRuleCodes` (19: 13 entity rules + 6 value-object guards) and `CoreRuleCodes`
(5 file guards) mirror it under `identity.` and `core.`.

**Naming:** the code names the *rule*, not the message. `content.tag.name-required`, not
`content.tag.name-required-error`. Kebab-case after the `module.entity.` prefix; the three Stage 6
codes keep their original module-flat spelling for stability.

### 7.2 Preserving the wire format

Each strategy reads a table mapping every code to the status and title the rule answered with
*before* the sweep, plus a resolver producing the same localized detail — so nothing observable
changes. What a rule answers with is an RFC 9457 **problem** — the `ProblemDetails` the pipeline
already returns — so the type is named for that. One flat 52-entry dictionary inside the strategy
would be a god class that every new rule edits, so the catalog is composed instead: each aggregate
owns a small catalog next to its message ownership, and the strategy merges them once.

```csharp
// Shared contract — Application/Exceptions/Problems/
public sealed record RuleProblem(
    int Status,
    string Title,
    Func<HttpContext, IReadOnlyList<string>, string> Detail
);

// One catalog per aggregate, e.g. Exceptions/Problems/TagRuleProblems.cs
public sealed class TagRuleProblems : IRuleProblemCatalog
{
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } = new Dictionary<…>
    {
        [ContentRuleCodes.TagNameRequired] = new(
            StatusCodes.Status400BadRequest,
            nameof(BadRequestException),
            (ctx, _) => ctx.Resolve<TagErrorMessage>().NameRequired()
        ),
        …
    };
}

// The strategy is a lookup that never grows
private static readonly Dictionary<string, RuleProblem> Problems = RuleProblemCatalog.Merge(
    new PublicationRuleProblems(),
    new TagRuleProblems(),
    …
);
```

Adding a rule edits its owning catalog only; adding an aggregate adds one catalog file plus one
line in the merge. `RuleProblemCatalog.Merge` throws on a duplicate code, so two catalogs cannot
silently shadow each other.

`CreateProblemDetails` looks up the problem and emits `code` and `args` as extensions exactly as
Stage 6 does; an unmapped code still degrades to the code string as a 400.

**Naming and placement.** These classes are neither handlers nor rules — the *rule* is the guard in
the domain, and these say only what a violated rule answers on the wire. They live in
`Application/Shared/Exceptions/Problems/`, beside `Handlers/` rather than inside it.

**Why the title matters more than it looks.** `HttpResponseExtensions.ShouldBeProblem<TException>`
asserts `problem.Title == typeof(TException).Name`. Titling a 409 as `ConflictException` means all
111 integration files that assert `ShouldBeProblem<ConflictException>` keep passing without a single
edit — and any client branching on `title` sees no change. Retitling everything
`DomainRuleException` would be a 111-file test diff *and* a silent contract break.

**One oddity to settle before implementing.** `InvalidEmailFormat` currently returns
`AuthenticationException` → **401**. A malformed email address is a bad request, not an
authentication failure; 400 is almost certainly the correct answer and 401 looks like a
copy-paste from the login errors. The table can preserve 401 faithfully or fix it to 400 — **it
preserves 401 unless you say otherwise**, because silently changing a status mid-refactor is how
refactors get blamed for outages. Flagged here rather than decided.

---

## Part B — The sweep

### 7.3 Content — 17 files

Each file follows the same three steps: replace `throw errors.X()` with
`throw new ContentRuleException(ContentRuleCodes.X)`, delete the now-unused `errors` parameter, and
delete the `using _116.Content.Application.Shared.Errors;` line.

```csharp
// before
public static TagEntity Create(Guid id, string name, string slug, TagErrors errors)
{
    if (string.IsNullOrWhiteSpace(value: name))
    {
        throw errors.NameRequired();
    }
    …
}

// after
public static TagEntity Create(Guid id, string name, string slug)
{
    if (string.IsNullOrWhiteSpace(value: name))
    {
        throw new ContentRuleException(ContentRuleCodes.TagNameRequired);
    }
    …
}
```

Order, smallest blast radius first: `TagEntity`, `ContentTypeEntity`, `PricingTierEntity`,
`CategoryPricingEntity`, `PromotionLevelEntity`, `PackageSlotEntity`, `PackageEntity`,
`CustomerEntity`, `CategoryEntity`, `AlbumEntity`, `ArtistEntity`, `ShortVideoEntity`,
`ContentPaymentEntity`, `ContentOrderEntity`, then `ArticleEntity`, `VideoEntity`, `LyricsEntity` —
the three Stage 6 already touched, so their remaining uses go last.

`VideoEntity` is the only file that keeps a partial state today (Stage 6 converted one of its five
uses); after this it drops the import entirely.

### 7.4 Identity — 4 files

`UserEntity` (the bulk), `RoleEntity`, `PermissionEntity`, `VisitorPermissions`. Same shape against
`IdentityRuleCodes`.

`UserEntity.ValidateCanLogin` is the one to write carefully: its two rules map to **403** and
**423**, not 400, and those two statuses exist nowhere else in the sweep. They are also the rules
whose dedicated strategies (`AccountNotVerifiedExceptionHandler`, `AccountInactiveExceptionHandler`)
stay registered for the Application-layer callers that still throw those types directly.

### 7.6 Value objects

Seven value objects reject invalid input with a raw `ArgumentException`, which no strategy handles —
so any path reaching one returns a **500**. `Email` is the live example: the whitespace-address bug
Stage 6 fixed at the caller is still a 500 for any other malformed value that reaches
`new Email(...)`.

```csharp
// Email, ShareChannel, OtpPurpose, AuthProvider, SessionStatus, Client, ExportFormat
throw new ArgumentException($"Invalid share channel: {value}");
// becomes
throw new ContentRuleException(ContentRuleCodes.InvalidShareChannel, value);
```

This is the one part of the stage that **changes behaviour**: those paths go from 500 to a
localized 400. That is the fix, not a regression, but it is worth naming in the PR.

### 7.7 Core

The audit called Core's import unused; it is not. `FileEntity.Create` takes a `CoreI18n` parameter
and throws five localized `BadRequestException` guards through it — the same offense as the other
modules, so Core gets the same treatment: `CoreRuleException`, `CoreRuleCodes` (5 file guards), a
`FileRuleProblems` catalog behind its own strategy, and `Create` drops the `i18n` parameter.

### 7.8 Call sites

Every caller drops its `errors` argument — handlers, factories, repositories, seeders, and the test
builders under `tests/Fixtures/`. This is the largest mechanical slice and the compiler drives it
completely: delete the parameter, fix what stops compiling.

---

## Part C — Guardrails

### 7.5 The completeness guard

A rule whose strategy arm is missing would fall to the default and answer 400 — right for most
rules, wrong for the eight that are not 400. One test makes that impossible:

```csharp
[Fact]
public void EveryDeclaredRuleCode_ShouldHaveAStrategyResponse()
{
    IEnumerable<string> declared = typeof(ContentRuleCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f is { IsLiteral: true, FieldType: { } t } && t == typeof(string))
        .Select(f => (string)f.GetRawConstantValue()!);

    declared.Should().OnlyContain(code => DomainRuleExceptionStrategy.Handles(code));
}
```

The strategy exposes `Handles(string code)` for this. The same test exists for Identity and Core.
Adding a rule without an arm now fails a test rather than shipping a wrong status.

### 7.10 Retiring what is left unused

With the domain off the factories, the compiler decided what could go: 65 candidate `*Errors`
methods were deleted and the build restored the 8 that still have Application callers
(`AccountInactive`, `AccountNotVerified`, `EmailRequiredToSetPassword` and
`RoleAlreadyAssignedToUser` on `UserErrors`; `SlugRequired` and `ArtistNameRequired` on
`LyricsErrors`; `AlreadyClaimed` on `ArtistErrors`; `CannotAddItemToNonDraftOrder` on
`ContentOrderErrors`) — 57 retired. Their `*ErrorMessage` methods all stay: the rule maps now
resolve through them.

---

## Tests

- **Unit**
  - Every domain entity test asserting `Throw<BadRequestException>()` or `Throw<ConflictException>()`
    from a domain method becomes `Throw<DomainRuleException>()` **and additionally asserts the
    `Code`** — a strictly stronger assertion, since the type alone never distinguished which rule
    fired. About 20 files.
  - All three strategies: representative codes resolve the localized detail they resolved before,
    and the status and title match the pre-sweep exception (including the 401/403/423/404 rows).
  - The completeness guard above, per module.
  - Value objects: each invalid input throws `DomainRuleException` with its code, replacing the
    `Throw<ArgumentException>()` assertions, keyed by the module's rule exception.
- **Integration**
  - **No changes expected.** Status, title and detail are all preserved, so the 111 files asserting
    `ShouldBeProblem<BadRequestException>` / `<ConflictException>` must pass untouched. That is the
    proof the sweep is invisible on the wire — if one needs editing, the table has a wrong entry.
  - One new test per value object that was previously a 500, asserting the localized 400.

---

## Rollout

No migration, no configuration, no client coordination. The only behavioural change is the seven
value objects moving from 500 to 400 ([7.6](#76-value-objects)).

Worth a scan of production logs before merge for `ArgumentException` originating in
`Domain/ValueObjects/` — each occurrence is a request that returned 500 and will now return a
localized 400, and the count tells you whether any client is currently relying on that failure.

---

## Verification

1. `dotnet build` — 0 warnings, 0 errors.
2. `dotnet csharpier check .`
3. `dotnet test tests/Unit` — green.
4. `dotnet test tests/Integration` — green (run locally).
5. Confirm the domain is clean:
   `grep -rn "Application.Shared.Errors\|Errors errors" src/Modules/*/*/Domain/` returns nothing.
6. Confirm nothing regressed to a raw throw:
   `grep -rn "throw new ArgumentException" src/Modules/*/*/Domain/` returns nothing.
7. Confirm Core's domain no longer localizes: `grep -rn "CoreI18n" src/Modules/Core/Core/Domain/`
   returns nothing.

**PR:** `refactor(domain): remove i18n from the domain via coded exceptions`
