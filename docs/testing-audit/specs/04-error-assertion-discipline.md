# Spec 04 — Error assertion discipline

## Goal

`ShouldBeProblem` is the most-used assertion in the integration suite — 483 calls
across 228 files — and it is the weakest. A status code is not a reason: every
non-trivial handler reaches 404, 400 and 409 from several distinct guards, and the
test name always claims one of them. Two named branches are proven below to be
incapable of producing the status their test asserts.

This spec makes the strong form of the assertion the default. An error assertion
pins **three** things — the HTTP status, the ProblemDetails `Title`, and the exact
localized `Detail`:

```csharp
await response.ShouldBeProblem<NotFoundException>(
    HttpStatusCode.NotFound,
    Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
);
```

Nothing in `src/` changes. The whole mechanism is built out of what the application
already emits.

Backing finding: [../integration/08-assertion-escape-hatches.md](../integration/08-assertion-escape-hatches.md).

## The design

### What each of the three pins is worth

**Status** is the transport contract and the weakest of the three, because several
guards in one handler reach it.

**`Title`** is `nameof(TException)`. Every strategy sets it that way — 19 of them
across `src/Shared/Shared/Application/Exceptions/Handlers/Strategies/` (12) and the
Identity module (7):

```csharp
// src/Shared/Shared/Application/Exceptions/Handlers/Strategies/NotFoundExceptionHandler.cs:25-30
return CreateStandardProblemDetails(
    title: nameof(NotFoundException),
    detail: detail,
    statusCode: StatusCodes.Status404NotFound,
    context: context
);
```

Passing `TException` to the assertion is therefore a compile-time-checked way of
naming the expected title: rename the exception and the test stops building rather
than silently passing. The twentieth strategy, `DefaultExceptionHandler`, sets
`Title = exception.GetType().Name` (`DefaultExceptionHandler.cs:21`), which is the
same rule applied to whatever unhandled type reached it.

What `Title` **can** do is separate two different exception types that share a
status. Four distinct exception types produce 403, and the suite already relies on
telling them apart by hand:

```csharp
// tests/Integration/.../PublicUpdateOwnProfile/V1/PublicUpdateOwnProfileEndpointV1Tests.cs:130-133
await response.ShouldBeProblem(HttpStatusCode.Forbidden);

ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
problem.Title.Should().Be(nameof(RefreshTokenExpiryException));
```

| Status | Titles that produce it |
| --- | --- |
| 400 | `BadRequestException`, `InvalidFormatException`, `ValidationException` |
| 401 | `AuthenticationException`, `AccessTokenExpiryException` |
| 403 | `AuthorizationException`, `AccessDeniedException`, `AccountNotVerifiedException`, `RefreshTokenExpiryException` |
| 404 | `NotFoundException`, `ResourceNotFoundException` |
| 405 | `MethodNotAllowedException` |
| 409 | `ConflictException` |
| 410 | `OtpExpirationException` |
| 423 | `AccountInactiveException` |
| 429 | `RateLimitExceededException`, `OtpAttemptsLimitException` |
| 500 | `InternalServerException`, plus whatever `DefaultExceptionHandler` catches |
| 502 | `BadGatewayException` |

What `Title` **cannot** do is separate two guards inside one handler. Both a missing
article and a missing comment are `NotFoundException`; both `LikeNotFound()` and
`NotCommentOwner()` are `BadRequestException`
(`src/Modules/Content/Content/Application/Shared/Errors/ArticleInteractionErrors.cs:28, 60`).
Title narrows the set; it does not pick a member of it.

**`Detail`** is what discriminates guards. It is the localized message the handler
raised, and it is asserted with exact equality:

```csharp
// tests/Integration/Common/Extensions/HttpResponseExtensions.cs:113-115
problem
    .Detail.Should()
    .Be(detail, "the detail is what tells two guards behind the same status code apart");
```

### Resolving the expected detail

Never hardcode the sentence. `BaseApiTest.Localized<TMessage>` resolves the
application's own message class out of the test host's container and invokes it under
an explicit culture, so the expectation is produced by the same code path the response
was:

```csharp
// tests/Integration/Common/Base/BaseApiTest.cs:77-81
protected string Localized<TMessage>(
    Func<TMessage, string> select,
    string culture = LocalizedMessage.DefaultCulture
)
    where TMessage : notnull => LocalizedMessage.Resolve(Api.Services, select, culture);
```

```csharp
// tests/Integration/Common/Helpers/LocalizedMessage.cs:36-43
public static string Resolve<TMessage>(IServiceProvider services, Func<TMessage, string> select, string culture)
    where TMessage : notnull
{
    using var cultureScope = new CultureScope(culture);
    using IServiceScope scope = services.CreateScope();

    return select(scope.ServiceProvider.GetRequiredService<TMessage>());
}
```

The scope stays open across the *invocation*, not merely across the resolve, because
the message classes read the ambient UI culture at call time. The message classes are
scoped registrations (`ContentModule.cs:81-93` and the equivalents in the other
modules), which is why a scope is needed at all.

