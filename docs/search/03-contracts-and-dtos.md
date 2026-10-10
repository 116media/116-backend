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
}
```

Lyrics and artists are deliberately absent: adding an enum member the API cannot serve
would publish a contract the backend does not honour. They are added when their
listing endpoints exist.

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
/// <param name="CategoryName">The owning category's display name.</param>
/// <param name="PublishedAt">When the entity was published.</param>
/// <param name="Score">The relevance score; higher ranks first.</param>
/// <param name="RatingAverage">The cached average rating, videos only.</param>
/// <param name="DurationSeconds">The runtime in seconds, videos only.</param>
public record SearchHitDto(
    EnumCoreContentType Type,
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    string? ImageUrl,
    string CategoryName,
    DateTimeOffset? PublishedAt,
    double Score,
    double? RatingAverage,
    int? DurationSeconds
);
```

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
public record SearchCountsDto(int All, int Articles, int Videos);
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
