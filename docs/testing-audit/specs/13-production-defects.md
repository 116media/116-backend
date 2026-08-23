# Spec 13 — Production defects

## Goal

Fix the `src/` defects the testing audit surfaced, and add the tests that would have
caught them: two parent-scoping holes, a culture-sensitive `ToLower()` in a query
filter, and three domain guards that bypass i18n. None of it is test work. Each needs
its own ticket, and this spec exists so they are not lost among thirteen specs that
are otherwise entirely about tests.

The ground rule in [00-index.md](00-index.md) says never change `src/` to make a test
easier. This spec is one of the three named exceptions, because the changes here are
not accommodations for tests — they are corrections to behaviour that is wrong today.

## Scope

In scope:

- `src/Modules/Content/Content/Application/Interactions/UseCases/Admin/Commands/DeleteArticleComment/AdminDeleteArticleCommentHandler.cs`
- `src/Modules/Content/Content/Application/Interactions/UseCases/Public/Commands/DeleteArticleComment/PublicDeleteArticleCommentHandler.cs`
- `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/AdminRemovePackageSlotHandler.cs`
- `src/Modules/Identity/Identity/Application/Session/Builders/SessionQueryBuilder.cs`
- `src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs`,
  `.../VideoEntity.cs` and `.../LyricsEntity.cs` — the three `ForceUnpromote` guards
  and the resource entries they need
- The repository methods and specifications those handlers call, where the fix needs a
  scoped lookup.
- The unit and integration tests that prove each fix.

Not in this spec:

- `PublicEditArticleCommentHandler`, `PublicLikeArticleCommentHandler` and
  `PublicUnlikeArticleCommentHandler`. All three take a comment id without an article
  id in the command, so they have no parent to scope to and are not instances of this
  defect. They are listed here so the next reader does not have to re-derive that.
- `PublicAddCommentReplyHandler`, which already scopes correctly — see
  `PublicAddCommentReplyHandler.cs:50` — and is the reference shape Change 1 copies.
- `SessionQueryBuilder.CombineSpecification` calling into `Specification.And` with a
  null argument when the status is unrecognised. That is a separate latent defect; it
  is noted in Change 3's fallout and needs its own ticket rather than being folded in
  here.
- Any broader authorization review. This spec fixes the concrete defects listed; it does not
  audit every parent-child lookup in the codebase.

## Prerequisites

- Spec 02 has landed, so `CultureScope` sets and restores both `CurrentCulture` and
  `CurrentUICulture`. Change 3's regression test needs `CurrentCulture`, because
  `string.ToLower()` reads `CurrentCulture`, not `CurrentUICulture`. The current
  `CultureScope` (`tests/Fixtures/Helpers/CultureScope.cs`) sets only
  `CurrentUICulture`, so the Turkish test would silently prove nothing without spec 02.
- Spec 04 has landed if the integration tests here use `ShouldBeProblem`; the new
  404 assertions must use `ShouldBeProblem<NotFoundException>` with a detail resolved
  through `Localized<TMessage>` rather than being bare. Change 4's regression test
  depends on it directly: the point of that test is which *language* the detail is in.

Neither prerequisite blocks the `src/` fix itself. If these defects are ticketed
separately and shipped ahead of the test-suite work, make the fixes first and add the
regression tests when the prerequisites land — but record that the tests are
outstanding, because a fix with no test is a fix that comes back.

## Changes

### 1. Scope article comments to their article on delete

**The defect.** Both delete-comment handlers look up the comment by id, then
separately look up the article and discard the result. Nothing checks that the comment
belongs to that article.

`AdminDeleteArticleCommentHandler.cs:30-48`:

```csharp
ArticleCommentEntity? comment = await articleRepository.GetCommentByIdAsync(
    commentId: command.CommentId,
    cancellationToken: cancellationToken
);

if (comment is not null)
{
    await articleRepository.GetByIdOrThrowAsync(id: command.ArticleId, cancellationToken: cancellationToken);

    if (comment.SoftDelete())
    {
        articleRepository.UpdateComment(comment: comment);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }

    return new AdminDeleteArticleCommentResult(IsSuccess: true);
}

throw i18n.ArticleInteraction.CommentNotFound(commentId: command.CommentId);
```

