# Stage 12 — Public/Admin DTO split & staff-data leak fixes

Closes **[06 §6]** (High) and **[07 S9]** (High). Public endpoints serialize `AuditableDto`
subclasses — every anonymous article read carries `createdBy`/`updatedBy` (staff identifiers),
`customerId`/`orderItemId` (commercial linkage), `rejectionReason`, and an `AuthorDto` with the
author's **email address**.

`AuditableDto`'s own doc comment says "Public-facing DTOs must NOT inherit from this type" — and
then `ArticleDetailDto`, `LyricsDetailDto`, `LyricsSummaryDto`, `ShortVideoDto`,
`ArticleCommentDto`, `PackageDto`, `ContentTypeDto`, `PaymentSummaryDto` all inherit it and all
flow through public queries (37 public use-case files reference the four content ones alone).

> Draft — finalized against the tree Stage 11 lands on. All types below verified against the
> current tree.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Split axis | strip fields from the shared DTOs, or dedicated `Public*Dto` records | **Dedicated records.** Stripping the shared DTO breaks the admin dashboard, which legitimately shows audit fields. A `Public*Dto` that never has the property cannot leak it back through a future mapper edit; inheritance discipline already failed once. |
| D2 | `AuthorInfo` vs `AuthorDto` | treat them as one problem | **They are two problems.** `AuthorInfo` (`Identity.Contracts`) is an in-process contract — the mail handler (`CommentReplyAddedNotificationsHandler.cs:94`) needs its `Email` to address the notification; it is never serialized and **keeps** the field. `AuthorDto` (Content, serialized) loses `Email`; admin queries that show it (`AdminGetArticleByIdHandler.cs:63`) get `AdminAuthorDto` with the field. |
| D3 | Enum leakage | keep `Status` on public detail DTOs | **Drop it.** A public reader only ever sees `Published` (the query filters on it), so the field carries no information — but `RejectionReason` next to it leaks editorial process. Both go. |
| D4 | Wire compat | additive, or breaking | **Deliberately breaking for public responses** — fields disappear. One PR, one release note; frontends confirmed not to read the removed fields first. |

---

## Checklist

- [ ] 12.1 — Census commit: the exact public endpoints returning `AuditableDto` subclasses (grep in Verification)
- [ ] 12.2 — `Public*Dto` records + mapper overloads for the affected aggregates
- [ ] 12.3 — Public query handlers re-pointed; admin paths untouched
- [ ] 12.4 — `AuthorDto` loses `Email`; `AdminAuthorDto` added for the two admin detail queries
- [ ] 12.5 — Tests: raw-JSON absence assertions on public payloads; admin payloads unchanged
- [ ] 12.6 — Verify (build 0/0, csharpier, unit, integration)

---

## Part A — The public records

Representative — `PublicArticleDetailDto` next to the existing `ArticleDetailDto`
(`Application/Shared/DTOs/`). No base type, no audit fields, no commercial linkage, no editorial
state:

```csharp
namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of a published article. Carries no audit trail, no commercial
/// linkage and no editorial state — those stay on <see cref="ArticleDetailDto" /> for admin.
/// </summary>
public record PublicArticleDetailDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Headline,
    string Body,
    string? CoverImageUrl,
    bool IsPromoted,
    DateTimeOffset? PublishedAt,
    string? MetaTitle,
    string? MetaDescription,
    IReadOnlyList<ArticleImageDto> Images,
    IReadOnlyList<TagDto> Tags,
    int ReadTimeInMinutes,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    int BookmarkCount,
    PublicAuthorDto? Author = null,
    bool IsLiked = false,
    bool IsBookmarked = false
);
```

Fields removed relative to `ArticleDetailDto`: `CreatedAt/CreatedBy/UpdatedAt/UpdatedBy` (base),
`AuthorId` (a raw staff identifier), `Status`, `RejectionReason`, `SocialBoost`, `PromotedUntil`,
`PromotionLevelId/Name` (commercial internals — `IsPromoted` alone drives the badge),
`CustomerId`, `CustomerName`, `OrderItemId`.

