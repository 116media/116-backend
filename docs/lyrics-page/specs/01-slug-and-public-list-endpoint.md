# Spec 01 — Slug Column, Editorial Status Workflow & Public List Endpoint

Three problems solved together: `LyricsEntity` has no stored slug (the existing
`/lyrics/{songTitle}/{artistName}` route does an exact `ILIKE` match against raw columns, not a
slug lookup), it has **no draft/review/publish workflow at all** — every record is public the
instant it's created, unlike articles and videos — and there is no way to *browse* lyrics publicly.
All three are fixed the same way `ArticleEntity`/`VideoEntity` already work.

## 0. Category, commerce fields & editorial status workflow — full parity with articles/videos

The original design treated "every lyrics record is public the instant it's created" as
intentional, on the reasoning that lyrics have no payment gate, and gave lyrics no `CategoryId` at
all (reasoning that lyrics don't need editorial categorization, only tags for discovery). Both
assumptions were corrected: unreviewed content must not be public once community submissions
(spec 11) and AI translations exist, and lyrics categorization/monetization should follow **the
exact same free-vs-paid category model** articles and videos already use, not a special case —
including the category-driven free/paid branching, not just the status enum.

`CategoryEntity` already supports this distinction (`IsFree` — see `CONTENT_SCHEMA.sql`); nothing
new is needed there. Any number of lyrics categories can exist (seeded/created via the same admin
category CRUD articles/videos already use, scoped to a new `Lyrics` `ContentTypeEntity` — see
spec 12), each independently marked free or paid. **Tags (spec 07) remain a separate, additional
concern** — discovery/similarity, not editorial classification — lyrics get both a required
`CategoryId` *and* a many-to-many tag set, exactly like articles and videos already do.

### `LyricsEntity` additions

```csharp
/// <summary>
/// The category this lyrics page belongs to. Determines whether it's free or paid — the
/// exact same CategoryEntity.IsFree distinction ArticleEntity/VideoEntity already use.
/// </summary>
public Guid CategoryId { get; private set; }

/// <summary>
/// The B2B customer who commissioned this lyrics page as a paid/promoted product.
/// <c>null</c> for free content — the common case (admin-entered, community-submitted, or
/// verified-artist self-uploaded lyrics all default to a free category, exactly like a
/// song with no commercial relationship behind it).
/// </summary>
public Guid? CustomerId { get; private set; }

/// <summary>
/// The order item commissioning this lyrics page. Both CustomerId and OrderItemId are set
/// together or both are null — same invariant ArticleEntity/VideoEntity already enforce.
/// </summary>
public Guid? OrderItemId { get; private set; }

/// <summary>
/// Current status in the editorial workflow.
/// </summary>
public EnumContentStatus Status { get; private set; }

/// <summary>
/// Reason provided by the editorial team when rejecting the lyrics page.
/// </summary>
[MaxLength(length: ContentConstants.MaxRejectionReasonLength)]
public string? RejectionReason { get; private set; }

/// <summary>
/// When the lyrics page was first published. <c>null</c> until <c>Publish()</c> is called.
/// </summary>
public DateTimeOffset? PublishedAt { get; private set; }
```

### Two factories — `CreateFree`/`CreatePaid`, mirroring `ArticleEntity` exactly

```csharp
/// <summary>
/// Creates a new free lyrics page — the default path for admin CRUD, community
/// submissions, and verified-artist self-uploads (spec 11). No customer, no order.
/// </summary>
public static LyricsEntity CreateFree(
    Guid id, Guid categoryId, string songTitle, string artistName, string slug,
    string lyricsText, string language, Guid authorId, Guid? videoId, LyricsErrors errors)
{
    ValidateRequiredFields(songTitle, artistName, lyricsText, errors);

    if (string.IsNullOrWhiteSpace(value: slug))
    {
        throw errors.SlugRequired();
    }

    return new LyricsEntity
    {
        Id = id, CategoryId = categoryId, AuthorId = authorId, VideoId = videoId,
        SongTitle = songTitle, ArtistName = artistName, Slug = slug,
        LyricsText = lyricsText, Language = language, Status = EnumContentStatus.Draft,
    };
}

/// <summary>
/// Creates a new paid lyrics page linked to a customer and order item — a label/artist
/// commissioning a promoted lyrics placement from the start. Both customerId and
/// orderItemId must be provided together.
/// </summary>
public static LyricsEntity CreatePaid(
    Guid id, Guid customerId, Guid orderItemId, Guid categoryId, string songTitle,
    string artistName, string slug, string lyricsText, string language, Guid authorId,
    Guid? videoId, LyricsErrors errors)
{
    ValidateRequiredFields(songTitle, artistName, lyricsText, errors);

    if (string.IsNullOrWhiteSpace(value: slug))
    {
        throw errors.SlugRequired();
    }

    return new LyricsEntity
    {
        Id = id, CustomerId = customerId, OrderItemId = orderItemId, CategoryId = categoryId,
        AuthorId = authorId, VideoId = videoId, SongTitle = songTitle, ArtistName = artistName,
        Slug = slug, LyricsText = lyricsText, Language = language, Status = EnumContentStatus.Draft,
    };
}
```

`CreateForVideo` is folded into these two (a `videoId` parameter on both, nullable) rather than
kept as a third factory — a video link and a free/paid distinction are orthogonal, and
`ArticleEntity` doesn't have a parallel "linked to a video" factory either, so this keeps the shape
consistent with the pattern being mirrored.

### Status transitions — `Submit()`/`MarkPendingReview()`, same dual path as articles

```csharp
/// <summary>
/// Transitions a paid lyrics page from <c>Draft</c> → <c>PendingPayment</c>.
/// Call <see cref="MarkPendingReview" /> instead for free lyrics pages.
/// </summary>
/// <returns><c>true</c> if submitted; <c>false</c> if already pending payment.</returns>
public bool Submit()
{
    if (Status == EnumContentStatus.PendingPayment)
    {
        return false;
    }

    Status = EnumContentStatus.PendingPayment;
    return true;
}

/// <summary>
/// Transitions a free lyrics page from <c>Draft</c> → <c>PendingReview</c>, or a paid
/// lyrics page from <c>PendingPayment</c> → <c>PendingReview</c> after payment is verified.
/// </summary>
/// <returns><c>true</c> if moved to pending review; <c>false</c> if already pending review.</returns>
public bool MarkPendingReview()
{
    if (Status == EnumContentStatus.PendingReview)
    {
        return false;
    }

    Status = EnumContentStatus.PendingReview;
    return true;
}

/// <summary>
/// Marks the lyrics page as editorially approved (→ <c>Approved</c>).
/// </summary>
public bool Approve()
{
    if (Status == EnumContentStatus.Approved)
    {
        return false;
    }

    Status = EnumContentStatus.Approved;
    return true;
}

/// <summary>
/// Publishes the lyrics page and records the publication timestamp.
/// </summary>
public bool Publish()
{
    if (Status == EnumContentStatus.Published)
    {
        return false;
    }

    Status = EnumContentStatus.Published;
    PublishedAt = DateTimeOffset.UtcNow;
    return true;
}

/// <summary>
/// Rejects the lyrics page with a mandatory reason.
/// </summary>
public bool Reject(string reason)
{
    if (Status == EnumContentStatus.Rejected)
    {
        return false;
    }

    Status = EnumContentStatus.Rejected;
    RejectionReason = reason;
    return true;
}

/// <summary>
/// Archives the lyrics page, removing it from all public feeds without deleting it.
/// </summary>
public bool Archive()
{
    if (Status == EnumContentStatus.Archived)
    {
        return false;
    }

    Status = EnumContentStatus.Archived;
    return true;
}
```

### Handler — same branching `AdminSubmitArticleHandler` already does

`AdminSubmitLyricsCommand`'s handler mirrors `AdminSubmitArticleHandler` exactly: if
`CustomerId`/`OrderItemId` are set and the linked order isn't paid yet, call `Submit()` (→
`PendingPayment`); otherwise (free lyrics, or a paid lyrics page whose order is already paid) call
`MarkPendingReview()` directly. **This is also the answer to "when lyrics has no artist, can it be
free" — free vs. paid was never about whether an artist is claimed; it's about whether a customer
commissioned this specific lyrics page.** The overwhelming majority of lyrics (admin-entered,
community-submitted, verified-artist self-uploads) have no `CustomerId` at all and go through the
free branch, regardless of whether `ArtistId` is set — exactly like a free article category needs
no customer either. Only a lyrics page a label/artist is specifically *paying to commission or
promote* ever gets a `CustomerId`/`OrderItemId`.