The `GetByIdOrThrowAsync` call returns an `ArticleEntity` and its result is not
assigned. It proves the article id exists; it does not relate the article to the
comment. The route is
`DELETE /api/v1/admin/articles/{id}/comments/{commentId}`
(`AdminDeleteArticleCommentEndpointV1.cs:33`), so a moderator who knows any comment id
can delete it by pairing it with any article id that exists. The public handler has the
same shape at `PublicDeleteArticleCommentHandler.cs:30-53`, with an ownership check on
`comment.UserId` in front of it — so for the public route the caller must own the
comment, which narrows the impact but does not remove the defect: the article id in the
URL is still unvalidated against the comment.

`PublicAddCommentReplyHandler.cs:50` already does this correctly:

```csharp
if (parent is null || parent.ArticleId != command.ArticleId || parent.IsDeleted)
{
    throw i18n.ArticleInteraction.CommentNotFound(command.ParentCommentId);
}
```

**The fix.** Scope the lookup itself rather than adding a check after it, so the
handler cannot be written wrongly again. Add an article-scoped overload to
`IArticleRepository` beside the existing method
(`src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs:323`):

```csharp
/// <summary>
/// Returns a single comment by its identifier, scoped to the article it belongs to.
/// </summary>
/// <remarks>
/// A comment that exists under a different article is not a match. Callers that reach a comment
/// through an article-scoped route use this overload so the article segment of the route is
/// enforced rather than merely present.
/// </remarks>
/// <param name="commentId">The comment identifier.</param>
/// <param name="articleId">The article the comment must belong to.</param>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
/// <returns>The comment, or <c>null</c> if no comment with that identifier belongs to that article.</returns>
Task<ArticleCommentEntity?> GetCommentByIdAsync(
    Guid commentId,
    Guid articleId,
    CancellationToken cancellationToken = default
);
```

The implementation mirrors the existing one at
`src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs:405-415`,
using a new specification beside `ArticleCommentByIdSpecification`
(`src/Modules/Content/Content/Application/Editorial/Specifications/ArticleSpecifications.cs:217-224`):

```csharp
/// <summary>
/// Specification that matches an article comment by its identifier within a specific article.
/// </summary>
public class ArticleCommentByIdInArticleSpecification(Guid commentId, Guid articleId)
    : Specification<ArticleCommentEntity>
{
    /// <inheritdoc />
    public override Expression<Func<ArticleCommentEntity, bool>> ToExpression()
    {
        return comment => comment.Id == commentId && comment.ArticleId == articleId;
    }
}
```

Both handlers then become, for the admin case:

```csharp
ArticleCommentEntity? comment = await articleRepository.GetCommentByIdAsync(
    commentId: command.CommentId,
    articleId: command.ArticleId,
    cancellationToken: cancellationToken
);

if (comment is null)
{
    throw i18n.ArticleInteraction.CommentNotFound(commentId: command.CommentId);
}

if (comment.SoftDelete())
{
    articleRepository.UpdateComment(comment: comment);
    await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
}

return new AdminDeleteArticleCommentResult(IsSuccess: true);
```

The public handler keeps its `comment.UserId != command.UserId` ownership check between
the null check and the soft delete.

The `GetByIdOrThrowAsync` call is removed from both handlers. Its only purpose was to
turn a non-existent article id into a 404, and the scoped lookup now does that: a
comment cannot belong to an article that does not exist, so a bad article id yields
`null` and the same `CommentNotFound` exception. Removing it also removes a database
round trip per delete.

What breaks if done wrong: keeping `GetByIdOrThrowAsync` alongside the scoped lookup
changes the error a client sees for a non-existent article from `CommentNotFound` to
the article's own not-found message, which is a contract change nobody asked for.
Removing the scoped lookup and adding an `if (comment.ArticleId != command.ArticleId)`
check instead is functionally equivalent but leaves the same trap for the next handler;
prefer the scoped repository method.

### 2. Scope package slots to their package on removal

The same shape appears at
`src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/AdminRemovePackageSlotHandler.cs:34-55`:

```csharp
await packageRepository.GetByIdWithSlotsOrThrowAsync(id: packageId, cancellationToken: cancellationToken);

PackageSlotEntity? slot = await packageRepository.GetSlotByIdAsync(
    slotId: slotId,
    cancellationToken: cancellationToken
);

if (slot is not null)
{
    packageRepository.RemoveSlot(slot: slot);
    await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    // ...
}
```

