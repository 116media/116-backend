# Medium — The shared assertion helper silently downgrades

`ShouldBeProblem` is the suite's standard way of asserting an error response, used
467 times across 226 files. It returns early when the response body is empty, so on
that path it asserts nothing but the status code. It also takes an optional
`errorCode` argument that 455 of those 467 calls omit — and that omission is what
lets a test assert the right status for entirely the wrong reason. Two named test
branches are proven below to be unreachable by the code path their names describe.

## The problem

### The empty-body early return

```csharp
// tests/Integration/Common/Extensions/HttpResponseExtensions.cs:48-57
response.StatusCode.Should().Be(status);

// Some error responses (e.g. framework/multipart model-binding failures) carry no
// body. The status assertion above is the contract in that case; only validate the
// ProblemDetails shape when a body is actually present.
string raw = await response.Content.ReadAsStringAsync();
if (string.IsNullOrWhiteSpace(raw))
{
    return;
}
```

The reasoning is sound for the case it names — a framework model-binding failure
genuinely has no body — but it applies globally. The helper cannot tell the
difference between "this endpoint is not expected to produce ProblemDetails" and
"this endpoint stopped producing ProblemDetails."

### The unused `errorCode` parameter

```csharp
// the signature as measured during the audit
public static async Task ShouldBeProblem(
    this HttpResponseMessage response,
    HttpStatusCode status,
    string? errorCode = null
)
```

That parameter no longer exists as designed. It survives only as an `[Obsolete]`
migration shim (`HttpResponseExtensions.cs:126-138`) that keeps unconverted call
sites compiling until [spec 04](../specs/04-error-assertion-discipline.md) Change 3
deletes it. The analysis below is what the parameter's disuse proved, and it still
holds; the prescription in *The fix* has been rewritten to what was actually built.

Measured across `tests/Integration/`, excluding the definition:

| Call shape | Count |
| --- | --- |
| Total `ShouldBeProblem(` calls | 467 |
| Calls passing `errorCode` | 12 |
| Bare `ShouldBeProblem(HttpStatusCode.NotFound)` | 197 |
| Bare `ShouldBeProblem(HttpStatusCode.BadRequest)` | 160 |
| Bare `ShouldBeProblem(HttpStatusCode.Conflict)` | 76 |
| Bare, other statuses | 22 |

The 12 that do pass it — for example
`AdminRemoveOrderItemEndpointV1Tests.cs:76` asserting
`ShouldBeProblem(HttpStatusCode.NotFound, "Could not find the requested order item.")`
— are the strongest error assertions in the suite. They are also 2.6% of them.

### Two branches that cannot produce the status their test asserts

```csharp
// tests/Integration/Modules/Content/Application/Interactions/UseCases/Public/Commands/UnlikeArticle/V1/PublicUnlikeArticleEndpointV1Tests.cs:38-46
[Fact]
public async Task UnlikeArticle_AsVisitor_NonExistentLike_ReturnsNotFound()
{
    Client.AuthenticateAsVisitor();

    var response = await Client.DeleteAsync(Routes.Public.Articles.Likes(Guid.NewGuid()));

    await response.ShouldBeProblem(HttpStatusCode.NotFound);
}
```

The `Guid.NewGuid()` is a random article id, and the handler looks the article up
first:

```csharp
// src/Modules/Content/Content/Application/Interactions/UseCases/Public/Commands/UnlikeArticle/PublicUnlikeArticleHandler.cs:27-38
await articleRepository.GetByIdOrThrowAsync(id: command.ArticleId, cancellationToken: cancellationToken);

bool hasLiked = await articleRepository.HasLikedAsync(...);

if (!hasLiked)
{
    throw i18n.ArticleInteraction.LikeNotFound();
}
```

The 404 comes from `GetByIdOrThrowAsync` on line 27. The branch the test is named
after is line 37, and it does not produce a 404:

```csharp
// src/Modules/Content/Content/Application/Shared/Errors/ArticleInteractionErrors.cs:28-31
public BadRequestException LikeNotFound()
{
    return new BadRequestException(i18n.LikeNotFound());
}
```