This is not the self-comparison defect that spec 06 deletes. There, both sides of the
assertion resolved the same key through the same localizer, so the assertion held
against an empty `.resx`. Here the expected value is resolved through the localizer
and the actual value arrives over HTTP from the exception middleware — the assertion
proves that the pipeline selected the right message *and* the right culture for the
request, and it fails when the handler picks a different guard.

### The culture the request actually selects is `fr`

This was undocumented anywhere before this spec and it is the easiest way to get a
converted assertion wrong.

```csharp
// src/Shared/Shared/Application/Extensions/LocalizationExtension.cs:17-22
private static readonly string[] SupportedCultures = ["fr", "en"];

private const string DefaultCulture = "fr";
```

```csharp
// src/Shared/Shared/Application/Extensions/LocalizationExtension.cs:41
options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
```

`AcceptLanguageHeaderRequestCultureProvider` is the *only* provider, so a request that
sends no `Accept-Language` header falls through to the default culture, which is `fr`.
Verified against a running host for a missing article:

| Request | `Detail` |
| --- | --- |
| no `Accept-Language` header | `Impossible de trouver l'article demandé.` |
| `Accept-Language: en` | `Could not find the requested article.` |

Both come from the same two resource entries — `EntityNotFound` is
`Impossible de trouver {0}.` / `Could not find {0}.` (`SharedExceptionMessage.fr.resx:19`,
`SharedExceptionMessage.en.resx:19`) and `Entity_Article` is `l'article demandé` /
`the requested article` (`:43` in each).

`BaseApiTest.Client` sends no `Accept-Language` header, so **the default expectation
is `fr`** and `Localized<T>` defaults to `LocalizedMessage.DefaultCulture`, which is
`"fr"` (`LocalizedMessage.cs:18`). Thirteen existing assertions expect English prose,
and every one of them is in a file that explicitly sets the header first — for example
`PublicVerifyOtpEndpointV1Tests.cs:81` before the assertion at `:103`,
`AdminLoginEndpointV1Tests.cs:116` before `:125`. Those convert with the culture
argument:

```csharp
await response.ShouldBeProblem<AuthenticationException>(
    HttpStatusCode.Unauthorized,
    Localized<AuthenticationErrorMessage>(m => m.InvalidCredentials(), LocalizedMessage.EnglishCulture)
);
```

**Resolve the expected detail in the culture that test's request actually selects.**
Getting it wrong does not fail loudly at the call site; it fails as a confusing string
mismatch, or — worse, if both cultures happen to agree — passes while proving nothing
about localization.

### Where `Detail` stops discriminating, and why that is a `src/` gap

`SharedExceptionMessage.EntityNotFound` maps the entity name to a friendly label and
falls back when there is none:

```csharp
// src/Shared/Shared/Application/Exceptions/Messages/SharedExceptionMessage.cs:45-49
private string ResolveEntityLabel(string entityName)
{
    LocalizedString label = localizer[$"Entity_{entityName}"];
    return label.ResourceNotFound ? localizer["Entity_Default"] : label.Value;
}
```

`Entity_Default` is `the requested resource` / `la ressource demandée`
(`SharedExceptionMessage.resx:22`). Twenty-six `Entity_*` labels are defined; seven
entity names raised by the error factories have none, and all seven therefore produce
the *identical* detail: **`Album`, `Artist`, `ArtistSocialLink`, `LyricsSubmission`,
`Lyrics revision`, `Translation`, `Translation revision`.**

For those seven a `Detail` assertion pins the status, the title and "some unlabelled
entity" — it cannot tell an `Album` 404 from an `Artist` 404. That is a client-facing
message defect before it is a testing one: a caller is told "the requested resource"
where every other entity gets a specific noun. File adding the seven labels as a
`src/` ticket; until it lands, tests touching those entities must carry a doc comment
saying the detail is the generic label and the guard is identified by the seeding
arrangement rather than by the assertion.

## Why not a machine-readable `code` extension

An earlier revision of this spec added `Extensions["code"]` to ProblemDetails, an
`ErrorCode` property to `BadRequestException` and `ConflictException`, and `nameof`
population in the error factories. **That change was implemented, rejected and fully
reverted.** `src/` is byte-identical to HEAD on all 29 files it touched;
`BaseExceptionStrategy.CreateStandardProblemDetails` writes `traceId` and `timestamp`
and nothing else (`BaseExceptionStrategy.cs:40-47`).

The substantive reason is that the code was consumed by a substring match, not an
equality one:

```csharp
// the rejected matcher, over detail + code
haystack.Should().Contain(errorCode);
```

Entity tokens in this codebase are routinely substrings of one another, so a
substring match silently accepts the wrong guard. Eight of the 27 entity tokens the
error factories raise — roughly a third — are contained in another:

| Token | Also matches |
| --- | --- |
| `Article` | `ArticleComment` |
| `Lyrics` | `LyricsSubmission`, `Lyrics revision` |
| `Video` | `ShortVideo` |
| `Artist` | `ArtistSocialLink` |
| `Category` | `CategoryPricing` |
| `Package` | `PackageSlot` |
| `ContentOrder` | `ContentOrderItem` |
| `Translation` | `Translation revision` |

