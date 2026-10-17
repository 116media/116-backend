# Contracts and DTOs

## Checklist

- [ ] `EnumSearchScope` in `Content.Domain/Enums/`
- [ ] `SearchHitDto` in `Content.Application/Shared/DTOs/`
- [ ] `SearchCountsDto` in the same folder
- [ ] Both registered in the OpenAPI schema through the endpoint's `Produces`

---

## `EnumSearchScope`

```csharp
namespace _116.Content.Domain.Enums;

/// <summary>
/// The content types a public search can be narrowed to.
/// </summary>
public enum EnumSearchScope
{
    /// <summary>
    /// Every searchable content type, ranked together.
    /// </summary>
    All,

    /// <summary>
    /// Published articles only.
    /// </summary>
    Articles,

    /// <summary>
    /// Published videos only.
    /// </summary>
    Videos,

    /// <summary>
    /// Active short videos only.
    /// </summary>
    Shorts,
}
```

Shorts were added after the first release of this endpoint. **Lyrics and artists are the
remaining work**: they are deliberately absent for now because adding an enum member the
API cannot serve would publish a contract the backend does not honour. Each needs a
`search_vector` on its table, a partial index, a read view, a scope member, a repository
arm and a branch in the handler's merge, exactly as shorts did.

Shorts differ from articles and videos in three ways the DTO has to carry:

| | Articles / videos | Shorts |
| --- | --- | --- |
| Visibility | `status = 'Published'` | `is_active` — shorts have no editorial workflow |
| Searchable text | title, headline/description, meta fields, body | title only |
| Category | always set | none; `CategoryId` and `CategoryName` come back null |

A short also has no publication date, so `PublishedAt` carries its `created_at`, which is
what the feed orders on anyway.

---

## `SearchHitDto`

One flat record for every content type, with a discriminator and nullable type-specific
fields. A discriminated union was considered and rejected: it forces every client into a
switch for what is one row component with guards.

```csharp
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// One search result, flattened across content types. Type-specific fields are null
/// when they do not apply to <paramref name="Type" />.
/// </summary>
/// <param name="Type">The content type this hit belongs to.</param>
/// <param name="Id">The entity identifier.</param>
/// <param name="Slug">The URL slug of the entity's detail page.</param>
/// <param name="Title">The entity title.</param>
/// <param name="Excerpt">A plain-text summary, truncated server-side on a word boundary.</param>
/// <param name="ImageUrl">The resolved cover or thumbnail URL, or null when none is set.</param>
/// <param name="CategoryName">The owning category's display name, or null for a type that has none.</param>
/// <param name="PublishedAt">When the entity was published, or first created for a type that has no publication date.</param>
/// <param name="Score">The relevance score; higher ranks first.</param>
/// <param name="RatingAverage">The cached average rating, videos only.</param>
public record SearchHitDto(
    EnumCoreContentType Type,
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    string? ImageUrl,
    string? CategoryName,
    DateTimeOffset? PublishedAt,
    double Score,
    decimal? RatingAverage
);
```

A duration field was specified here originally and has been removed: videos carry no
duration column, nothing in the frontend reads one, and the helper the spec called for
does not exist. Adding it later is additive, the same argument this spec makes for the
lyrics and artists scopes.

`RatingAverage` is `decimal?` to match `VideoEntity.RatingAverage`, so no precision is
lost crossing the boundary. `Score` stays `double`: `ts_rank_cd` returns `real`.

`Type` reuses the existing `EnumCoreContentType` rather than inventing a parallel enum,
so a hit's type is the same value the rest of the system uses for content types.

### Excerpt truncation

200 characters, cut on a word boundary, no ellipsis added by the server (the client owns
presentation). A shared helper rather than per-entity duplication:

```csharp
namespace _116.Content.Application.Shared.Helpers;

/// <summary>
/// Trims plain text to a maximum length without splitting a word.
/// </summary>
public static class ExcerptHelper
{
    private const int MaxLength = 200;

    /// <summary>
    /// Returns the text cut to at most 200 characters, ending on a word boundary.
    /// </summary>
    /// <param name="text">The source text; null or whitespace yields an empty string.</param>
    /// <returns>The truncated text.</returns>
    public static string Truncate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string trimmed = text.Trim();

        if (trimmed.Length <= MaxLength)
        {
            return trimmed;
        }

        int boundary = trimmed.LastIndexOf(' ', MaxLength);

        return boundary <= 0 ? trimmed[..MaxLength] : trimmed[..boundary];
    }
}
```

---

## `SearchCountsDto`

```csharp
namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Hit counts per scope for one query, resolved in the same round trip as the results
/// so the client's scope tabs need no additional request.
/// </summary>
/// <param name="All">Total across every scope.</param>
/// <param name="Articles">Matching published articles.</param>
/// <param name="Videos">Matching published videos.</param>
/// <param name="Shorts">Matching active short videos.</param>
public record SearchCountsDto(int All, int Articles, int Videos, int Shorts);
```

`All` is stored rather than derived so a future scope cannot silently break the sum on
clients that compute it themselves.

---

## Result record

Lives beside the query, per the module's use-case convention:

```csharp
/// <summary>
/// Result of the <see cref="PublicSearchQuery" />: the ranked hits for the active
/// scope plus the per-scope counts for the whole query.
/// </summary>
/// <param name="Query">The trimmed query that was run, echoed for client display.</param>
/// <param name="Scope">The scope the results were narrowed to.</param>
/// <param name="Counts">Hit counts per scope.</param>
/// <param name="Results">The paginated hits for <paramref name="Scope" />.</param>
public record PublicSearchResult(
    string Query,
    EnumSearchScope Scope,
    SearchCountsDto Counts,
    PaginatedResult<SearchHitDto> Results
);
```