### Admin endpoints

Four new admin endpoints, copy-pasted from the article equivalents with the status-transition
guards preserved exactly (`InvalidStatusTransition` on an out-of-order call, e.g. rejecting a
`Draft` record):

| Method | Route | Handler template |
| --- | --- | --- |
| `PATCH` | `/api/v1/admin/lyrics/{id}/submit` | `AdminSubmitArticleHandler` |
| `PATCH` | `/api/v1/admin/lyrics/{id}/approve` | `AdminApproveArticleHandler` |
| `PATCH` | `/api/v1/admin/lyrics/{id}/publish` | `AdminPublishArticleHandler` |
| `PATCH` | `/api/v1/admin/lyrics/{id}/reject` | `AdminRejectArticleHandler` |
| `PATCH` | `/api/v1/admin/lyrics/{id}/archive` | `AdminArchiveArticleHandler` |

**Every public read (by-slug lookup, by-video lookup, and the new list endpoint below) filters to
`Status == EnumContentStatus.Published` only** — a new
`LyricsPublishedSpecification`, combined via `.And()` exactly like
`ArticleSpecifications`' own `Status == EnumContentStatus.Published` checks:

```csharp
/// <summary>
/// Specification that matches only lyrics records with <c>Status = Published</c>.
/// </summary>
public class LyricsPublishedSpecification : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => lyrics.Status == EnumContentStatus.Published;
    }
}
```