`ShouldBeProblem(NotFound, "Article")` passed against a response whose error was
`ArticleComment`. That is the same class of defect the spec exists to remove — an
assertion that a different bug also satisfies — reintroduced inside the fix. The
`Detail` assertion uses `Be`, so it has no equivalent hole, and it needs no production
change at all.

Two secondary reasons, recorded so the decision is not relitigated: the extension was
a public response-contract change carried for the benefit of tests, and it created a
second source of truth for an identity the localized message already carries.

## Scope

In this spec:

- `ShouldBeProblem` gains `allowEmptyBody`, defaulting to `false`. **Landed.**
- A typed `ShouldBeProblem<TException>(status, detail)` overload, plus
  `LocalizedMessage` and `BaseApiTest.Localized<TMessage>`. **Landed.**
- All 482 remaining error assertions convert to the typed overload, and the
  `[Obsolete]` string shim is deleted.
- The unlike/unbookmark pair and the two commerce payment tests are renamed to what
  they prove, strengthened, and the branch `VerifyPayment` never covers gets a test.
- The eight `BeOneOf` status assertions become exact expectations. **Landed.**

Not in this spec:

- The 731 type-only throw assertions in `tests/Unit/`. `.WithMessage` on reused
  exception types is spec 07's mock-and-assertion discipline work.
- Whether `AdminRejectPaymentHandler` and `AdminAttachPaymentProofHandler` should look
  the order up before the payment. Change 4 renames the tests to the truth and raises
  the question; the `src/` decision is a separate ticket.
- Adding the seven missing `Entity_*` labels. Recorded above as a `src/` ticket.
- Localizing the three `ForceUnpromote` guards that throw raw English literals. That is
  [13-production-defects.md](13-production-defects.md) Change 4. Until it lands, those
  three 400s are asserted with the English literal, because that is what the endpoint
  returns in every culture.
- Proving the header-absent default culture end to end. Every localized error assertion
  this spec writes depends on it, but the test that pins it belongs with the rest of the
  localization work — [06-localization-testing.md](06-localization-testing.md) Change 4.

## Prerequisites

[01-test-host-fidelity.md](01-test-host-fidelity.md) must land first. Deleting
`OverrideJwtAuthentication` changes which component produces 401 responses, and this
spec should not be measuring error bodies against a host that is still diverging from
production.

Within the spec, the numbering is the order. Changes 1, 2 and 5 have landed; Change 3
is the sweep and Change 4 is what the sweep exposes.

## Current state, measured

Across `tests/Integration/`, excluding the helper's own definition:

| Call shape | Count |
| --- | --- |
| Total `ShouldBeProblem` calls | 483 |
| Files containing one | 228 |
| Status only (bare) | 300 |
| Bare plus `allowEmptyBody: true` | 1 |
| String argument to the `[Obsolete]` shim | 182 |
| Typed `ShouldBeProblem<TException>` | 0 |

Bare calls by status:

| Status | Bare | In the sweep? |
| --- | --- | --- |
| `NotFound` (404) | 122 | Yes |
| `BadRequest` (400) | 110 | Yes |
| `Conflict` (409) | 46 | Yes |
| `Forbidden` (403) | 11 | Yes — four exception types share this status |
| `TooManyRequests` (429) | 4 | Yes — two types share it |
| `Gone` (410) | 3 | Yes |
| `Unauthorized` (401) | 2 | Yes — two types share it |
| `BadGateway` (502) | 1 | Yes |

The earlier revision excluded 401 and 403 on the grounds that each carries one meaning
per endpoint. Under a `Title`-bearing assertion that no longer holds: 403 is reachable
from four distinct exception types and 401 from two, and telling them apart is exactly
what `ShouldBeProblem<TException>` is for. Every bare call converts.

The 182 string arguments split two ways, and the larger half is broken today:

- **169 pass an entity name or a resource key** — `"Lyrics"`, `"Article"`,
  `"SlugAlreadyExists"`, `"InvalidStatusTransition"`. They were written against the
  reverted `code` extension. With `code` gone the shim matches the detail only, and a
  French 404 detail (`Impossible de trouver les paroles demandées.`) does not contain
  the token `Lyrics`. These sites do not merely need improving; they need converting.
- **13 pass English prose**, every one of them in a file that sets
  `Accept-Language: en` first. They are the strongest error assertions in the suite
  today and convert to `Localized<T>(..., LocalizedMessage.EnglishCulture)`.

## Changes

### 1. Make empty-body tolerance opt-in — landed

File: `tests/Integration/Common/Extensions/HttpResponseExtensions.cs`.

```csharp
// tests/Integration/Common/Extensions/HttpResponseExtensions.cs:52-69
response.StatusCode.Should().Be(status);

string raw = await response.Content.ReadAsStringAsync();

if (string.IsNullOrWhiteSpace(raw))
{
    allowEmptyBody
        .Should()
        .BeTrue(
            "every error this application raises is translated to ProblemDetails by the "
                + "global exception middleware; an empty body means that did not happen"
        );
    return;
}

ProblemDetails problem = ParseProblem(raw, status);
problem.Title.Should().NotBeNullOrWhiteSpace();
```