`LikeNotFound()` is a `BadRequestException`. The named branch is **structurally
incapable** of returning 404, so a test asserting 404 can never reach it, and the
like-not-found path has no integration coverage at all. The article-not-found path,
which is what the test actually exercises, is covered by accident and under the
wrong name.

**The same defect in the unbookmark sibling.**
`PublicUnbookmarkArticleEndpointV1Tests.cs:38-46` is
`UnbookmarkArticle_AsVisitor_NonExistentBookmark_ReturnsNotFound`, structurally
identical, and `ArticleInteractionErrors.cs:44-47` shows `BookmarkNotFound()` is
also a `BadRequestException`.

**A weaker but related version in commerce.** Ten test methods are named
`*_NonExistentOrder_ReturnsNotFound`. Two of them get their 404 from a payment
lookup rather than an order lookup:

```csharp
// src/Modules/Content/Content/Application/Commerce/UseCases/Admin/Commands/RejectPayment/AdminRejectPaymentHandler.cs:30-35
Guid orderId = Guid.Parse(command.OrderId);

ContentPaymentEntity payment = await orderPaymentFactory.GetByOrderIdOrThrowAsync(
    orderId: orderId,
    ct: cancellationToken
);
```

There is no order lookup in `AdminRejectPaymentHandler` or in
`AdminAttachPaymentProofHandler` (`:37-42`). The 404 those tests observe is
`contentOrderErrors.PaymentNotFound(orderId)`
(`src/Modules/Content/Content/Application/Commerce/Factories/OrderPaymentFactory.cs:25`),
which resolves to `new NotFoundException("ContentPayment", "orderId", ...)`
(`ContentOrderErrors.cs:44-46`) — a missing **payment**, not a missing **order**.

For contrast, the third payment handler does check:
`AdminVerifyPaymentHandler.cs:32-55` loads the order and throws
`i18n.ContentOrder.NotFound(id: orderId)` when it is absent, so
`VerifyPayment_NonExistentOrder_ReturnsNotFound` is honest. The reviewer's original
claim that all three lacked an order lookup does not hold; two of three do.

Every one of these would have failed on the day it was written if the call had
asserted the detail the order lookup produces —
`EntityNotFound("ContentOrder")` — rather than the number 404.

### Throw assertions that check only the type

Across all of `tests/`:

| Assertion | Count |
| --- | --- |
| `Should().ThrowAsync<` | 512 |
| `Should().Throw<` | 139 |
| `Should().ThrowExactlyAsync<` | 46 |
| `Should().ThrowExactly<` | 34 |
| **Total throw assertions** | **731** |
| Of those, chaining `.WithMessage(` | 23 |

Asserting only the exception type is weak wherever a type is reused. `BadRequestException`
is thrown for `LikeNotFound`, `BookmarkNotFound`, `NotCommentOwner` and
`CannotReplyToReply` in a single error class
(`ArticleInteractionErrors.cs:28, 44, 60, 69`) — a test asserting
`ThrowAsync<BadRequestException>` passes if the handler throws any of the four.

### An assertion that accepts either answer

```csharp
// tests/Integration/Shared/Exceptions/Handlers/ExceptionHandlerTests.cs:105-111
[Fact]
public async Task MethodNotAllowedExceptionHandler_ShouldReturn405_ForWrongMethod()
{
    var response = await Client.DeleteAsync(Routes.Public.Auth.Login());

    response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
}
```

The name claims 405. The assertion accepts 404, which is what routing returns when
the method-not-allowed handler is absent. The test passes whether the handler under
test exists or not. `BeOneOf` appears eight times in the integration suite; this is
the clearest case, but `AuditableEntityInterceptorTests.cs:78`
(`BeOneOf(OK, Created)`) and
`AdminUploadArticleImageEndpointV1Tests.cs:80`
(`BeOneOf(BadRequest, NotFound, UnprocessableEntity)`) have the same shape.

### How widespread is "status only"? — the measurement