The package is loaded with its slots and the result is discarded; the slot is then
looked up globally. `PackageSlotEntity` carries `PackageId`
(`src/Modules/Content/Content/Domain/Entities/PackageSlotEntity.cs:15`), so an
administrator can remove a slot belonging to package A by addressing it under package
B, and the response then reports package B's slot list, which is unchanged. Unlike the
comment case this is a hard delete, not a soft delete.

Apply the same fix. Add a scoped overload to `IPackageRepository` beside
`GetSlotByIdAsync` (`IPackageRepository.cs:48`), backed by a
`PackageSlotByIdInPackageSpecification` beside `PackageSlotByIdSpecification`
(`src/Modules/Content/Content/Application/Catalog/Specifications/PackageSpecifications.cs:47`),
and rewrite the handler:

```csharp
PackageSlotEntity? slot = await packageRepository.GetSlotByIdAsync(
    slotId: slotId,
    packageId: packageId,
    cancellationToken: cancellationToken
);

if (slot is null)
{
    throw i18n.Package.SlotNotFound(slotId: slotId);
}

packageRepository.RemoveSlot(slot: slot);
await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

PackageEntity updatedPackage = await packageRepository.GetByIdWithSlotsOrThrowAsync(
    id: packageId,
    cancellationToken: cancellationToken
);

var dto = updatedPackage.ToPackageDto(mapper);
return new AdminRemovePackageSlotResult(Package: dto, IsSuccess: true);
```

The leading `GetByIdWithSlotsOrThrowAsync` at line 34 is removed; the one after the
commit stays, because its result is used to build the response.

What breaks if done wrong: removing both `GetByIdWithSlotsOrThrowAsync` calls loses the
response payload. Only the leading discarded call goes.

### 3. Remove the culture-sensitive `ToLower()` from the session status filter

**The defect.** `src/Modules/Identity/Identity/Application/Session/Builders/SessionQueryBuilder.cs:33-49`:

```csharp
string normalizedStatus = status.ToLower();
Specification<SessionEntity> statusSpec = normalizedStatus switch
{
    _ when normalizedStatus.Equals(
            nameof(EnumSessionStatus.Active),
            comparisonType: StringComparison.InvariantCultureIgnoreCase
        ) => new SessionIsActiveSpecification(),
    _ when normalizedStatus.Equals(
            nameof(EnumSessionStatus.Expired),
            comparisonType: StringComparison.InvariantCultureIgnoreCase
        ) => new SessionIsExpiredSpecification(),
    _ when normalizedStatus.Equals(
            nameof(EnumSessionStatus.Revoked),
            comparisonType: StringComparison.InvariantCultureIgnoreCase
        ) => new SessionIsRevokedSpecification(),
    _ => null!,
};
```

`string.ToLower()` with no argument uses `CultureInfo.CurrentCulture`. Under a Turkish
locale, `"ACTIVE".ToLower()` produces `"aktıve"` with a dotless `ı`, which does not
match `"Active"` under any comparison. The status filter then falls to the `null!` arm
and the caller receives an unfiltered session list rather than an error.

The `ToLower()` is redundant regardless of locale: all three comparisons below it
already pass `StringComparison.InvariantCultureIgnoreCase`, so lowercasing changes
nothing except in the locales where it breaks the match.

**The fix.** Delete the normalisation and compare the original string:

```csharp
Specification<SessionEntity> statusSpec = status switch
{
    _ when status.Equals(
            nameof(EnumSessionStatus.Active),
            comparisonType: StringComparison.InvariantCultureIgnoreCase
        ) => new SessionIsActiveSpecification(),
    _ when status.Equals(
            nameof(EnumSessionStatus.Expired),
            comparisonType: StringComparison.InvariantCultureIgnoreCase
        ) => new SessionIsExpiredSpecification(),
    _ when status.Equals(
            nameof(EnumSessionStatus.Revoked),
            comparisonType: StringComparison.InvariantCultureIgnoreCase
        ) => new SessionIsRevokedSpecification(),
    _ => null!,
};
```