Correcting one premise in the backing document: this application **does** emit a
ProblemDetails body for 401 and 403. `ConfigureJwtBearerEvents`
(`src/Modules/Identity/Identity/Application/Shared/Authorizations/Extensions/AuthorizationExtensions.cs:142-176`)
calls `context.HandleResponse()` on challenge, builds problem details through
`AccessTokenExpiryExceptionHandler` and `AuthorizationExceptionHandler`, and writes
them with `application/problem+json`.

**Outcome.** Exactly one call site legitimately needs the flag:
`AdminUploadArticleImageEndpointV1Tests.cs:118`, a multipart model-binding failure the
framework produces before the exception middleware runs. `grep -rn "allowEmptyBody"
tests/Integration` returning that one line, plus the helper, is the invariant.

**If this is done wrong** — if `allowEmptyBody: true` is added to a call site to make a
failure go away — the suite has re-created the hole this change closes, at exactly the
place that was about to tell it something. A failing empty-body assertion is a response
that stopped being ProblemDetails; investigate the endpoint first.

### 2. Add the typed overload and the localized-message resolver — landed

Files: `tests/Integration/Common/Extensions/HttpResponseExtensions.cs`,
`tests/Integration/Common/Helpers/LocalizedMessage.cs`,
`tests/Integration/Common/Base/BaseApiTest.cs`.

```csharp
// tests/Integration/Common/Extensions/HttpResponseExtensions.cs:88-116
public static async Task ShouldBeProblem<TException>(
    this HttpResponseMessage response,
    HttpStatusCode status,
    string detail
)
    where TException : Exception
{
    response.StatusCode.Should().Be(status);

    string raw = await response.Content.ReadAsStringAsync();
    raw.Should()
        .NotBeNullOrWhiteSpace(
            "a {0} is translated to a ProblemDetails body by the global exception middleware",
            typeof(TException).Name
        );

    ProblemDetails problem = ParseProblem(raw, status);

    problem
        .Title.Should()
        .Be(
            typeof(TException).Name,
            "every exception strategy titles the problem after the exception type it handles"
        );

    problem
        .Detail.Should()
        .Be(detail, "the detail is what tells two guards behind the same status code apart");
}
```

Both overloads share `ParseProblem`, which deserializes the body and asserts the
envelope agrees with the transport status:

```csharp
// tests/Integration/Common/Extensions/HttpResponseExtensions.cs:147-154
private static ProblemDetails ParseProblem(string raw, HttpStatusCode status)
{
    ProblemDetails? problem = JsonSerializer.Deserialize<ProblemDetails>(raw, JsonOptions);
    problem.Should().NotBeNull("a non-empty error response should be a ProblemDetails body");
    problem!.Status.Should().Be((int)status);

    return problem;
}
```

A third overload exists at `:126-138`, taking a `string errorCode` and matching it with
`Contain`. It is `[Obsolete]` and it is **not part of the design** — it is a migration
shim that keeps the 182 unconverted call sites compiling. Change 3 ends with its
deletion. Do not add a call to it.

**If this is done wrong** — if a test hardcodes the expected sentence instead of calling
`Localized<T>` — the assertion is coupled to the `.resx` copy and to one culture, which
is what the 13 prose sites already demonstrate.

### 3. Convert every error assertion to the typed overload

Files: across `tests/Integration/`, 482 call sites in 228 files.

Every call site requires reading the handler to answer one question: *which guard is
this test's name claiming?* Work module by module, not file by file, so the handler is
open while its tests are being updated.

**Deriving the expected detail, per shape:**

- **404 with an entity name.** Most `NotFoundException`s carry one, and
  `NotFoundExceptionHandler.cs:17-23` replaces the exception message with the localized
  label:

  ```csharp
  await response.ShouldBeProblem<NotFoundException>(
      HttpStatusCode.NotFound,
      Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
  );
  ```

  The token is the one the factory passes —
  `new NotFoundException("Article", "id", keyValue: id)`
  (`src/Modules/Content/Content/Application/Shared/Errors/ArticleErrors.cs:22`) — or,
  on the repository path, `typeof(T).Name` with the `Entity` suffix stripped
  (`src/Shared/Shared/Infrastructure/Extensions/DbSetExtension.cs:200-207`).

- **404 without an entity name.** Six factories use the message-only constructor, so
  `EntityName` stays null, the `if` in `NotFoundExceptionHandler.cs:19` is skipped, and
  the exception's own localized message becomes the detail. Resolve that message class
  directly:

  ```csharp
  await response.ShouldBeProblem<NotFoundException>(
      HttpStatusCode.NotFound,
      Localized<PlaylistErrorMessage>(m => m.NotFound(playlist.Id))
  );
  ```

  The six are `UserErrors.NoValidOtpFound` (`UserErrors.cs:310`),
  `StreamingLinkErrors.NothingResolved` (`:41`),
  `CategoryErrors.NoExclusiveCategoryFound` (`:137`), `PlaylistErrors.NotFound` (`:21`),
  `NotificationErrors` (`:24`) and `NewsletterErrors` (`:23`). Under the rejected `code`
  design these were a gap — no `EntityName` meant no code, so those six 404s could not be
  discriminated at all. Under this design they need nothing: the detail is asserted
  directly, and their messages are already localized. **Resolved by design, not
  outstanding.**

