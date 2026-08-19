# Stage 6 — Domain state-machine guards, order total & payment proof

Closes **[03 §3.3]** (every editorial state transition is unguarded; the real state machine lives
copy-pasted in 11 handlers), **[03 §3.4]** (the order total is not recalculated when an item is
added, so the payment snapshots a stale amount), **[03 §3.5]** (`ContentPaymentEntity.Verify`
accepts a payment with no proof attached), and **[06 §6.15]**'s immediate half (a unique-constraint
violation reaches the default handler as a 500 instead of a 409).

The four are one theme: **the domain accepts states it should refuse, and the guard that would
refuse them lives somewhere else** — in a handler, in a caller that forgot, or nowhere.

> **No breaking change to the public API.** Every guard this stage adds refuses an input the system
> should already have refused, and the error body keeps its localized `detail` — clients gain a
> machine-readable `code` extension alongside it.

> **Depends on Stage 5** (branch stacks on the tree Stage 5 landed on).

---

## Design — put the invariant where it cannot be skipped

Three of these findings share a root cause. `ArticleEntity.Publish()` checks only whether the
article is *already* `Published`; it accepts every other source state. The rule that actually
matters — you may only publish something `Approved` — is written in
`AdminPublishArticleHandler.cs:40`. The entity is not the authority on its own lifecycle; the
handler is, and there are eleven of them.

That is survivable exactly as long as each entity method has one caller. It stops being
survivable the moment a second caller appears, and this stage's own work adds callers. The
concrete holes today:

- A paid article sitting in `PendingPayment` can be published without the customer ever paying —
  nothing between `Publish()` and the database says otherwise. Only the handler's guard stops it,
  and `AdminArchiveArticleHandler` demonstrates what happens when a handler simply omits one:
  `Archive()` accepts `Draft`, `PendingPayment`, anything.
- `Submit()` on a `Published` article moves it back to `PendingPayment` and raises **no**
  unpublished event — unlike `Reject` and `Archive`, which both raise `ArticleUnpublishedEvent`
  when they leave the published state. The homepage cache and search index keep serving a page
  that is no longer published.
- An `Archived` article can be republished with a fresh `PublishedAt`, silently losing its
  original publication date.

The same shape produces the money bugs. `RecalculateTotalFromItems` is public with five callers,
and `AdminAddOrderItemFactory` is not one of them — it creates the item, commits, and returns.
`ContentPaymentEntity.Verify` guards its own status but never asks whether any proof exists.

So the fix is uniform: **move each invariant to the one place that cannot be bypassed**, and delete
the copies.

For the publication state machine that place is the entity, because `Status` has a `private set` —
the entity is the only thing that can perform the mutation, so it must be the thing that knows the
rules. Every one of the 20 call sites that invokes a transition today is a handler; there is no
factory in the Editorial slice to host the rule, and `OrderPaidEffectsHandler` mutates content
state straight from an event handler, which no use-case-level layer would cover. The rule itself is
a pure function of one field — `(from, to) → bool`, no repository, no services — which is what
makes it domain logic rather than application logic.

The `DbUpdateException` strategy is a different animal — not a missing invariant but a missing
translation. The database already enforces the uniqueness that 28 interaction handlers check for
in application code; when a race beats the check, the resulting `23505` has no strategy and falls
through to a 500. One strategy fixes all 28 call sites without touching any of them.

---

## Decisions