`GetBySlugAsync`/`GetByVideoIdAsync` (used by the two public lookups) both combine this
specification with their existing one; the admin lookups (`GetByIdAsync`,
`GetAllAsync`) do not — an admin must be able to see and review unpublished drafts.

**Interaction with community submissions (spec 11)**: this status workflow is what a community
submission's "pending" state actually *is* now — spec 11's approval flow creates the
`LyricsEntity` directly in `Draft` (or `PendingReview`, skipping the separate submission-status
enum it originally proposed) rather than inventing a parallel status concept. Spec 11 is written
against this shared status field, not a duplicate one.

## 1. `Slug` column on `LyricsEntity`

`Domain/Entities/LyricsEntity.cs` gains a new property, mirroring `VideoEntity.Slug` exactly:

```csharp
/// <summary>
/// URL-safe slug used in public lyrics URLs (e.g., "fally-ipupa-eloko-oyo").
/// Must be unique across all lyrics records.
/// </summary>
[MaxLength(length: ContentConstants.MaxSlugLength)]
public string Slug { get; private set; } = null!;
```

`CreateFree`/`CreatePaid` (§0 above) already take `slug` and throw `errors.SlugRequired()` when
blank — not repeated here. `Update` gains the identical `slug`/`SlugRequired()` guard.
`LyricsConfiguration` gains:

```csharp
builder.Property(x => x.Slug).HasMaxLength(ContentConstants.MaxSlugLength).IsRequired();
builder.HasIndex(x => x.Slug).IsUnique();
```

### Slug generation — one segment, `{artist}-{song-title}-lyrics`, Genius-style

The slug is generated by combining **only** the artist's main name and the song title — never any
other field — into a single hyphenated segment, the same shape Genius uses
(`genius.com/Fally-ipupa-mayday-lyrics`): `artistName` + `songTitle` + a literal `lyrics` suffix,
run through this codebase's existing slugify step.

One difference from Genius's own display: this platform's `SlugRegex` (`EditorialValidation.cs`)
requires `^[a-z0-9]+(?:-[a-z0-9]+)*$` — **all lowercase**, matching every other slugged entity
(articles, videos, tags, categories) — where Genius keeps capitalized words. The generated slug is
fully lowercased to satisfy that existing constraint; only the casing differs from Genius's
example, not the shape:

```text
artistName = "Fally Ipupa"
songTitle  = "Mayday"
slug       = slugify(`${artistName} ${songTitle} lyrics`)
           = "fally-ipupa-mayday-lyrics"
```

This is a single value computed once (dashboard-side, on create — see the collision note below)
and stored verbatim in the `Slug` column; the by-slug lookup (§4) does a plain equality/`ILIKE`
match against that stored value, never a live slugify-and-compare at request time. This directly
supersedes the misleading, un-slugified two-segment route this spec replaces — that route's own
docstring gave a kebab-case example (`/lyrics/eloko-oyo/fally-ipupa`) that its actual
`ILIKE`-against-raw-columns implementation never honored; the new single-segment `Slug` column is
what makes a real hyphenated URL like `/lyrics/fally-ipupa-mayday-lyrics` actually work end to end.

Collision handling follows this platform's normal slug convention exactly (unlike this doc set's
earlier, now-superseded "clean-first, suffix-only-on-collision" proposal — see the frontend docs'
[05-open-questions.md](../../../../frontend/docs/lyrics-page/05-open-questions.md) question 1 for
that reversed decision): the dashboard's shared `generateSlug(text, { unique: true })` util
generates the slug with its usual random-suffix behavior, and `AdminCreateLyricsHandler`'s
`GetBySlugAsync` uniqueness check (§3) rejects an actual duplicate the same way
`AdminCreateVideoHandler` already does. No special-cased "clean slug on the happy path" behavior is
introduced for lyrics — it uses the same mechanism every other slugged entity in this codebase
already uses.

## 2. Errors, validator, DTO

`LyricsErrors` (`Application/Shared/Errors/LyricsErrors.cs`) gains two methods, mirroring
`VideoErrors` exactly:

```csharp
/// <summary>
/// Throws when a lyrics slug is required but not provided.
/// </summary>
public BadRequestException SlugRequired()
{
    return new BadRequestException(i18n.SlugRequired());
}

/// <summary>
/// Throws when a lyrics record with the given slug already exists.
/// </summary>
public ConflictException SlugAlreadyExists(string slug)
{
    return new ConflictException(i18n.SlugAlreadyExists(slug: slug));
}
```

`LyricsErrorMessage` (the `.resx`-backed i18n provider) gains matching `SlugRequired()`,
`SlugTooLong(int)`, `SlugInvalidFormat()`, `SlugAlreadyExists(string)` entries in all three `.resx`
files (`LyricsErrorMessage.resx`/`.en.resx`/`.fr.resx`), following the existing entries for
`SongTitleRequired`/`AlreadyExists` verbatim in structure.

`EditorialValidation.cs` gains `ValidLyricsSlug`, copy-pasted from `ValidVideoSlug` with the i18n
type swapped:

```csharp
/// <summary>
/// Validates lyrics slug with length and format constraints (lowercase, letters, numbers, hyphens).
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
/// <param name="ruleBuilder">The rule builder for the slug property.</param>
/// <param name="i18n">The lyrics error message provider.</param>
/// <returns>The configured rule builder.</returns>
public static IRuleBuilderOptions<T, string?> ValidLyricsSlug<T>(
    this IRuleBuilderInitial<T, string?> ruleBuilder,
    LyricsErrorMessage i18n
)
{
    return ruleBuilder
        .Cascade(cascadeMode: CascadeMode.Stop)
        .NotEmpty()
        .WithMessage(i18n.SlugRequired())
        .MaximumLength(maximumLength: ContentConstants.MaxSlugLength)
        .WithMessage(i18n.SlugTooLong(ContentConstants.MaxSlugLength))
        .Matches(SlugRegex())
        .WithMessage(i18n.SlugInvalidFormat());
}
```

`LyricsDto` gains `Slug`:

```csharp
public record LyricsDto(
    Guid Id, string SongTitle, string ArtistName, string Slug, string LyricsText, string Language,
    Guid? VideoId, string? MetaTitle, string? MetaDescription, string AuthorId,
    AuthorDto? Author = null
) : AuditableDto;
```

## 3. Repository — slug lookup, replacing the song+artist lookup