- **400 and 409.** Resolve the message method the factory calls:

  ```csharp
  await response.ShouldBeProblem<BadRequestException>(
      HttpStatusCode.BadRequest,
      Localized<ArticleInteractionErrorMessage>(m => m.LikeNotFound())
  );
  ```

- **Any test that sets `Accept-Language: en`.** Pass
  `LocalizedMessage.EnglishCulture` as the second argument to `Localized<T>`. There are
  13 such assertions today; a converted test that adds the header later must update its
  expectation at the same time.

The 169 token-argument sites are the natural first tranche: they already name the guard
their author intended, so converting them is mechanical, and they are failing now.

**Budget time for what this exposes.** The five wrong-reason passes in Change 4 were
found by asking "which guard is this name claiming?" of a handful of tests. Asking it of
482 will find more. Every one that surfaces is a test whose name asserts a branch it
never reaches, and each needs a rename plus, usually, a new test for the branch that
turns out to be uncovered. Treat that as the deliverable rather than as scope creep — it
is the reason the change is worth doing.

Delete the `[Obsolete]` shim in the same PR as the last converted module, so the
compiler prevents a regression.

**If this is done wrong** — if an expectation is copied from the observed response body
rather than derived from the handler — the assertion locks in current behaviour instead
of intended behaviour, and a test that is passing for the wrong reason keeps passing with
a stronger assertion attached to it.

### 4. Fix the tests that assert a status their branch cannot produce

Files: `tests/Integration/.../UnlikeArticle/V1/PublicUnlikeArticleEndpointV1Tests.cs`,
`.../UnbookmarkArticle/V1/PublicUnbookmarkArticleEndpointV1Tests.cs`,
`.../RejectPayment/V1/AdminRejectPaymentEndpointV1Tests.cs`,
`.../AttachPaymentProof/V1/AdminAttachPaymentProofEndpointV1Tests.cs`,
`.../VerifyPayment/V1/AdminVerifyPaymentEndpointV1Tests.cs`.

**a. The unlike/unbookmark pair.**
`UnlikeArticle_AsVisitor_NonExistentLike_ReturnsNotFound` passes `Guid.NewGuid()` as the
article id. The handler looks the article up first
(`src/Modules/Content/Content/Application/Interactions/UseCases/Public/Commands/UnlikeArticle/PublicUnlikeArticleHandler.cs:27`),
so the 404 comes from `GetByIdOrThrowAsync`. The branch the name refers to is
`throw i18n.ArticleInteraction.LikeNotFound()`, and `LikeNotFound()` returns a
`BadRequestException` (`ArticleInteractionErrors.cs:28-31`). The named branch is
structurally incapable of returning 404.

```csharp
// after — the test renamed to the branch it actually reaches
[Fact]
public async Task UnlikeArticle_AsVisitor_NonExistentArticle_ReturnsNotFound()
{
    Client.AuthenticateAsVisitor();

    var response = await Client.DeleteAsync(Routes.Public.Articles.Likes(Guid.NewGuid()));

    await response.ShouldBeProblem<NotFoundException>(
        HttpStatusCode.NotFound,
        Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
    );
}
```

**Deviation from the backing document, recorded here.** That document states the
like-not-found branch "has no integration coverage at all". It does:
`UnlikeArticle_WhenNotLiked_ReturnsBadRequest` seeds an article, deletes the like that
was never created, and asserts 400. So no new test is needed — the existing one is
strengthened instead:

```csharp
// after
[Fact]
public async Task UnlikeArticle_WhenNotLiked_ReturnsBadRequest()
{
    ArticleEntity article = await SeedArticleAsync();
    Client.AuthenticateAsVisitor();

    var response = await Client.DeleteAsync(Routes.Public.Articles.Likes(article.Id));

    await response.ShouldBeProblem<BadRequestException>(
        HttpStatusCode.BadRequest,
        Localized<ArticleInteractionErrorMessage>(m => m.LikeNotFound())
    );
}
```

`PublicUnbookmarkArticleEndpointV1Tests.cs` is the same shape throughout —
`UnbookmarkArticle_AsVisitor_NonExistentBookmark_ReturnsNotFound` renames to
`..._NonExistentArticle_ReturnsNotFound`, and
`UnbookmarkArticle_WhenNotBookmarked_ReturnsBadRequest` takes
`m => m.BookmarkNotFound()`. `BookmarkNotFound()` is a `BadRequestException` too
(`ArticleInteractionErrors.cs:44-47`).

**b. The two commerce payment tests.** `AdminRejectPaymentHandler` parses the order id
and immediately calls `orderPaymentFactory.GetByOrderIdOrThrowAsync`
(`AdminRejectPaymentHandler.cs:30-35`); `AdminAttachPaymentProofHandler` does the same at
`:37-42`. Neither looks the order up. The 404 both tests observe comes from
`OrderPaymentFactory.GetByOrderIdOrThrowAsync` throwing
`contentOrderErrors.PaymentNotFound(orderId)`
(`src/Modules/Content/Content/Application/Commerce/Factories/OrderPaymentFactory.cs:25`),
which is `new NotFoundException("ContentPayment", "orderId", keyValue: orderId)`
(`ContentOrderErrors.cs:46`) — a missing **payment**, not a missing **order**.