A common suspicion about a suite this size is that many tests assert an HTTP status
and nothing else. That is worth measuring rather than assuming, because the answer
determines whether the fix is "rewrite hundreds of tests" or "populate one
parameter". All 1,879 integration test methods were classified, resolving private
helper methods declared in the same file so that a test verifying state through a
helper is not miscounted as shallow.

**Success responses are well covered.** Of 566 tests asserting a 2xx status, only
11 assert nothing beyond the status line — and on inspection 6 of those are adapter
tests against loopback servers where the assertion is on the request the server
received, and 2 more assert content type and payload bytes. The genuine remainder
is roughly **5 tests out of 566**. The overwhelming majority verify persisted state
or the deserialized response body, frequently through a same-file helper such as
`GetVideoStatusAsync`.

**Error responses are where the gap is.** Of the 467 `ShouldBeProblem` calls,
**404 pass no `errorCode`**, so they assert only that *some* error of that status
occurred:

| Status asserted with no error identity | Count |
| --- | --- |
| `Unauthorized` (401) | 217 |
| `NotFound` (404) | 199 |
| `Forbidden` (403) | 167 |
| `BadRequest` (400) | 162 |
| `Conflict` (409) | 63 |

401 and 403 look at first like the cheap rows to skip, and this application does
not make that easy: it does not fall back to ASP.NET Core's bodiless challenge.
`ConfigureJwtBearerEvents`
(`src/Modules/Identity/Identity/Application/Shared/Authorizations/Extensions/AuthorizationExtensions.cs:142-176`)
handles `OnChallenge` and `OnForbidden` itself and writes a full ProblemDetails
payload for both. A body is available to assert against.

The audit's first reading was that each of those two carries **one meaning per
endpoint** and could be left bare. That does not survive contact with the exception
strategies: four distinct exception types produce 403 —
`AuthorizationException`, `AccessDeniedException`, `AccountNotVerifiedException`
and `RefreshTokenExpiryException` — and two produce 401. Telling them apart is
precisely what a `Title`-bearing assertion does, and one test in the suite already
does it by hand (`PublicUpdateOwnProfileEndpointV1Tests.cs:130-133`). Under
[spec 04](../specs/04-error-assertion-discipline.md) every bare call converts,
whatever its status.

The **404, 400 and 409** rows remain the largest part of the gap, because each of
those statuses is additionally reachable from several distinct guards in the *same*
handler, where the title is identical and only the detail differs. Asserting the
number alone cannot distinguish "the branch I am testing fired" from "an earlier,
unrelated branch fired first."

That is not a theoretical concern — it is exactly the mechanism behind the
wrong-reason passes documented in the next section, where a test named for a
missing *like* is in fact proving a missing *article*.

## Why it matters

`ShouldBeProblem` is the single most-used assertion in the integration suite. A
regression that stopped the global exception middleware emitting ProblemDetails
bodies — a serializer misconfiguration, a middleware ordering change, a handler
returning `Results.StatusCode(404)` instead of throwing — would leave all 467 calls
green. The suite would report full health for an API that had stopped explaining its
errors to clients at all.

The wrong-reason passes are the more insidious half. A test named
`NonExistentLike_ReturnsNotFound` is a claim that the like-not-found branch is
covered. Anyone reading the file, or a coverage report, or the test list in a PR,
will believe it. Meanwhile that branch has never executed under test, and if
`LikeNotFound()` were deleted outright the suite would stay green.

This is the mechanism by which a suite with high file-level coverage accumulates
uncovered *branches* while reporting the opposite. The optional parameter is not a
convenience that some tests happen to skip — it is the only thing standing between
"asserted the outcome" and "asserted a number that several different bugs also
produce."

## The fix

### Make empty-body tolerance opt-in

```csharp
// tests/Integration/Common/Extensions/HttpResponseExtensions.cs — before
string raw = await response.Content.ReadAsStringAsync();
if (string.IsNullOrWhiteSpace(raw))
{
    return;
}

// after
string raw = await response.Content.ReadAsStringAsync();
if (string.IsNullOrWhiteSpace(raw))
{
    allowEmptyBody
        .Should()
        .BeTrue(
            "every error this application raises is translated to ProblemDetails by the global "
                + "exception middleware; an empty body means that translation did not happen"
        );
    return;
}
```