The local `normalizedStatus` disappears entirely. Do not replace `ToLower()` with
`ToLowerInvariant()`: that would keep a redundant allocation and leave the reader
wondering which of the two normalisations is authoritative.

What breaks if done wrong: switching the comparisons to `StringComparison.Ordinal`
while removing the lowercasing would make the filter case-sensitive and silently break
every client sending `"active"`. The comparison type stays exactly as it is; only the
`ToLower()` call and its local go.

### 4. Localize the three `ForceUnpromote` guards

**The defect.** Three domain guards throw raw English literals, bypassing i18n entirely:

```csharp
// src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs:591
throw new BadRequestException("Article is not currently promoted.");
```

```csharp
// src/Modules/Content/Content/Domain/Entities/VideoEntity.cs:605
throw new BadRequestException("Video is not currently promoted.");
```

```csharp
// src/Modules/Content/Content/Domain/Entities/LyricsEntity.cs:659
throw new BadRequestException("Lyrics page is not currently promoted.");
```

`BadRequestExceptionHandler` puts `exception.Message` straight into the ProblemDetails
`Detail` (`BadRequestExceptionHandler.cs:16-19`), so a client sending
`Accept-Language: fr` receives an English sentence. That is not a test-only concern: the
default request culture is `fr`
(`src/Shared/Shared/Application/Extensions/LocalizationExtension.cs:22`), so **the
default caller gets the wrong language**, and these three are the only error paths in the
Content module where that is true.

**The fix.** Every other guard in these entities raises through an error factory backed
by an `IStringLocalizer` message class. Add the three keys to the existing catalogues —
`ArticleErrorMessage`, `VideoErrorMessage`, `LyricsErrorMessage`, with `en` and `fr`
values — and raise through the matching factory method, exactly as the surrounding guards
do.

The entities take the message provider the same way their neighbouring guards do; do not
introduce a new dependency shape for these three. Where a domain method genuinely cannot
reach a localizer, the guard belongs in the handler that calls it rather than in the
entity — decide that per entity while making the change, and record which route was taken.

What breaks if done wrong: hardcoding the French sentence instead of adding a resource
key moves the defect rather than fixing it, and the resource-completeness theory from
[06-localization-testing.md](06-localization-testing.md) will not see the key at all.

### 5. Regression tests

**For Changes 1 and 2, an integration test per route.** These must go through real
HTTP, because the defect is that a route segment is not enforced, and only a request
exercises the route.

Add to
`tests/Integration/Modules/Content/Application/Interactions/UseCases/Admin/Commands/DeleteArticleComment/V1/AdminDeleteArticleCommentEndpointV1Tests.cs`,
which already has a `SeedArticleWithCommentAsync` helper and a `CommentUrl` builder:

```csharp
[Fact]
public async Task DeleteArticleComment_WithCommentBelongingToAnotherArticle_ReturnsNotFound()
{
    (ArticleEntity _, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync(TestUser.VisitorId);
    (ArticleEntity otherArticle, ArticleCommentEntity _) = await SeedArticleWithCommentAsync(TestUser.VisitorId);
    Client.AuthenticateAsSuperAdmin();

    var response = await Client.DeleteAsync(CommentUrl(otherArticle.Id, comment.Id));

    await response.ShouldBeProblem<NotFoundException>(
        HttpStatusCode.NotFound,
        Localized<SharedExceptionMessage>(m => m.EntityNotFound("ArticleComment"))
    );

    await using ContentDbContext context = CreateDbContext<ContentDbContext>();
    ArticleCommentEntity? persisted = await context.ArticleComments.FindAsync(comment.Id);
    persisted!.IsDeleted.Should().BeFalse("a comment under another article must not be deleted");
}
```

The persisted assertion is the part that matters. A 404 alone would also be produced by
a handler that deleted the comment and then failed to find the article, so the test
must read the row back.

Add the equivalent to
`tests/Integration/Modules/Content/Application/Interactions/UseCases/Public/Commands/DeleteArticleComment/V1/PublicDeleteArticleCommentEndpointV1Tests.cs`
with the comment owned by the authenticated visitor, so the ownership check passes and
the article scoping is the only thing that can produce the 404.

