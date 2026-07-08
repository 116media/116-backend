# 04 — Admin Pin / Unpin Use Cases

Two new admin commands let a super admin curate the feed, mirroring the existing
`SetExclusiveCategory` use case. Unlike `set-exclusive` (a single mutex action),
the feed is a **capped set**, so we need both an add (`pin-to-feed`) and a remove
(`unpin-from-feed`) action.

```
src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/
├── PinCategoryToFeed/
│   ├── AdminPinCategoryToFeedCommand.cs
│   ├── AdminPinCategoryToFeedHandler.cs
│   ├── AdminPinCategoryToFeedMetaField.cs
│   └── V1/
│       └── AdminPinCategoryToFeedEndpointV1.cs
└── UnpinCategoryFromFeed/
    ├── AdminUnpinCategoryFromFeedCommand.cs
    ├── AdminUnpinCategoryFromFeedHandler.cs
    ├── AdminUnpinCategoryFromFeedMetaField.cs
    └── V1/
        └── AdminUnpinCategoryFromFeedEndpointV1.cs
```

## Endpoints

| Method  | Route                                              | Auth        | Body | Returns               |
|---------|----------------------------------------------------|-------------|------|-----------------------|
| `PATCH` | `/api/v1/admin/categories/{id}/pin-to-feed`        | SuperAdmin  | none | updated `CategoryDto` |
| `PATCH` | `/api/v1/admin/categories/{id}/unpin-from-feed`    | SuperAdmin  | none | updated `CategoryDto` |

Add the route segments to `CatalogRouteConstants`:

```csharp
/// <summary>
/// Route segment for pinning a category to the content feed.
/// Example: /api/v1/admin/categories/{id}/pin-to-feed.
/// </summary>
public const string PinToFeed = "pin-to-feed";

/// <summary>
/// Route segment for unpinning a category from the content feed.
/// Example: /api/v1/admin/categories/{id}/unpin-from-feed.
/// </summary>
public const string UnpinFromFeed = "unpin-from-feed";
```

## AdminPinCategoryToFeedCommand

**File:** `.../PinCategoryToFeed/AdminPinCategoryToFeedCommand.cs`

```csharp
public record AdminPinCategoryToFeedCommand(string Id) : ICommand<AdminPinCategoryToFeedResult>;

public record AdminPinCategoryToFeedResult(CategoryDto Category);
```

## AdminPinCategoryToFeedHandler — cap + FIFO eviction

**File:** `.../PinCategoryToFeed/AdminPinCategoryToFeedHandler.cs`

This is the heart of the feature. It validates the category, then enforces the
per-content-type cap by unpinning the oldest pinned category when needed.

```csharp
public class AdminPinCategoryToFeedHandler(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminPinCategoryToFeedCommand, AdminPinCategoryToFeedResult>
{
    /// <inheritdoc />
    public async Task<AdminPinCategoryToFeedResult> Handle(
        AdminPinCategoryToFeedCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (!category.IsActive)
        {
            throw i18n.Category.CannotPinInactiveToFeed();
        }

        // Only the video feed exists today, so only Video categories can be pinned.
        // Article categories become eligible when the article feed lands.
        if (category.ContentType.Name != nameof(EnumCoreContentType.Video))
        {
            throw i18n.Category.ContentTypeNotFeedable();
        }

        // Eligibility gate: a category needs enough published videos to fill a credible section.
        int publishedCount = await videoRepository.CountPublishedByCategoryAsync(
            categoryId: category.Id,
            cancellationToken: cancellationToken
        );

        if (publishedCount < EditorialFeedConstants.MinVideosToPinToFeed)
        {
            throw i18n.Category.NotEnoughVideosToPinToFeed();
        }

        IReadOnlyList<CategoryEntity> pinned = await categoryRepository.GetPinnedToFeedCategoriesAsync(
            contentTypeId: category.ContentTypeId,
            cancellationToken: cancellationToken
        );

        bool alreadyPinned = pinned.Any(c => c.Id == category.Id);

        // FIFO eviction: only when pinning a NEW category that would exceed the cap.
        if (!alreadyPinned && pinned.Count >= CatalogFeedConstants.MaxPinnedCategoriesPerContentType)
        {
            CategoryEntity oldest = pinned.OrderBy(c => c.PinnedToFeedAt).First();
            oldest.UnpinFromFeed();
        }

        // Re-pinning an already-pinned category refreshes its timestamp (front of queue).
        category.PinToFeed();

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminPinCategoryToFeedResult(Category: dto);
    }
}
```