`ILyricsRepository` (`Application/Shared/Repositories/ILyricsRepository.cs`): replace
`GetBySongTitleAndArtistAsync` with `GetBySlugAsync`, mirroring `IVideoRepository.GetBySlugAsync`:

```csharp
/// <summary>
/// Retrieves a lyrics record matching the given slug (case-insensitive).
/// Returns null if not found.
/// </summary>
/// <param name="slug">The slug to look up.</param>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
/// <returns>The lyrics entity if found, otherwise null.</returns>
Task<LyricsEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
```

New specification, replacing `LyricsBySongAndArtistSpecification`
(`Application/Editorial/Specifications/LyricsSpecifications.cs`):

```csharp
/// <summary>
/// Specification that matches a lyrics record by its unique slug (case-insensitive).
/// </summary>
public class LyricsBySlugSpecification(string slug) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => EF.Functions.ILike(lyrics.Slug, slug);
    }
}
```

`LyricsRepository.GetBySlugAsync` mirrors the existing `GetBySongTitleAndArtistAsync`
implementation 1:1, swapping the specification. `AdminCreateLyricsHandler` and
`AdminUpdateLyricsHandler` swap their `GetBySongTitleAndArtistAsync` uniqueness check for
`GetBySlugAsync`, throwing `i18n.Lyrics.SlugAlreadyExists(command.Slug)` instead of
`AlreadyExists(songTitle, artistName)` — same shape as `AdminCreateVideoHandler`'s existing
slug-uniqueness check (read above, copied directly):

```csharp
LyricsEntity? existing = await lyricsRepository.GetBySlugAsync(
    slug: command.Slug, cancellationToken: cancellationToken);

if (existing is not null)
{
    throw i18n.Lyrics.SlugAlreadyExists(slug: command.Slug);
}
```

`AdminCreateLyricsCommand`/`AdminUpdateLyricsCommand` gain `Slug`, `CategoryId`, `CustomerId`
(nullable), and `OrderItemId` (nullable) fields — the identical four `AdminCreateArticleCommand`
already has. `AdminCreateLyricsValidator`/`AdminUpdateLyricsValidator` gain
`RuleFor(x => x.Slug).ValidLyricsSlug(i18n.Lyrics.Msg)` and `RuleFor(x => x.CategoryId).NotEmpty()`.
`AdminCreateLyricsHandler` branches `CreateFree`/`CreatePaid` exactly like
`AdminCreateArticleHandler` does (`command.CustomerId.HasValue ? CreatePaid(...) : CreateFree(...)`)
— see §0 above for both factories.

## 4. The by-slug endpoint — replaces the two-segment route

`PublicGetLyricsBySlugQuery`/`Handler`/`V1/PublicGetLyricsBySlugEndpointV1` all change from
`(SongTitle, ArtistName)` to a single `Slug`:

```csharp
/// <summary>
/// Query for retrieving a lyrics page by its unique slug.
/// </summary>
/// <param name="Slug">The URL-safe slug identifying the lyrics page.</param>
public record PublicGetLyricsBySlugQuery(string Slug) : IQuery<PublicGetLyricsBySlugResult>;
```

```csharp
public async Task<PublicGetLyricsBySlugResult> Handle(
    PublicGetLyricsBySlugQuery query, CancellationToken cancellationToken)
{
    LyricsEntity? lyrics = await lyricsRepository.GetBySlugAsync(
        slug: query.Slug, cancellationToken: cancellationToken);

    if (lyrics is not null)
    {
        var dto = lyrics.ToLyricsDto(mapper);
        return new PublicGetLyricsBySlugResult(Lyrics: dto);
    }

    throw i18n.Lyrics.NotFound(id: Guid.Empty);
}
```

```csharp
group.MapGet(
    "/{slug}",
    async (string slug, IDispatcher dispatcher) =>
    {
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);
        PublicGetLyricsBySlugResult result = await dispatcher.Send(request: query);
        return Results.Ok(new PublicGetLyricsBySlugResponse(Lyrics: result.Lyrics));
    }
)
```