Add the equivalent to the admin remove-package-slot endpoint tests: seed two packages
each with a slot, call the route with package B and package A's slot id, expect 404,
and assert package A still has its slot.

**For Change 3, a unit theory with an explicit culture.** The existing
`SessionQueryBuilderTests` has five `WithStatus` facts that assert only
`specification.Should().NotBeNull()`
(`tests/Unit/Modules/Identity/Application/Session/Builders/SessionQueryBuilderTests.cs:147-210`).
Those are within spec 05's remit; this change adds the culture coverage they lack.

```csharp
/// <summary>
/// Supplies each recognised session status in mixed casing, paired with the culture the
/// comparison runs under. The tr-TR rows cover the dotless-i case, where a culture-sensitive
/// lowercase would break the match and silently drop the status filter.
/// </summary>
/// <returns>Status string and culture name per row.</returns>
public static TheoryData<string, string> StatusesAndCultures()
{
    var data = new TheoryData<string, string>();

    foreach (string status in new[] { "Active", "ACTIVE", "active", "Expired", "EXPIRED", "Revoked", "REVOKED" })
    {
        data.Add(status, "en-US");
        data.Add(status, "tr-TR");
    }

    return data;
}

[Theory]
[MemberData(nameof(StatusesAndCultures))]
public void WithStatus_ShouldBuildASpecification_ForEveryRecognisedStatusInEveryCulture(
    string status,
    string culture
)
{
    using var _ = new CultureScope(culture);
    SessionQueryBuilder builder = new();

    Specification<SessionEntity>? specification = builder.WithStatus(status).Build();

    specification.Should().NotBeNull($"'{status}' is a recognised status under {culture}");
}
```

`CultureScope` must set `CurrentCulture`, not only `CurrentUICulture`, or the `tr-TR`
rows are indistinguishable from the `en-US` rows and the theory proves nothing. That is
the spec 02 prerequisite, and it is worth verifying by hand before trusting this test:
revert Change 3 locally and confirm the seven `tr-TR` rows go red while the seven
`en-US` rows stay green.

Follow spec 05's guidance and strengthen the assertion beyond `NotBeNull` where the
built specification can be evaluated against a seeded `SessionEntity`, so the theory
proves the *right* specification was chosen rather than merely that one was.

## Expected fallout

- The unit tests for all three handlers
  (`tests/Unit/Modules/Content/Application/Interactions/UseCases/Admin/Commands/DeleteArticleComment/AdminDeleteArticleCommentHandlerTests.cs`,
  its public counterpart, and
  `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/AdminRemovePackageSlotHandlerTests.cs`)
  arrange the old repository signatures and will fail to compile. Update the
  arrangements to the scoped overloads.
- `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs:245-251` and `:542`
  set up `GetCommentByIdAsync` with `It.IsAny<Guid>()` for the id. The scoped overload
  needs its own setup helper, and per spec 07 it should match the article id with
  `It.Is<Guid>` rather than `It.IsAny<Guid>`, or the mock will return the comment for
  any article and the unit test will not be able to distinguish the fix from the defect.
- Any existing test that deletes a comment while passing a mismatched article id passes
  today and will start failing. That is the defect, not a regression. Do not relax the
  new scoping to keep such a test green.
- Change 3 exposes a second, separate defect: when the status string is unrecognised,
  `statusSpec` is `null!` and `CombineSpecification` is still called
  (`SessionQueryBuilder.cs:51`). With no prior specification this leaves the filter
  null, which is the current behaviour; with a prior specification it calls
  `.And(null)`. File that as its own ticket. Do not fold it into this change, because
  fixing it changes the response for an unrecognised status, which is an API decision.

## Testing

```bash
dotnet build
dotnet test tests/Unit --filter "FullyQualifiedName~SessionQueryBuilderTests"
dotnet test tests/Unit --filter "FullyQualifiedName~DeleteArticleCommentHandlerTests"
dotnet test tests/Unit --filter "FullyQualifiedName~AdminRemovePackageSlotHandlerTests"
dotnet test tests/Unit
dotnet test tests/Integration --filter "FullyQualifiedName~DeleteArticleCommentEndpointV1Tests"
dotnet test tests/Integration --filter "FullyQualifiedName~RemovePackageSlotEndpointV1Tests"
dotnet test tests/Integration --settings tests/coverage.runsettings
```