### Eviction semantics

- `GetPinnedToFeedCategoriesAsync` returns newest-first; the handler re-sorts **ascending** (`OrderBy(c => c.PinnedToFeedAt)`) and unpins `.First()` — the oldest.
- Eviction runs **only** when pinning a category that is not already in the set and the set is at capacity. Re-pinning an existing one just bumps its timestamp and never evicts.
- Eviction is **silent** (no error), matching the requirement "the old one gets kicked out" and the `IsExclusive` "unset previous" precedent.

No validator is needed — `Id` comes from the route. Add the `AdminPinCategoryToFeedMetaField` (route name/summary/description) following `AdminSetExclusiveCategoryMetaField`.

## AdminUnpinCategoryFromFeedHandler

**File:** `.../UnpinCategoryFromFeed/AdminUnpinCategoryFromFeedHandler.cs`

```csharp
public class AdminUnpinCategoryFromFeedHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminUnpinCategoryFromFeedCommand, AdminUnpinCategoryFromFeedResult>
{
    /// <inheritdoc />
    public async Task<AdminUnpinCategoryFromFeedResult> Handle(
        AdminUnpinCategoryFromFeedCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        category.UnpinFromFeed();

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminUnpinCategoryFromFeedResult(Category: dto);
    }
}
```

`UnpinFromFeed()` returning `false` for a category that was not pinned is treated as
idempotent success (no error) — `PATCH .../unpin-from-feed` on an unpinned category is a no-op `200`.

## Endpoints (Carter)

Both endpoints mirror `AdminSetExclusiveCategoryEndpointV1` exactly — same group,
auth, rate limit, and `Produces` set. Only the route, command, and metadata differ.

```csharp
group
    .MapPatch(
        $"/{{id}}/{CatalogRouteConstants.PinToFeed}",
        async (string id, IDispatcher dispatcher) =>
        {
            var command = new AdminPinCategoryToFeedCommand(Id: id);
            AdminPinCategoryToFeedResult result = await dispatcher.Send(request: command);
            return Results.Ok(new AdminPinCategoryToFeedResponse(Category: result.Category));
        }
    )
    .WithName(endpointName: AdminPinCategoryToFeedMetaField.PinCategoryToFeed.Name)
    .WithSummary(summary: AdminPinCategoryToFeedMetaField.PinCategoryToFeed.Summary)
    .WithDescription(description: AdminPinCategoryToFeedMetaField.PinCategoryToFeed.Description)
    .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
    .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
    .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
    .ProducesValidationProblem()
    .Produces<AdminPinCategoryToFeedResponse>(statusCode: StatusCodes.Status200OK)
    .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
    .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
    .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
    .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
```

## Create / Update passthrough (intentionally omitted)

`IsPinnedToFeed` is **not** added to `AdminCreateCategoryCommand` / `AdminUpdateCategoryCommand`.
Pinning is a deliberate curation action with cap + eviction side effects, so it stays
in its own endpoints — the same reason `IsExclusive` is toggled via `set-exclusive`
rather than at create/update time. (`CreateCategory`/`UpdateCategory` already accept
`IsExclusive` as a param, but that is a simple mutex with no capacity logic; do not
follow that precedent for the capped feed flag.)