with the flag added to the signature:

```csharp
public static async Task ShouldBeProblem(
    this HttpResponseMessage response,
    HttpStatusCode status,
    bool allowEmptyBody = false
)
```

The handful of tests that genuinely assert a framework model-binding failure pass
`allowEmptyBody: true`, which documents the exception at the site that needs it. All
other calls gain a real assertion for free. Expect a small number of failures on
first run — each one is a response that was silently not a ProblemDetails.

**Outcome, now that it has landed:** exactly one call site needs the flag —
`AdminUploadArticleImageEndpointV1Tests.cs:118`, a multipart model-binding failure.
Every other error response in the suite carries a ProblemDetails body.

### Pin the reason, not just the number

Once the empty-body hole is closed, tighten the reason. The strong form asserts three
things — status, the ProblemDetails `Title`, and the exact localized `Detail`:

```csharp
// tests/Integration/.../UnlikeArticle/V1/PublicUnlikeArticleEndpointV1Tests.cs — before
[Fact]
public async Task UnlikeArticle_AsVisitor_NonExistentLike_ReturnsNotFound()
{
    Client.AuthenticateAsVisitor();

    var response = await Client.DeleteAsync(Routes.Public.Articles.Likes(Guid.NewGuid()));

    await response.ShouldBeProblem(HttpStatusCode.NotFound);
}

// after — two tests, each reaching the branch its name claims
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

[Fact]
public async Task UnlikeArticle_AsVisitor_ArticleNeverLiked_ReturnsBadRequest()
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

The second test is new coverage: it is the first time the `LikeNotFound()` branch
executes under test. The same split applies to the unbookmark sibling.

For the commerce pair the assertion becomes
`EntityNotFound("ContentPayment")` and the test name changes to
`NonExistentPayment_ReturnsNotFound` — which is what the handler actually does.
Whether the handlers *should* check the order first is a question for `src/`, and the
renamed tests make it visible instead of hiding it.

### The title comes from the exception type, and the detail from the resource file

Neither pin needs a production change, which is the point.

Every exception strategy titles the problem after the type it handles —
`title: nameof(NotFoundException)`
(`src/Shared/Shared/Application/Exceptions/Handlers/Strategies/NotFoundExceptionHandler.cs:26`)
and the same in the other eighteen. Passing the exception type to the assertion is
therefore a compile-checked expectation: rename the exception and the test stops
building. It separates two *types* that share a status — four distinct types produce
403 — but not two guards inside one handler, since both a missing article and a
missing comment are `NotFoundException`.

The detail is what separates guards, and it is resolved rather than hardcoded.
`BaseApiTest.Localized<TMessage>` (`tests/Integration/Common/Base/BaseApiTest.cs:77-81`)
pulls the application's own message class out of the host container and invokes it
under an explicit culture, so the expectation is produced by the same code path the
response was, and a `.resx` reword moves both sides together.

The culture matters more than it looks. `LocalizationExtension.DefaultCulture` is
`"fr"` with `AcceptLanguageHeaderRequestCultureProvider` as the only provider
(`src/Shared/Shared/Application/Extensions/LocalizationExtension.cs:17-22, 41`), so a
request that sends no `Accept-Language` header — which is every request
`BaseApiTest.Client` makes by default — is answered in French. The thirteen
prose-asserting call sites in this suite work only because their tests set
`Accept-Language: en` first, for example `AdminLoginEndpointV1Tests.cs:116` ahead of
the assertion at `:125`.

### The `code` extension was tried and rejected

The `errorCode` match had two arms, and the second read an extension nothing in `src/`
ever wrote:

```csharp
// the shape at audit time
string haystack = problem.Detail ?? string.Empty;
if (problem.Extensions.TryGetValue("code", out object? code) && code is not null)
{
    haystack += code.ToString();
}

