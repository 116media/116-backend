# Spec 01 — Phase 1: Detail Flags

`IsLiked` / `IsBookmarked` on `ArticleDetailDto` for `GET /api/v1/public/articles/{slug}`.
Reuses the existing `HasLikedAsync` / `HasBookmarkedAsync` repository methods — **no new repo
methods, no EF migration**.

---

## 1.1 `ArticleDetailDto` — add two flags

**File:** `src/Modules/Content/Content/Application/Shared/DTOs/ArticleDetailDto.cs`

Add two XML-documented parameters at the **end** of the positional parameter list (after
`Author`), each defaulting to `false`:

```csharp
/// <param name="IsLiked">
/// Whether the current authenticated user has liked this article. False for anonymous
/// requests and for authenticated users who have not liked it.
/// </param>
/// <param name="IsBookmarked">
/// Whether the current authenticated user has bookmarked this article. False for anonymous
/// requests and for authenticated users who have not bookmarked it.
/// </param>
public record ArticleDetailDto(
    // ... all existing parameters unchanged, through: ...
    Guid? CustomerId = null,
    string? CustomerName = null,
    Guid? OrderItemId = null,
    AuthorDto? Author = null,
    bool IsLiked = false,
    bool IsBookmarked = false
) : AuditableDto;
```

---

## 1.2 `ArticleMapper.ToArticleDetailDtoAsync` — thread the flags

**File:** `src/Modules/Content/Content/Application/Shared/Mappers/ArticleMapper.cs`

Add two optional parameters to the extension method and set them in the object initializer.

```csharp
/// <summary>
/// Maps an <see cref="ArticleEntity" /> to an <see cref="ArticleDetailDto" />, resolving the
/// cover image URL from the associated FileEntity and stamping the current user's
/// interaction flags.
/// </summary>
/// <param name="entity">The article to map.</param>
/// <param name="mapper">The Mapster mapper used for images and tags.</param>
/// <param name="fileRepository">Repository used to resolve the cover image URL.</param>
/// <param name="ct">Cancellation token.</param>
/// <param name="isLiked">Whether the current user has liked this article. False when anonymous.</param>
/// <param name="isBookmarked">Whether the current user has bookmarked this article. False when anonymous.</param>
/// <returns>The mapped detail DTO.</returns>
public static async Task<ArticleDetailDto> ToArticleDetailDtoAsync(
    this ArticleEntity entity,
    IMapper mapper,
    IFileRepository fileRepository,
    CancellationToken ct = default,
    bool isLiked = false,
    bool isBookmarked = false
)
{
    string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileRepository, ct);

    return new ArticleDetailDto(
        // ... existing positional arguments unchanged, through OrderItemId ...
        entity.OrderItemId
    )
    {
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy,
        IsLiked = isLiked,
        IsBookmarked = isBookmarked,
    };
}
```

---

## 1.3 `PublicGetArticleBySlugQuery` — add optional current user

**File:** `.../GetArticleBySlug/PublicGetArticleBySlugQuery.cs`

```csharp
/// <summary>
/// Query to retrieve a single published article by its slug, including the current user's
/// interaction state.
/// </summary>
/// <param name="Slug">The URL-safe slug of the article to retrieve.</param>
/// <param name="CurrentUserId">
/// The authenticated caller's id, or null for an anonymous request. When null, the per-user
/// interaction flags on the returned DTO resolve to false.
/// </param>
public record PublicGetArticleBySlugQuery(string Slug, Guid? CurrentUserId = null)
    : IQuery<PublicGetArticleBySlugResult>;
```

---

## 1.4 `PublicGetArticleBySlugHandler` — resolve and set the flags

**File:** `.../GetArticleBySlug/PublicGetArticleBySlugHandler.cs`

```csharp
/// <inheritdoc />
public async Task<PublicGetArticleBySlugResult> Handle(
    PublicGetArticleBySlugQuery query,
    CancellationToken cancellationToken
)
{
    ArticleEntity? article = await articleRepository.GetBySlugAsync(
        slug: query.Slug,
        cancellationToken: cancellationToken
    );

    if (article is null || article.Status != EnumContentStatus.Published)
    {
        throw i18n.Article.NotFound(Guid.Empty);
    }

    bool isLiked = false;
    bool isBookmarked = false;

    if (query.CurrentUserId is Guid userId)
    {
        isLiked = await articleRepository.HasLikedAsync(userId, article.Id, cancellationToken);
        isBookmarked = await articleRepository.HasBookmarkedAsync(userId, article.Id, cancellationToken);
    }

    ArticleDetailDto dto = await article.ToArticleDetailDtoAsync(
        mapper,
        fileRepository,
        cancellationToken,
        isLiked: isLiked,
        isBookmarked: isBookmarked
    );

    return new PublicGetArticleBySlugResult(Article: dto);
}
```

---

## 1.5 `PublicGetArticleBySlugEndpointV1` — resolve the optional user

**File:** `.../GetArticleBySlug/V1/PublicGetArticleBySlugEndpointV1.cs`

Add `ClaimsPrincipal user` and `IClaimsProvider claimsProvider` to the route delegate; resolve
`Guid? userId`; pass it into the query. Everything else on the endpoint is unchanged.

```csharp
group
    .MapGet(
        "/{slug}",
        async (
            string slug,
            ClaimsPrincipal user,
            IClaimsProvider claimsProvider,
            IDispatcher dispatcher
        ) =>
        {
            Guid? userId = null;
            if (user.Identity?.IsAuthenticated == true)
            {
                userId = claimsProvider.GetUserIdFromClaims(user: user);
            }

            var query = new PublicGetArticleBySlugQuery(Slug: slug, CurrentUserId: userId);
            PublicGetArticleBySlugResult result = await dispatcher.Send(request: query);

            var response = new PublicGetArticleBySlugResponse(Article: result.Article);
            return Results.Ok(response);
        }
    )
    .WithName(endpointName: PublicGetArticleBySlugMetaField.GetArticleBySlug.Name)
    .WithSummary(summary: PublicGetArticleBySlugMetaField.GetArticleBySlug.Summary)
    .WithDescription(description: PublicGetArticleBySlugMetaField.GetArticleBySlug.Description)
    .AllowAnonymous()
    .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
    .Produces<PublicGetArticleBySlugResponse>(statusCode: StatusCodes.Status200OK)
    .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
    .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
```

Add the required usings if not already present:

```csharp
using System.Security.Claims;
using _116.Identity.Contracts.Application; // IClaimsProvider
```

---

## Tasks

- [ ] Add `bool IsLiked = false`, `bool IsBookmarked = false` as the last parameters of `ArticleDetailDto` with multiline XML `<param>` docs.
- [ ] Add `isLiked` / `isBookmarked` optional parameters to `ToArticleDetailDtoAsync`; set them in the object initializer; update the method's XML docs.
- [ ] Add `Guid? CurrentUserId = null` to `PublicGetArticleBySlugQuery` with XML docs.
- [ ] In `PublicGetArticleBySlugHandler`, resolve `isLiked` / `isBookmarked` via `HasLikedAsync` / `HasBookmarkedAsync` only when `CurrentUserId` is set; pass them into the mapper.
- [ ] In `PublicGetArticleBySlugEndpointV1`, add `ClaimsPrincipal user` + `IClaimsProvider claimsProvider`, resolve `Guid? userId`, pass into the query; add the two usings.
- [ ] Confirm the handler injects `IArticleRepository` (it already does) — no new dependency needed.
- [ ] Verify no other call site of `ToArticleDetailDtoAsync` breaks (new params are optional).
- [ ] `dotnet csharpier .` on the touched files (user runs).