This **replaces** `/api/v1/public/lyrics/{songTitle}/{artistName}` — it does not stay alongside the
new route. `PublicGetLyricsByVideoIdEndpointV1` (`/lyrics/videos/{videoId}` — the real route path;
an earlier draft of this doc used `/by-video/{videoId}`) is unaffected beyond
its `LyricsDto` response now also carrying `Slug`.

## 5. Public list endpoint (new)

Nothing lets an anonymous visitor browse lyrics today. New query, mirroring
`PublicGetPublishedArticlesQuery`'s shape, simplified because lyrics have no publish status:

```csharp
/// <summary>
/// Query for retrieving a paginated, publicly browsable list of lyrics pages.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters.</param>
/// <param name="Search">Optional search term across song title, artist name, and lyrics text.</param>
/// <param name="Language">Optional exact-match ISO 639-1 language filter.</param>
/// <param name="Sort">Optional sort key: "views", "likes", "shares". Defaults to newest first.</param>
public record PublicGetPublishedLyricsQuery(
    PaginatedRequest PaginatedRequest,
    string? Search,
    string? Language,
    string? Sort
) : IQuery<PublicGetPublishedLyricsResult>;

/// <summary>
/// Result of the <see cref="PublicGetPublishedLyricsQuery" /> containing a paginated lyrics page.
/// </summary>
/// <param name="Lyrics">The paginated result of matching lyrics.</param>
public record PublicGetPublishedLyricsResult(PaginatedResult<LyricsDto> Lyrics);
```

New specification for the language filter:

```csharp
/// <summary>
/// Specification that matches lyrics records by an exact-match ISO 639-1 language code.
/// Not ILIKE — language codes are a closed vocabulary, not free text.
/// </summary>
public class LyricsByLanguageSpecification(string language) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics => lyrics.Language == language;
    }
}
```

`ILyricsQueryBuilder`/`LyricsQueryBuilder` gain `WithLanguage`:

```csharp
/// <inheritdoc />
public ILyricsQueryBuilder WithLanguage(string? language)
{
    if (string.IsNullOrWhiteSpace(value: language))
    {
        return this;
    }

    CombineSpecification(spec: new LyricsByLanguageSpecification(language: language));
    return this;
}
```

`ILyricsRepository` gains a list method accepting the sort key (kept separate from the existing
admin `GetAllAsync` rather than overloading it, since the admin listing has no sort concept today):

```csharp
/// <summary>
/// Retrieves a paginated, publicly browsable list of lyrics, optionally filtered by search
/// term and language, and sorted by the given metric.
/// </summary>
/// <param name="page">The 1-based page number.</param>
/// <param name="pageSize">The number of items per page.</param>
/// <param name="search">Optional search term across song title, artist name, and lyrics text.</param>
/// <param name="language">Optional exact-match ISO 639-1 language filter.</param>
/// <param name="sort">Optional sort key: "views", "likes", "shares". Null sorts by newest first.</param>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
/// <returns>A tuple containing the list of lyrics and the total count.</returns>
Task<(List<LyricsEntity> Lyrics, int TotalCount)> GetPublishedAsync(
    int page, int pageSize, string? search, string? language, string? sort,
    CancellationToken cancellationToken = default);
```

```csharp
public async Task<(List<LyricsEntity> Lyrics, int TotalCount)> GetPublishedAsync(
    int page, int pageSize, string? search, string? language, string? sort,
    CancellationToken cancellationToken = default)
{
    IQueryable<LyricsEntity> query = context.Lyrics;

    Specification<LyricsEntity>? spec = new LyricsQueryBuilder()
        .WithSearch(search: search)
        .WithLanguage(language: language)
        .Build();

    if (spec is not null)
    {
        query = query.ApplySpecification(specification: spec);
    }

    int totalCount = await query.CountAsync(cancellationToken);

    query = sort switch
    {
        "views" => query.OrderByDescending(l => l.ViewCount),
        "likes" => query.OrderByDescending(l => l.LikeCount),
        "shares" => query.OrderByDescending(l => l.ShareCount),
        _ => query.OrderByDescending(l => l.CreatedAt),
    };

    List<LyricsEntity> lyrics = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return (lyrics, totalCount);
}
```