Both suites must be green. Each of the four new tests must be confirmed to fail against
the unfixed code: revert the `src/` change, run the test, see red, restore. A
cross-parent test that passes before the fix is testing the wrong thing — most often
because the seeded comment and the seeded article happened to match, or because the
mock returns the comment regardless of the article id.

## Risks

**These are behaviour changes, and one is user-visible.** A client that today
successfully deletes a comment through a mismatched article id will start receiving
404. That is the intended correction, but it is a behaviour change and belongs in the
release notes for the ticket, not buried in a testing PR.

**The repository overload could be adopted inconsistently.** If some call sites move to
the scoped method and others do not, the codebase carries two lookups with nearly
identical names and different safety properties. Mitigation: the four remaining callers
of the unscoped `GetCommentByIdAsync`
(`CommentReplyAddedNotificationsHandler`, `CommentEngagementHandler`,
`PublicLikeArticleCommentHandler`, `PublicUnlikeArticleCommentHandler`,
`PublicEditArticleCommentHandler`) genuinely have no article id in scope, so the
unscoped overload stays. State that in the overload's remarks so the next reader does
not delete it as redundant.

**The Turkish test can silently prove nothing.** If `CultureScope` sets only
`CurrentUICulture`, the `tr-TR` rows behave exactly like the `en-US` rows and the
theory is 14 copies of one case. Mitigation: verify by reverting Change 3 and
confirming exactly the `tr-TR` rows fail. Do this once, by hand, and record the result
in the PR.

**Removing `GetByIdOrThrowAsync` changes which error a client sees.** For a
non-existent article id the response detail changes from the article's not-found
message to the comment's. Mitigation: this is the correct message for the route — the
client asked to delete a comment — and the status code is 404 either way. Note it in
the ticket.

**Fixing the package slot defect is a hard delete path.** A test that gets the seeding
wrong could delete a slot it did not intend to. Mitigation: the regression test asserts
package A still has its slot after the rejected call, so a wrong deletion fails the test
rather than passing quietly.

## Checklist

- [ ] 1 — `ArticleCommentByIdInArticleSpecification` added and the article-scoped
      `GetCommentByIdAsync` overload added to `IArticleRepository` and `ArticleRepository`
- [ ] 1 — `AdminDeleteArticleCommentHandler` uses the scoped lookup and no longer calls
      `GetByIdOrThrowAsync`
- [ ] 1 — `PublicDeleteArticleCommentHandler` uses the scoped lookup, keeps its
      ownership check, and no longer calls `GetByIdOrThrowAsync`
- [ ] 1 — The unscoped overload's remarks name the five callers that legitimately keep
      using it
- [ ] 2 — `PackageSlotByIdInPackageSpecification` added, the scoped `GetSlotByIdAsync`
      overload added, and the leading discarded `GetByIdWithSlotsOrThrowAsync` removed
      from `AdminRemovePackageSlotHandler`
- [ ] 3 — `ToLower()` and the `normalizedStatus` local removed from
      `SessionQueryBuilder.WithStatus`, with the comparison type unchanged
- [ ] 4 — The three `ForceUnpromote` guards raise through their error factories, with
      `en` and `fr` resource entries, and no raw literal remains in
      `ArticleEntity`, `VideoEntity` or `LyricsEntity`
- [ ] 5 — Cross-parent 404 integration test added for the admin delete-comment route,
      asserting the comment is still not deleted
- [ ] 5 — Cross-parent 404 integration test added for the public delete-comment route
- [ ] 5 — Cross-parent 404 integration test added for the remove-package-slot route,
      asserting the other package still has its slot
- [ ] 5 — `WithStatus` theory added over mixed casing and `en-US` / `tr-TR`, with
      `CultureScope` confirmed to set `CurrentCulture`
- [ ] 5 — Integration test proving a `ForceUnpromote` guard answers in French when the
      request sends no `Accept-Language` header
- [ ] Each new test confirmed red against the unfixed code and green after
- [ ] Mock setups for the scoped repository overloads use `It.Is<Guid>` for the parent
      id, not `It.IsAny<Guid>`
- [ ] Follow-up ticket filed for `CombineSpecification` being called with a null
      specification when the status is unrecognised