```csharp
// tests/Integration/.../RejectPayment/V1/AdminRejectPaymentEndpointV1Tests.cs — after
/// <summary>
/// The handler resolves the payment by order id and never loads the order, so an unknown
/// id produces a missing-payment 404. The name says so; whether the handler ought to
/// verify the order exists first is a question for the handler, not for this test.
/// </summary>
[Fact]
public async Task RejectPayment_NonExistentPayment_ReturnsNotFound()
{
    Client.AuthenticateAsSuperAdmin();
    var request = new { Notes = "Invalid payment" };
    var msg = new HttpRequestMessage(HttpMethod.Patch, Routes.Admin.Orders.RejectPayment(Guid.NewGuid()))
    {
        Content = JsonContent.Create(request),
    };

    var response = await Client.SendAsync(msg);

    await response.ShouldBeProblem<NotFoundException>(
        HttpStatusCode.NotFound,
        Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentPayment"))
    );
}
```

`AttachPaymentProof_NonExistentOrder_ReturnsNotFound` renames the same way.

Raise the missing order lookup as a `src/` ticket rather than fixing it here: rejecting
or attaching proof to a payment without confirming the order exists is the same shape as
the parent-scoping defect recorded in
[../90-remediation-plan.md](../90-remediation-plan.md).

**c. The third payment handler is honest, and it has a genuinely uncovered branch.**
`AdminVerifyPaymentHandler` loads the order first and throws
`i18n.ContentOrder.NotFound(id: orderId)` when it is absent
(`AdminVerifyPaymentHandler.cs:30-56`), so `VerifyPayment_NonExistentOrder_ReturnsNotFound`
keeps its name and takes `EntityNotFound("ContentOrder")`. What it does not have is a
test for the inner lookup: when the order exists and no payment row does, the handler
reaches `GetByOrderIdOrThrowAsync` inside the `order is not null` branch and produces a
`ContentPayment` 404. That branch has no test today.

```csharp
// tests/Integration/.../VerifyPayment/V1/AdminVerifyPaymentEndpointV1Tests.cs — new
/// <summary>
/// An order with no payment row reaches the inner payment lookup, which is a different
/// guard from the order lookup above it and produces a different localized detail.
/// Without this test the two 404s are indistinguishable and either guard could be
/// deleted with the suite staying green.
/// </summary>
[Fact]
public async Task VerifyPayment_WhenTheOrderHasNoPayment_ReturnsNotFound()
{
    ContentOrderEntity order = await SeedOrderWithoutPaymentAsync();
    Client.AuthenticateAsSuperAdmin();

    var response = await Client.PatchAsync(Routes.Admin.Orders.VerifyPayment(order.Id), content: null);

    await response.ShouldBeProblem<NotFoundException>(
        HttpStatusCode.NotFound,
        Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentPayment"))
    );
}
```

Build `SeedOrderWithoutPaymentAsync` from the seeding helper the file already uses for
`VerifyPayment_AsSuperAdmin_ReturnsOk`, minus the payment row, and match the real request
shape of the endpoint rather than the sketch above.

**If this is done wrong** — if a test is renamed without converting the assertion — the
name is now accurate and the assertion still cannot tell the two guards apart, so the
next refactor can swap them silently.

### 5. Replace the `BeOneOf` status assertions with exact expectations — landed

Files: eight call sites.

| Site | Was | Now |
| --- | --- | --- |
| `Shared/Exceptions/Handlers/ExceptionHandlerTests.cs:110` | `BeOneOf(MethodNotAllowed, NotFound)` | `Be(MethodNotAllowed)` |
| `Shared/Infrastructure/Middleware/ResourceNotFoundMiddlewareTests.cs` | `BeOneOf(Unauthorized, NotFound)` | `Be(Unauthorized)` — the test name already said `ShouldReturn401_WhenUnauthenticated` |
| `Shared/Infrastructure/Interceptors/AuditableEntityInterceptorTests.cs` | `BeOneOf(OK, Created)` | `Be(Created)` — the endpoint returns `Results.Created` |
| `.../CreatePermission/V1/AdminCreatePermissionEndpointV1Tests.cs` | `BeOneOf(OK, Created)` | `Be(Created)` |
| `.../Admin/Commands/UpdateOwnProfile/V1/AdminUpdateOwnProfileEndpointV1Tests.cs` | `BeOneOf(Forbidden, BadRequest)` | the contract decided and asserted exactly |
| `.../Public/Commands/UpdateOwnProfile/V1/PublicUpdateOwnProfileEndpointV1Tests.cs:130` | `BeOneOf(Forbidden, BadRequest)` | `Be(Forbidden)` plus `Title == nameof(RefreshTokenExpiryException)` |
| `.../GetCustomerOrders/V1/AdminGetCustomerOrdersEndpointV1Tests.cs` | `BeOneOf(OK, NotFound)` plus an `if` around the body assertion | `Be(OK)` and the empty page asserted unconditionally |
| `.../UploadArticleImage/V1/AdminUploadArticleImageEndpointV1Tests.cs` | `BeOneOf(BadRequest, NotFound, UnprocessableEntity)` | `NotFound` for an unknown article; the multipart binding failure is a separate test taking `allowEmptyBody: true` |