`ViewCount`/`LikeCount`/`ShareCount` referenced above land in spec 04 — this method is written
against their final shape; land spec 04 before or together with this piece specifically (the sort
switch), or omit the `sort` cases until then and add them when spec 04 lands.

Handler:

```csharp
public class PublicGetPublishedLyricsHandler(ILyricsRepository lyricsRepository, IMapper mapper)
    : IQueryHandler<PublicGetPublishedLyricsQuery, PublicGetPublishedLyricsResult>
{
    public async Task<PublicGetPublishedLyricsResult> Handle(
        PublicGetPublishedLyricsQuery query, CancellationToken cancellationToken)
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<LyricsEntity> lyricsList, int totalCount) = await lyricsRepository.GetPublishedAsync(
            page: pageIndex + 1, pageSize: pageSize, search: query.Search,
            language: query.Language, sort: query.Sort, cancellationToken: cancellationToken);

        IReadOnlyList<LyricsDto> dtoList = lyricsList.AsReadOnly().ToLyricsDtos(mapper);

        return new PublicGetPublishedLyricsResult(
            new PaginatedResult<LyricsDto>(pageIndex, pageSize, totalCount, dtoList.ToList()));
    }
}
```

Note this uses the synchronous `ToLyricsDtos` (no author resolution) — a public list has no more
reason to resolve `Author` than the two existing public lookups do.

Endpoint — a sibling `MapGet("/", ...)` in the same route group `PublicGetLyricsBySlugEndpointV1`
already mounts at (`{Public}/{EditorialRouteConstants.Lyrics}`), same pattern
`PublicGetPublishedArticlesEndpointV1` uses for `/articles`:

```csharp
group
    .MapGet(
        "/",
        async (
            [AsParameters] PaginatedRequest paginatedRequest,
            string? search,
            string? language,
            string? sort,
            IDispatcher dispatcher
        ) =>
        {
            var query = new PublicGetPublishedLyricsQuery(paginatedRequest, search, language, sort);
            PublicGetPublishedLyricsResult result = await dispatcher.Send(request: query);
            return Results.Ok(new PublicGetPublishedLyricsResponse(Lyrics: result.Lyrics));
        }
    )
    .WithName(endpointName: PublicGetPublishedLyricsMetaField.GetPublishedLyrics.Name)
    .AllowAnonymous()
    .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
    .Produces<PublicGetPublishedLyricsResponse>(statusCode: StatusCodes.Status200OK)
    .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
```

## 6. Migration