The same treatment produces `PublicLyricsDetailDto`, `PublicLyricsSummaryDto`,
`PublicShortVideoDto`, `PublicArticleCommentDto` (a comment's public shape keeps `CreatedAt` as
the display timestamp — renamed `PostedAt` so the audit convention doesn't re-attach it),
`PublicPackageDto`, `PublicContentTypeDto`. `PaymentSummaryDto` turns out to be admin-only —
the census commit records that finding instead of inventing a public twin.

## Part B — The author split

Today (both serialized and not):

```csharp
// Identity.Contracts — in-process, feeds the mail handler; NOT serialized. Keeps Email.
public record AuthorInfo(string UserName, string? Email, Guid? AvatarFileId, string? Role);

// Content — serialized into public payloads today, WITH the email. This is the leak.
public record AuthorDto(string UserName, string? Email, string? AvatarUrl, string? Role);
```

After:

```csharp
/// <summary>
/// The public projection of a content author: display identity only.
/// </summary>
public record PublicAuthorDto(string UserName, string? AvatarUrl);

/// <summary>
/// The admin projection of a content author, including the contact address shown in the
/// editorial dashboard.
/// </summary>
public record AdminAuthorDto(string UserName, string? Email, string? AvatarUrl, string? Role);
```

`ShortVideoMapper.cs:112/160` and the public article/lyrics mappers build `PublicAuthorDto`;
`AdminGetArticleByIdHandler.cs:63` and `AdminGetVideoByIdHandler.cs:57` build `AdminAuthorDto`.
`AuthorDto` is deleted when its last reference is gone. `CommentReplyAddedNotificationsHandler`
is untouched — it reads `AuthorInfo.Email`, which never crossed the wire.

## Part C — Mappers

The mappers follow Stage 9's batch shape — a public overload beside the admin one, same
`BuildDtosAsync` internals, different record:

```csharp
/// <summary>
/// Maps published articles to their public projection, file URLs resolved in one batch.
/// </summary>
public static Task<IReadOnlyList<PublicArticleDetailDto>> ToPublicArticleDetailDtosAsync(
    this IReadOnlyList<ArticleEntity> entities,
    IMapper mapper,
    IFileRepository fileRepository,
    IReadOnlySet<Guid> likedArticleIds,
    IReadOnlySet<Guid> bookmarkedArticleIds,
    CancellationToken ct = default
);
```

Public query handlers (`PublicGetArticleBySlugHandler`, `PublicGetPublishedArticlesHandler`,
`PublicGetPopularArticlesHandler`, and their lyrics/short-video siblings) change only their
mapper call and result type; repositories and specifications are untouched.

---

## Tests

The existing endpoint tests deserialize into the server's own DTO types — which **hides** extra
fields and is why this leak had green tests. The absence assertions must read raw JSON:

```csharp
[Fact]
public async Task GetArticleBySlug_AsAnonymous_ShouldNotCarryAuditOrCommercialFields()
{
    Guid articleId = await SeedPublishedArticleAsync();
    string slug = await SlugOfAsync(articleId);

    HttpResponseMessage response = await Client.GetAsync(Routes.Public.Articles.BySlug(slug));
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    JsonElement article = body.RootElement.GetProperty("article");

    string[] forbidden =
    [
        "createdBy", "updatedBy", "createdAt", "updatedAt",
        "customerId", "customerName", "orderItemId",
        "status", "rejectionReason", "authorId",
    ];
    foreach (string field in forbidden)
    {
        article.TryGetProperty(field, out _).Should().BeFalse($"public payloads must not carry {field}");
    }

    article.GetProperty("author").TryGetProperty("email", out _).Should().BeFalse();
}
```

One such test per affected public endpoint family; the admin endpoint tests stay as they are and
prove the admin contract did not move.

---

## Rollout

Breaking for public API consumers: coordinate the frontend/mobile releases; one release note
listing every removed field per endpoint. No database or migration impact.

---

## Verification

1. Build/format/unit/integration green.
2. The census grep returns only admin use-case files:

   ```bash
   grep -rln "ArticleDetailDto\|LyricsDetailDto\|LyricsSummaryDto\|ShortVideoDto\|ArticleCommentDto" \
     src/Modules/Content/Content/Application/*/UseCases/Public
   ```

3. `grep -rn "record AuthorDto" src/` → empty (replaced by the Public/Admin pair).
4. Raw-JSON sweep in the suite passes for every `/api/v1/public/**` payload.

---

**PR:** `fix(api): stop leaking audit, commercial and staff data on public endpoints`