`grep -rn "BeOneOf" tests/` now returns nothing.

The `PublicUpdateOwnProfile` case is worth keeping in view during Change 3: it asserts
the title by hand because the typed overload did not exist when it was written. It
converts to `ShouldBeProblem<RefreshTokenExpiryException>` and loses the manual
`ReadAsAsync<ProblemDetails>` round trip.

**If this is done wrong** — if a narrowed assertion fails and gets widened again — the
test has found a real routing or contract defect and the fix belongs in `src/`. Widening
it back restores an assertion that carries no information.

## Expected fallout

**Change 3 starts from a red suite, not a green one.** The 169 token-argument sites are
already failing: they were written against the reverted `code` extension and the shim now
matches the localized detail only. Convert them first and the suite gets greener as the
sweep progresses, which is the opposite of the usual shape and worth saying out loud in
the PR.

**Change 3 will surface more wrong-reason tests than Change 4 lists.** The five known
cases were found by asking "which guard does this name claim?" of a handful of files;
asking it 482 times will find more of the same shape — most likely wherever a handler
looks up a parent entity before checking the child, which is the pattern behind all five.
Expect roughly one rename per handler that has more than two 404 paths, and budget for
the new tests those renames reveal are missing.

**No production response shape changes.** This is the difference from the rejected
revision. `traceId` and `timestamp` remain the only extensions, and no client sees a new
member.

**The seven unlabelled entities produce weaker assertions than the rest.** Tests touching
`Album`, `Artist`, `ArtistSocialLink`, `LyricsSubmission`, `Lyrics revision`,
`Translation` and `Translation revision` pin the generic `Entity_Default` sentence. They
are still stronger than a bare status, and they get stronger for free once the labels are
added in `src/`.

**Test counts change.** Change 4 adds one test to `AdminVerifyPaymentEndpointV1Tests` and
renames four. Renames are not deletions — verify by name that nothing disappeared.

## Testing

```bash
dotnet build
dotnet test tests/Integration
dotnet test tests/Unit
```

Invariants to check after the sweep:

```bash
# The migration shim is gone, and with it every call that used it.
grep -rn "Obsolete" tests/Integration/Common/Extensions/HttpResponseExtensions.cs

# No status assertion accepts two answers.
grep -rn "BeOneOf" tests/

# Every allowEmptyBody use is deliberate; there is exactly one, and it is multipart.
grep -rn "allowEmptyBody" tests/Integration

# No expected detail is hardcoded prose.
grep -rn 'ShouldBeProblem<[A-Za-z]*>(\s*HttpStatusCode\.[A-Za-z]*,\s*"' tests/Integration
```

New evidence the changes worked:

- Delete the `if (!hasLiked) throw i18n.ArticleInteraction.LikeNotFound();` guard at
  `PublicUnlikeArticleHandler.cs:35-38` locally. `UnlikeArticle_WhenNotLiked_ReturnsBadRequest`
  must fail. Revert. Before this spec that deletion left the suite green.
- Swap the order of the two lookups in `AdminVerifyPaymentHandler` locally so the payment
  is fetched first. `VerifyPayment_NonExistentOrder_ReturnsNotFound` must fail on the
  detail. Revert.
- Change `SharedExceptionMessage`'s `Entity_Article` value in the `fr` catalogue only.
  Every converted 404 test for articles that sends no `Accept-Language` header must still
  pass, because the expectation is resolved through the same catalogue — and the
  equivalent test that sends `Accept-Language: en` must also still pass. A test that
  fails is one that hardcoded the sentence instead of calling `Localized<T>`.
- Add `Accept-Language: en` to a converted test that does not set it, without changing
  its expectation. It must fail. That is the proof the assertion is culture-sensitive,
  and it is the first such proof the suite has ever had.

## Risks

**482 edited call sites is a large diff with a small per-site decision.** Mitigation:
split by module, one PR each, and require that the reviewer of each PR can name the guard
for every detail added. A sweep done without reading handlers produces assertions that
lock in current behaviour, which is worse than no assertion because it looks like
coverage.

**Culture drift between a request and its expectation.** A test that sets
`Accept-Language: en` and resolves its expectation in the default `fr` fails with a
confusing string diff; the reverse combination can pass by accident where the two
catalogues agree. Mitigation: the culture argument to `Localized<T>` and the header on the
request are set in the same edit, and a test that sets no header never passes a culture.

**Exact-equality assertions are copy-edit sensitive.** A `.resx` reword breaks nothing,
because both sides resolve through the catalogue — but a test that hardcodes the sentence
does break, which is the intended pressure. Mitigation: the grep above.