```bash
dotnet ef migrations add AddSlugToLyrics \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

Existing rows have no slug — the migration must backfill one using the exact formula from §1
(`slugify(artist_name || ' ' || song_title || ' lyrics')`, lowercased), suffixed with a short id
fragment on collision, as part of the migration's `Up()`, or as a one-off data-fix script run
before the `NOT NULL` + `UNIQUE` constraints are applied; this repo has no existing precedent for a
backfill-in-migration pattern, so whichever approach is chosen, document it directly in the
migration file's own comment.

## Task checklist

- [x] `LyricsEntity.CategoryId`/`CustomerId`/`OrderItemId` + `CreateFree`/`CreatePaid` factories
  (replacing the single `Draft`-only factory) — full parity with `ArticleEntity`
- [x] `LyricsEntity.Status`/`RejectionReason`/`PublishedAt` + `Submit`/`MarkPendingReview`/`Approve`/
  `Publish`/`Reject`/`Archive` methods, both factories start in `Draft`
- [x] `LyricsErrors.AlreadyPublished`/`AlreadyRejected`/`AlreadySubmitted`/`AlreadyPendingReview`/
  `InvalidStatusTransition` (mirroring `ArticleErrors`'s equivalents)
- [x] `AdminSubmitLyricsCommand`/`Handler` branches `Submit()` vs `MarkPendingReview()` exactly like
  `AdminSubmitArticleHandler`
- [x] Five new admin endpoints: `submit`/`approve`/`publish`/`reject`/`archive`, copy-pasted from
  the `Article` equivalents with `Lyrics` substituted throughout
- [x] Public single-item lookups (`GetLyricsBySlug`, `GetLyricsByVideoId`) gate on
  `Status == Published`, mirroring `PublicGetArticleBySlugHandler` — implemented as an inline
  status check in each handler rather than a separate `LyricsPublishedSpecification` (the
  originally-planned name), since the repository methods return a single nullable entity and the
  handler is the natural place to gate it; admin lookups (`GetByIdAsync`/`GetAllAsync`) stay
  unfiltered by status. **Caught and fixed during verification**: the first implementation pass
  left both handlers ungated (any status was publicly fetchable) — closed before this box was
  checked, with regression tests added for the not-yet-published case on both endpoints.
- [x] `LyricsEntity.Slug` + updated factories/`Update` + `LyricsConfiguration` unique index
- [x] `LyricsErrors.SlugRequired`/`SlugAlreadyExists` + `.resx` entries (all three locales)
- [x] `EditorialValidation.ValidLyricsSlug`
- [x] `Slug` on `LyricsSummaryDto`/`LyricsDetailDto` — the flat `LyricsDto` was replaced by this
  summary/detail split (see below) rather than gaining `Slug` in place
- [x] `LyricsBySlugSpecification` replaces `LyricsBySongAndArtistSpecification`
- [x] `ILyricsRepository.GetBySlugAsync` replaces `GetBySongTitleAndArtistAsync`
- [x] `AdminCreateLyricsCommand`/`Validator`/`Handler` and `AdminUpdateLyrics*` updated for `Slug`
  and its uniqueness check
- [x] `PublicGetLyricsBySlugQuery`/`Handler`/`EndpointV1` changed to single-`{slug}` route
- [x] `LyricsByLanguageSpecification`, `LyricsQueryBuilder.WithLanguage`
- [x] `ILyricsRepository.GetAllAsync` extended with `status`/`categoryId`/`language`/`search`
  filters (implemented as one extended method rather than a separate `GetPublishedAsync` — the
  public list handler always passes `status: Published`, so no second method was needed)
- [x] `PublicGetPublishedLyricsQuery`/`Handler`/`EndpointV1` (new `GET /api/v1/public/lyrics`)
- [x] Migration `AddLyricsSlugCategoryAndEditorialWorkflow` (named for its actual scope), with an
  explicit SQL backfill for both `category_id` (first available category) and `slug` (derived from
  song title + artist + id suffix) ahead of the `NOT NULL`/unique constraints
- [x] Integration tests: slug uniqueness rejected on create/update, by-slug lookup 404s on a
  missing slug, list endpoint's search/language/category filters verified, existing
  `videos/{videoId}` endpoint unregressed
- [x] `AdminGetAllLyricsEndpointV1`'s existing test suite still passes, extended for the new
  required `Slug`/`CategoryId` fields on create/update requests
- [x] Integration + unit tests: a `Draft`/`PendingReview`/`Approved`/`Rejected`/`Archived` lyrics
  record is invisible to both public single-item lookups and the public list endpoint, and becomes
  visible only after `Publish()`; each status transition rejects an out-of-order call with the
  matching `AlreadyX`/`InvalidStatusTransition` error
- [x] Integration tests: creating a lyrics record with no `CustomerId` (the `CreateFree` path)
  succeeds regardless of whether `ArtistId` is set — an artist-linked song is not required to be
  paid, and an unclaimed-artist song is not required to be free, the two are independent concerns;
  `Submit()` on a paid lyrics page (`CreatePaid`) transitions to `PendingPayment`,
  `MarkPendingReview()` on a free one transitions straight to `PendingReview` — matching
  `ArticleEntity`'s own behavior exactly (there is no cross-check against `Category.IsFree` at the
  entity level for articles either — `IsFree` only controls whether the dashboard offers a payment
  step at all, via `category_pricing` rows existing or not)

**Verification, 2026-07-29**: `dotnet build` clean across all projects (0 errors/warnings);
`dotnet test tests/Unit` 6318 passed/3 skipped (pre-existing, unrelated)/0 failed; Lyrics-scoped
runs: 230/230 unit, 92/92 integration (Testcontainers-backed Postgres). Migration generated but
**not applied** to any database (no `dotnet ef database update` run) — review the migration before
deploying.