haystack.Should().Contain(errorCode);
```

Wiring that second arm up looked like the obvious fix, and it was implemented: a
`code` extension on every problem, an `ErrorCode` property on `BadRequestException`
and `ConflictException`, `nameof` population in the error factories. It was then
**rejected and fully reverted** — `src/` is byte-identical to HEAD on all 29 files.

The deciding reason is visible in the snippet above: `Contain` is a substring match.
Eight of the 27 entity tokens the error factories raise are substrings of another —
`Article` inside `ArticleComment`, `Lyrics` inside `LyricsSubmission`, `Video` inside
`ShortVideo` — so `ShouldBeProblem(NotFound, "Article")` passed against a response
whose error was `ArticleComment`. That is the same defect the finding is about,
reintroduced inside its own fix. Asserting the localized detail with `Be` has no
equivalent hole and costs no production change.

### Assert what the throw means

```csharp
// before
await act.Should().ThrowAsync<BadRequestException>();

// after
await act.Should().ThrowAsync<BadRequestException>().WithMessage("*already liked*");
```

Where the type is genuinely unambiguous — a single-use exception class — the type
alone is enough. Where a type carries four different meanings, as
`BadRequestException` does in `ArticleInteractionErrors`, the message has to be in
the assertion. This is the unit-suite counterpart of the `Title` / `Detail` split
above: the type is the title, the message is the detail.

### Fix the `BeOneOf` assertions

```csharp
// tests/Integration/Shared/Exceptions/Handlers/ExceptionHandlerTests.cs — after
[Fact]
public async Task MethodNotAllowedExceptionHandler_ShouldReturn405_ForWrongMethod()
{
    var response = await Client.DeleteAsync(Routes.Public.Auth.Login());

    response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
}
```

If that fails, the test has found a real routing defect and the fix belongs in
`src/`. If it passes, the suite has gained an assertion it did not previously have.
Either outcome is better than the current one, which is no information.

## The principle

**An assertion helper must never assert less than its name promises.** `ShouldBeProblem`
promises "this is an RFC 7807 problem response." When the body is empty it delivers
"the number was 404." Silent degradation inside a helper is worse than the same
weakness written inline, because it is invisible at 467 call sites and reviewed at
none of them.

**A status code is not a reason.** Every non-trivial handler has several routes to
the same status, and the test name always claims one of them. Where the assertion
does not distinguish them, the name is documentation that the code does not support
— and the branch the name refers to is often, as here, not covered at all.

**An optional strictness parameter is a parameter that will not be used.** If the
strong form of an assertion is opt-in, 97% of calls will take the weak form. Make
the strong form the default and the weak form explicit, so that weakening an
assertion is a visible act in a diff.

## Checklist

- [x] `ShouldBeProblem` fails on an empty body unless `allowEmptyBody: true` is passed
- [x] Every call site that legitimately expects no body passes the flag explicitly —
      one site does, and it is a multipart model-binding failure
- [x] A typed `ShouldBeProblem<TException>(status, detail)` overload pins the status,
      the `Title` and the exact localized `Detail`
- [ ] Every `ShouldBeProblem` call site converted to the typed overload, with the
      expected detail resolved through `BaseApiTest.Localized<TMessage>` in the culture
      that test's request selects — `fr` unless it sets `Accept-Language: en`
- [ ] The `[Obsolete]` string overload deleted once the last call site is converted
- [ ] `UnlikeArticle` and `UnbookmarkArticle` split into an article-not-found test
      and a genuine not-liked / not-bookmarked test asserting `BadRequest`
- [ ] `RejectPayment` and `AttachPaymentProof` tests renamed to the branch they
      exercise, and the missing order-lookup question raised against `src/`
- [x] `ExceptionHandlerTests.cs:110` asserts `MethodNotAllowed` only
- [x] The other seven `BeOneOf` status assertions reviewed and narrowed —
      `grep -rn "BeOneOf" tests/` returns nothing
- [ ] Throw assertions on reused exception types chain `.WithMessage` or an
      error-code check