**The `Entity_Default` collapse could be mistaken for a passing discrimination.** Seven
entities share one detail. Mitigation: the doc comment required above, and the `src/`
ticket that removes the ambiguity.

## Checklist

- [x] 1 — `ShouldBeProblem` takes `allowEmptyBody`, defaulting to `false`, with a doc
      comment naming the only legitimate case
- [x] 1 — The single remaining `allowEmptyBody: true` is a framework model-binding
      failure and says so
- [x] 2 — `ShouldBeProblem<TException>(status, detail)` pins status, `Title` and exact
      `Detail`; both overloads share `ParseProblem`
- [x] 2 — `LocalizedMessage` and `BaseApiTest.Localized<TMessage>` resolve an expected
      detail from the host container under an explicit culture, defaulting to `fr`
- [x] 3 — All call sites converted to `ShouldBeProblem<TException>` — 493 by the
      audit's count, 501 as landed, since specs 12 and 13 added endpoint tests mid-sweep
- [x] 3 — The prose assertions resolve through `Localized<T>` with
      `LocalizedMessage.EnglishCulture`, and their tests still set `Accept-Language: en`
- [x] 3 — The `[Obsolete]` string overload deleted from `HttpResponseExtensions`
- [x] 4 — Unlike and unbookmark renamed to `NonExistentArticle`; the not-liked and
      not-bookmarked tests assert their `BadRequestException` details
- [x] 4 — `RejectPayment` and `AttachPaymentProof` renamed to say what is missing; the
      missing order lookup raised as a `src/` ticket
- [x] 4 — `VerifyPayment_NonExistentOrder_ReturnsNotFound` asserts the `ContentOrder`
      detail, and the order-with-no-payment test added
- [x] 5 — All eight `BeOneOf` status assertions replaced with exact expectations, and the
      conditional body assertion in `AdminGetCustomerOrdersEndpointV1Tests` made
      unconditional
- [ ] Ticket filed for the missing `Entity_*` labels — the count is ~9, not seven, and
      no ticket is filed; carried to the open follow-ups in
      [../90-remediation-plan.md](../90-remediation-plan.md)
- [x] Full integration suite green; full unit suite green

## Implementation notes

Implemented 2026-08-23. This spec was implemented once the wrong way, reverted in full,
and implemented again. Both passes are recorded here because the failure mode is the
interesting part.

### The `errorCode` design was built, rejected and fully reverted

The first implementation added a machine-readable `code` extension to `ProblemDetails`
in `src/` and asserted against it. It was rejected in review and every line of it was
reverted; there is no `src/` change left from this spec.

The defect was not the extension itself but its consumer: the test-side helper matched
the code with a **substring** comparison. Roughly a third of this codebase's entity
tokens are substrings of another — `"Article"` is a substring of `"ArticleComment"`,
`"Package"` of `"PackageSlot"`, `"Order"` of `"ContentOrder"`, `"Payment"` of
`"PaymentProof"`. A test asserting the `"Article"` code passed against a response whose
error was `ArticleComment`, which is precisely the wrong-reason pass the spec exists to
eliminate. A substring match on a namespaced token is not discrimination; it is a
weaker assertion wearing a stronger name.

The landed design asserts three things the response already carries, all with exact
equality: the status code, the ProblemDetails `Title` (every strategy sets it to
`nameof(TException)`, so renaming the exception is a compile error at the call site),
and the exact localized `Detail` resolved through `BaseApiTest.Localized<TMessage>`
against the host container. No production change was needed for any of it.

### The host's default request culture is `fr`, not `en`

Verified against a running host, not inferred from configuration. `Localized<TMessage>`
therefore defaults to `fr`, and the handful of tests that assert English prose do so
**only because they send `Accept-Language: en`** — the header is load-bearing, not
decoration. Deleting it from one of those tests turns the assertion red against a
French detail, which is the correct behaviour and worth knowing before someone
"tidies" the header away.

### `NotFoundException.CleanEntityName` replaces, it does not trim

`CleanEntityName` is a case-insensitive `Replace("entity", "")` applied **anywhere in
the name**, not a suffix trim. `ArticleEntity` → `Article` is the happy path that hides
this. A future `IdentityUserEntity` would render as `IdUser`, because the `entity` in
`Identity` is replaced too. Any expected detail computed from a type name has to go
through the same function rather than a hand-written suffix strip, or the two will
disagree the first time an entity name contains the substring.

### Deviation from Change 4's naming

The spec proposed `NonExistentPayment` for the `RejectPayment` and `AttachPaymentProof`
tests. What landed is `WithOrderThatHasNoPayment_ReturnsPaymentNotFound`, because the
order does exist in those tests — it is the payment that is absent, and the shorter name
would have implied a bad payment id. The intent of Change 4, that a test name state
what is actually missing, is met.

### `allowEmptyBody: true` now has zero call sites

Change 1's checklist item records "the single remaining `allowEmptyBody: true`". By the
end of the sweep there were none: the bodiless-400 defect it tolerated was a production
defect, fixed across nine upload endpoints (see
[13-production-defects.md](13-production-defects.md)). The parameter is kept, defaulting
to `false`, as the documented escape hatch for framework model-binding failures.