All settled — the spec below implements the **Decision** column. D1 and D3 were decided in review:
the domain must not reach into the Application layer even though one project makes it possible, so
it throws coded exceptions and the Application layer's strategy translates them. That makes this
stage the home of **[03 §3.6]**'s mechanism; the follow-up stage applies it to every remaining
`errors` signature (the sweep now scheduled as Stage 7).

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Where the transition table lives | the audit says `Domain/Entities/ContentPublicationState.cs`, or a non-`Entities` folder | **`Domain/StateMachines/ContentPublicationState.cs`.** It is a static policy, not a persisted entity; putting it under `Entities/` next to 49 real entities invites a future reader to look for a table. The audit's file name is kept. |
| D2 | Method signature after guarding | **throw** on an illegal source (audit), or keep returning `bool` for everything | **Throw on illegal, keep `bool` for idempotent.** `false` keeps its current single meaning — "already in the target state, nothing done" — and an illegal source becomes an exception. Handlers keep their `AlreadyPublished`-style conflict responses unchanged. |
| D3 | How the domain raises the error | pass an `errors` factory in from Application (today's pattern), or throw a coded **domain** exception the Application layer translates | **Coded domain exception, translated by the Application layer.** The domain throws a culture-free `code` + `args` and never reaches into Application; the exception strategy maps the code to the existing localized message, exactly as `NotFoundExceptionHandler` already does for `EntityName`. `detail` stays localized and `code` is added as an extension so clients can branch without string-matching a sentence. This lands **[03 §3.6]**'s mechanism here — see [6.1](#61-one-transition-table). |
| D4 | Scope of the `23505` fix | strategy only, or strategy **+** the `TryAddLikeAsync` (`ON CONFLICT DO NOTHING`) collapse | **Strategy only.** It closes the 500 across all 28 handlers in one file. The `ON CONFLICT` collapse rewrites 28 handlers and 4 repositories and belongs with the atomic-counter work in Stage 8, which touches the same files. |
| D5 | Status code for a lost insert race | **409 Conflict**, or 200 (treat as idempotent success) | **409.** It matches what the handlers already return when their own pre-check catches the duplicate, so a client sees one answer for one situation regardless of who won the race. Making like/unlike genuinely idempotent is a deliberate API change, not a bug fix. |
| D6 | `MarkPendingReview` | route it through the table too, or leave it | **Leave it.** Its `bool` return is load-bearing for `OrderPaidEffectsHandler` event replay (`changed \|= …` at three call sites) and its allowed-source set is already explicit and correct. Guarding it would convert replayed events into exceptions. |

---

## Checklist

- [x] 6.1 — `ContentPublicationState` table + `EnsureCanMove` + `DomainRuleException` + `ContentRuleCodes` + the translating strategy ([03 §3.6]'s mechanism, delivered here)
- [x] 6.2 — Route `Article`/`Video`/`Lyrics` `Submit`/`Approve`/`Publish`/`Reject`/`Archive` through it; drop `VideoEntity.Publish`'s `errors` parameter
- [x] 6.3 — Raise the missing unpublished event when `Submit` leaves `Published`
- [x] 6.4 — Delete the duplicated source-state guards from the 11 Admin handlers
- [x] 6.5 — `AdminAddOrderItemFactory` recalculates the total before committing
- [x] 6.6 — `RecalculateTotalFromItems` becomes private behind `AddItem`/`RemoveItem`
- [x] 6.7 — `AdminSubmitOrderFactory` guard tightened from `Any` to `All`
- [x] 6.8 — `ContentPaymentEntity.Verify` requires proof and method
- [x] 6.9 — `ContentPaymentEntity.AttachProof` refuses a decided payment
- [x] 6.10 — `DbUpdateExceptionStrategy` maps `23505` → 409
- [x] 6.11 — Unit + integration tests (the 5 endpoint tests asserting the localized transition
      message must pass unchanged — proof the guard moved without changing the contract)
- [x] 6.12 — Verify (build 0/0, unit green; run integration locally)

---

## Part A — The publication state machine `[03 §3.3]`

### 6.1 One transition table

New `Domain/StateMachines/ContentPublicationState.cs`. The table is identical for Article, Video
and Lyrics, which is why it lives once:

```csharp
public static class ContentPublicationState
{
    private static readonly Dictionary<EnumContentStatus, EnumContentStatus[]> Allowed = new()
    {
        [EnumContentStatus.Draft] = [EnumContentStatus.PendingPayment, EnumContentStatus.PendingReview],
        [EnumContentStatus.PendingPayment] = [EnumContentStatus.PendingReview, EnumContentStatus.Rejected],
        [EnumContentStatus.PendingReview] = [EnumContentStatus.Approved, EnumContentStatus.Rejected],
        [EnumContentStatus.Approved] =
        [
            EnumContentStatus.Published,
            EnumContentStatus.Rejected,
            EnumContentStatus.Archived,
        ],
        [EnumContentStatus.Published] = [EnumContentStatus.Archived, EnumContentStatus.Rejected],
        [EnumContentStatus.Rejected] = [EnumContentStatus.PendingReview, EnumContentStatus.Archived],
        [EnumContentStatus.Archived] = [EnumContentStatus.PendingReview],
    };

    public static bool CanMove(EnumContentStatus from, EnumContentStatus to) =>
        Allowed.TryGetValue(from, out EnumContentStatus[]? targets)
        && Array.IndexOf(targets, to) >= 0;
}
```

The guard lives on the table too, so no entity needs a copy:

```csharp
public static void EnsureCanMove(EnumContentStatus from, EnumContentStatus to, EnumCoreContentType contentType)
{
    if (!CanMove(from: from, to: to))
    {
        throw new DomainRuleException(
            ContentRuleCodes.InvalidStatusTransition,
            contentType.ToString(),
            from.ToString(),
            to.ToString()
        );
    }
}
```

**The domain does not reach into the Application layer.** `Domain/` and `Application/` share one
project and 17 domain files already import `Application.Shared.Errors` — that coupling is
**[03 §3.6]**, and this stage will not add an eighteenth. So the state machine throws a
culture-free domain exception, and the Application layer's exception strategy translates it.

**[03 §3.6]**'s mechanism lands here; Stage 7 sweeps the remaining signatures with it.

The domain states the fact — "Draft cannot become Published" — as a culture-free code, and the
Application layer turns that into a sentence in the caller's language. That split is what §3.6 asks
for: the rule stops depending on `IStringLocalizer` and the ambient `CultureInfo`, so it can be
evaluated in a background job or an event replay, while the user still gets a translated message.

New `Domain/Exceptions/DomainRuleException.cs`, the first file in that folder:

```csharp
public class DomainRuleException(string code, params string[] args)
    : Exception($"Domain rule violated: {code}.")
{
    /// <summary>Stable, culture-free identifier of the rule that refused the operation.</summary>
    public string Code => code;

    /// <summary>Positional context for the rule, in the order the code documents.</summary>
    public IReadOnlyList<string> Args => args;
}
```

The `Exception.Message` is developer-facing and reaches logs only — never a response body. Codes live
beside the rules they guard, in `Domain/StateMachines/ContentRuleCodes.cs`:

```csharp
public static class ContentRuleCodes
{
    /// <summary>Args: [0] content type, [1] source status, [2] target status.</summary>
    public const string InvalidStatusTransition = "content.invalid-status-transition";

    /// <summary>Args: none.</summary>
    public const string PublicationRequiresYoutubeUrl = "content.publication-requires-youtube-url";
}
```

So `EnsureCanMove` throws:

```csharp
throw new DomainRuleException(
    ContentRuleCodes.InvalidStatusTransition,
    contentType.ToString(),
    from.ToString(),
    to.ToString()
);
```

The strategy lives in `Content/Application/Shared/Exceptions/Handlers/` and translates the code to
the existing localized message, resolving localizers from `RequestServices` exactly as
`NotFoundExceptionHandler` and `StreamingLinkResolutionExceptionHandler` already do:

```csharp
public sealed class DomainRuleExceptionStrategy
    : BaseExceptionStrategy<DomainRuleException>
{
    public override ProblemDetails CreateProblemDetails(
        DomainRuleException exception,
        HttpContext context
    )
    {
        string detail = exception.Code switch
        {
            ContentRuleCodes.InvalidStatusTransition => ResolveTransitionMessage(exception, context),
            ContentRuleCodes.PublicationRequiresYoutubeUrl => context
                .RequestServices.GetRequiredService<VideoErrorMessage>()
                .CannotPublishWithoutYoutubeUrl(),
            _ => exception.Code,
        };

        ProblemDetails problem = CreateStandardProblemDetails(
            title: nameof(DomainRuleException),
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest,
            context: context
        );

        problem.Extensions["code"] = exception.Code;
        problem.Extensions["args"] = exception.Args;

        return problem;
    }
}
```

`ResolveTransitionMessage` reads `Args[0]` to pick `ArticleErrorMessage`, `VideoErrorMessage` or
`LyricsErrorMessage` and calls `InvalidStatusTransition(Args[1], Args[2])` — a method that already
exists on all three with this exact signature, as does `CannotPublishWithoutYoutubeUrl` on
`VideoErrorMessage`. So **no new message keys and no `.resx` changes**: the client receives the same
localized `detail` it receives today, and the five endpoint tests that assert it keep passing
untouched.

Response body for publishing a `Draft` article (with `Accept-Language: en`):

```json
{
  "title": "DomainRuleException",
  "detail": "Invalid status transition from Draft to Published.",
  "status": 400,
  "code": "content.invalid-status-transition",
  "args": ["Article", "Draft", "Published"],
  "traceId": "…",
  "timestamp": "…"
}
```

`detail` stays what users see; `code` and `args` are new extensions giving clients a stable value to
branch on instead of string-matching a translated sentence.

Three consequences, stated plainly:

- **Every domain rule maps to 400**, and an unmapped code degrades to the code string as `detail` —
  a translation gap, never a 500. A future rule added before its strategy arm still refuses the
  operation safely.
- **The `*Errors` factory methods stay.** `ArticleErrors.InvalidStatusTransition` and its siblings
  remain for Application-layer callers; only the domain stops calling them. The `.resx` keys remain
  live because the strategy formats through the same `*ErrorMessage` classes.
- **Stage 7 inherits a finished mechanism** — exception, code convention, strategy — and is
  reduced to converting the remaining ~75 signatures across Content and Identity, adding a code
  constant and a strategy arm per rule.

### 6.2 Routing the transitions

Each method keeps its idempotent early return and gains the guard behind it. `Publish` shown; the
other four follow the same shape:

```csharp
public bool Publish()
{
    if (Status == EnumContentStatus.Published)
    {
        return false;
    }

    ContentPublicationState.EnsureCanMove(
        from: Status,
        to: EnumContentStatus.Published,
        contentType: EnumCoreContentType.Article
    );

    Status = EnumContentStatus.Published;
    PublishedAt = DateTimeOffset.UtcNow;
    // …events unchanged
    return true;
}
```

The ordering matters: the idempotent check runs **first**, so re-publishing a `Published` article
still returns `false` (and the handler still answers `AlreadyPublished`) rather than throwing
"cannot move Published → Published".

**Signatures get simpler, not wider.** Because the guard throws a domain exception, no method gains
an `errors` parameter — and `VideoEntity.Publish(VideoErrors errors)` **loses** the one it has
today. Its remaining use of `errors` is the YouTube-URL gate, which becomes a second domain
exception:

```csharp
public bool Publish()   // was: Publish(VideoErrors errors)
{
    if (Status == EnumContentStatus.Published) { return false; }

    ContentPublicationState.EnsureCanMove(Status, EnumContentStatus.Published, EnumCoreContentType.Video);

    if (string.IsNullOrWhiteSpace(YoutubeVideoUrl))
    {
        throw new DomainRuleException(ContentRuleCodes.PublicationRequiresYoutubeUrl);
    }
    // …unchanged
}
```

`AdminPublishVideoHandler` stops passing `i18n.Video`. The file's `Application.Shared.Errors`
import survives — `VideoEntity` still uses `VideoErrors` in `Update`, `AttachYoutubeVideoUrl` and
`ForceUnpromote` — so retiring the import itself is Stage 7's job; this stage converts one of its
five uses and adds none.

`Archive` is the one that changes behaviour most visibly: today it accepts any source, and after
this it accepts only `Approved`, `Published` and `Rejected`. Archiving a `Draft` becomes a 400
instead of silently succeeding.

### 6.3 The missing unpublished event

`Submit` on a `Published` article is illegal under the table (`Published` → `PendingPayment` is not
listed), so 6.2 closes the hole by refusal. The event asymmetry is still worth fixing for the paths
that remain legal, so `Submit` gets the same `wasPublished` treatment `Reject` and `Archive`
already have — cheap, and it removes a trap for whoever edits the table next.

### 6.4 Deleting the duplicated guards

Eleven handlers carry a source-state `if` that the entity now owns. The **idempotent** guard stays
(it maps `false` → `AlreadyPublished`); only the `InvalidStatusTransition` block goes:

| Handler | Lines removed |
| --- | --- |
| `AdminPublishArticleHandler` / `Video` / `Lyrics` | the `!= Approved` block |
| `AdminApproveArticleHandler` / `Video` / `Lyrics` | the `!= PendingReview` block |
| `AdminRejectArticleHandler` / `Video` / `Lyrics` | the source-state block |
| `AdminUpdateArticleHandler` / `Video` | the editability block — its rule moves into the entity `Update` methods as `ContentPublicationState.EnsureEditable` (`content.not-editable`), phrased by the strategy exactly as the handlers phrased it |

The `i18n.Article.InvalidStatusTransition` call does **not** move into the entity — it moves into
the strategy, which is now the only place that knows how to phrase the rule.

Six endpoint tests assert this path today (`AdminPublishArticle/Lyrics/Video`,
`AdminApproveVideo`, `AdminRejectVideo`, plus the Video no-YouTube-URL case). The status and the
localized `detail` stay byte-identical; the one thing that changes is the ProblemDetails **title**,
which the strategy convention derives from the exception type — so each test's
`ShouldBeProblem<BadRequestException>` becomes `ShouldBeProblem<DomainRuleException>`, and
nothing else in them moves. The three `*ErrorsTests` unit files pass untouched: the factories they
test still exist for Application-layer callers.

Two behavioural widenings ride along with the table, both intended by the audit's design:

- **Reject** accepted only `PendingReview` in the handlers; the table also allows rejecting
  `PendingPayment`, `Approved` and `Published` (raising the unpublished event on the last).
- **Archive** accepted anything; the table narrows it to `Approved`, `Published` and `Rejected`,
  so archiving a `Draft` becomes a 400.

---

## Part B — Order total `[03 §3.4]`

### 6.5 Recalculate on add

`AdminAddOrderItemFactory` creates the item and commits without recalculating. The immediate fix is
two lines before the commit:

```csharp
await contentOrderRepository.AddItemAsync(item: item, ct: cancellationToken);

order.Items.Add(item);
order.RecalculateTotalFromItems();
await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);

await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
```

The reproduction the audit describes is real and worth a test verbatim: add item A → add a tier to
A (recalc fires) → add item B carrying a promotion price and no tiers (**no recalc**) → submit. The
payment freezes a total missing B's promotion price. The customer is invoiced short and
`OrderPaidEvent` still stamps B's promotion.

### 6.6 Making the omission impossible

The two-line fix leaves the next caller free to forget. `RecalculateTotalFromItems` becomes
`private`, and the two mutations that need it move onto the aggregate:

```csharp
public void AddItem(ContentOrderItemEntity item)
{
    Items.Add(item);
    RecalculateTotalFromItems();
}

public void RemoveItem(ContentOrderItemEntity item)
{
    Items.Remove(item);
    RecalculateTotalFromItems();
}
```

The five existing callers of the public method become calls to `AddItem`/`RemoveItem`, or keep a
direct recalculation where they mutate tiers rather than items — `AdminAddItemTierFactory`,
`AdminRemoveItemTierHandler` and `AdminEditOrderItemHandler` change an item's tiers in place, so
they need a public `RecalculateTotals()` seam or the tier mutation itself moves onto the aggregate.
**The tier-level move is the larger change and is not in this stage**; the seam is, so the method
stays reachable for those three and private for item add/remove. Full aggregate ownership of items
and tiers is **[03 §3.1]**, scheduled for the restructure stages.

### 6.7 Submit guard

`AdminSubmitOrderFactory` requires `order.Items.Any(i => i.Tiers.Count > 0)` — one item with a tier
is enough to submit an order whose other items have none. Tightened to `All`:

```csharp
if (!order.Items.All(i => i.Tiers.Count > 0))
{
    throw contentOrderErrors.MustHaveAtLeastOneItemWithTier();
}
```

Bonus items are excluded from the total but still need tiers to be reviewable, so the guard applies
to every item. The error message name is now slightly wrong for what it asserts; renaming it is a
`.resx` change across three locales and is **deferred** — noted here so it is not mistaken for an
oversight.

---

## Part C — Payment proof `[03 §3.5]`

### 6.8 `Verify` requires evidence

A `Pending` payment with `PaymentProofFileId == null` verifies cleanly today: the order flips to
`Paid`, `OrderPaidEvent` fires, and every commissioned item moves to `PendingReview`. Content goes
live off an unevidenced payment with no artefact to dispute later.

```csharp
public void Verify(Guid adminUserId, string receiptUrl, ContentOrderErrors errors)
{
    if (Status == EnumPaymentStatus.Verified)
    {
        throw errors.PaymentAlreadyVerified();
    }

    if (Status == EnumPaymentStatus.Rejected)
    {
        throw errors.PaymentAlreadyRejected();
    }

    if (PaymentProofFileId is null || PaymentMethod is null)
    {
        throw errors.PaymentProofRequired();
    }

    // …unchanged
}
```

**Why this one keeps `errors` while Part A drops it.** `ContentPaymentEntity` has three methods
(`Verify`, `Reject`, and now `AttachProof`) that already take `ContentOrderErrors`, and its two
existing guards throw through it. Converting only the new guard would leave one method throwing two
different ways; converting the whole class means moving five error methods behind domain exceptions
and rewriting their callers — a change with no behavioural payoff, in a class this stage is
touching for a different reason.

So the rule this stage follows is: **where a new dependency-free component is introduced (Part A's
state machine), it stays free of the Application layer; where an existing `errors`-coupled method
gains a guard (Part C), the guard matches its neighbours.** Retiring the remaining `errors`
parameters across all 17 domain files is **[03 §3.6]**, swept in Stage 7; Part A shrinks that list by
one rather than growing it.

### 6.9 `AttachProof` refuses a decided payment

`AttachProof` has no status guard at all, so proof can be overwritten on an already-verified
payment — destroying the evidence the verification was based on.

```csharp
public void AttachProof(Guid proofFileId, EnumPaymentMethod paymentMethod, ContentOrderErrors errors)
{
    if (Status != EnumPaymentStatus.Pending)
    {
        throw errors.PaymentAlreadyDecided();
    }

    PaymentProofFileId = proofFileId;
    PaymentMethod = paymentMethod;
}
```

Two new error methods on `ContentOrderErrors`, each with a message in the three `.resx` locales:

| Method | Type | Message key |
| --- | --- | --- |
| `PaymentProofRequired()` | `BadRequestException` | `PaymentProofRequired` |
| `PaymentAlreadyDecided()` | `ConflictException` | `PaymentAlreadyDecided` |

---

## Part D — The lost insert race `[06 §6.15]`

### 6.10 `DbUpdateException` → 409

28 interaction handlers follow load → `HasLikedAsync` probe → throw `AlreadyLiked` → insert →
commit. The probe and the insert are not atomic, so two concurrent double-taps both pass the probe,
both reach `SaveChanges`, and the loser gets a `23505` unique violation. No strategy handles
`DbUpdateException`, so it reaches `DefaultExceptionHandler` as a **500** — on mobile clients that
retry over flaky networks, the single most likely 500 in production.

New `DbUpdateExceptionStrategy` in `Shared/Application/Exceptions/Handlers/Strategies/`. `Shared`
already references `Npgsql.EntityFrameworkCore.PostgreSQL`, so reading `SqlState` adds no
dependency:

```csharp
public sealed class DbUpdateExceptionStrategy : BaseExceptionStrategy<DbUpdateException>
{
    private const string UniqueViolation = "23505";

    public override ProblemDetails CreateProblemDetails(DbUpdateException exception, HttpContext context)
    {
        bool isUniqueViolation =
            exception.InnerException is PostgresException { SqlState: UniqueViolation };

        return CreateStandardProblemDetails(
            nameof(DbUpdateException),
            detail: isUniqueViolation ? /* localized conflict */ : /* localized generic */,
            statusCode: isUniqueViolation
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status500InternalServerError,
            context: context
        );
    }
}
```

Only `23505` becomes a 409. Every other `DbUpdateException` — a foreign-key violation, a not-null
violation — stays a 500, because those are genuine defects and demoting them would hide bugs. The
detail string must not echo the constraint name: it names internal columns.

Registration follows the existing convention (the registry discovers `IExceptionStrategy`
implementations from DI), so no `Program.cs` change is expected — confirm during implementation.

---

## Tests

- **Unit**
  - `ContentPublicationState`: every legal pair returns true; a representative illegal set
    (`Draft`→`Published`, `PendingPayment`→`Published`, `Archived`→`Published`,
    `Published`→`PendingPayment`, `Draft`→`Archived`) returns false; an unmapped source returns
    false rather than throwing.
  - `ArticleEntity`/`VideoEntity`/`LyricsEntity`: each of the five transitions succeeds from every
    legal source, throws `DomainRuleException` from an illegal one, and still returns
    `false` from its own target state. `Publish` from `Approved` stamps `PublishedAt` and raises
    both events; `Publish` from `PendingPayment` throws and raises none. `Archive` from `Draft` now
    throws. The thrown exception carries `InvalidStatusTransition` and the content type, source and
    target as `Args`.
  - `DomainRuleExceptionStrategy`: each of the three content types resolves its own
    localized `InvalidStatusTransition` message into `detail`; the YouTube code resolves
    `CannotPublishWithoutYoutubeUrl`; `code` and `args` extensions carry the thrown values; an
    **unmapped** code falls back to the code string as `detail`, still 400 — never a 500.
  - `VideoEntity.Publish` throws `PublicationRequiresYoutubeUrl` with no URL, and the transition
    guard runs before the URL check (a `Draft` video with no URL reports the transition failure,
    not the URL one).
  - `ContentOrderEntity`: `AddItem` recalculates; bonus items stay excluded; `RemoveItem`
    recalculates; the audit's A→tier→B(promo, no tier) sequence produces a total including B.
  - `ContentPaymentEntity`: `Verify` throws without a proof file, throws without a method, succeeds
    with both; `AttachProof` throws on `Verified` and on `Rejected`, succeeds on `Pending`.
  - `DbUpdateExceptionStrategy`: `23505` → 409; another `SqlState` → 500; a `DbUpdateException`
    with no `PostgresException` inner → 500; the detail never contains the constraint name.
- **Integration**
  - Publishing an article that is `PendingPayment` returns 400 and leaves the row unpublished —
    the money hole, driven through the real admin endpoint
    (`AdminPublishArticleEndpointV1Tests`).
  - Archiving a `Draft` article returns 400 — the new refusal
    (`AdminArchiveArticleEndpointV1Tests`).
  - The add-item → add-tier → add-promo-item → submit flow, all through real endpoints, freezes a
    `ContentPaymentEntity.AmountUsd` equal to both tiers plus the promotion price; and an order
    with any tierless item refuses to submit (`OrderTotalIntegrityFlowTests`).
  - Verifying a payment with no proof returns 400 and leaves both the payment `Pending` and the
    order `PendingPayment` (`AdminVerifyPaymentEndpointV1Tests`).
  - Attaching proof to a verified payment returns 409 and keeps the original proof
    (`AdminAttachPaymentProofEndpointV1Tests`).
  - Two concurrent likes on one article: one 200, one 409, exactly one row persisted, no 500
    (`InteractionRaceFlowTests`).

The 11 handlers losing their guards keep their existing endpoint tests — those tests must still
pass unchanged, which is the proof the guard moved rather than disappeared.

---

## Rollout

No migration, no configuration, no data backfill, no client coordination — `detail` stays the same
localized sentence, and the new `code`/`args` extensions are additive. The one risk is behavioural:
**transitions that silently succeeded now return 400.**

Worth checking against production data before merge — whether any live row is in a state the new
table would have refused to reach (an `Archived` row with a `PublishedAt` later than its archive
audit stamp, or a `Published` row whose order is unpaid). Such rows are evidence one of these holes
was already exercised; they stay readable and publishable under the new table, but they should be
known about rather than discovered later.

---

## Verification

1. `dotnet build` — 0 warnings, 0 errors.
2. `dotnet csharpier check .`
3. `dotnet test tests/Unit` — green.
4. `dotnet test tests/Integration` — green (run locally).
5. Confirm the guard actually moved: `grep -rn "InvalidStatusTransition" src/Modules/Content/Content/Application/` should return only the error-factory and message files, no handlers.
