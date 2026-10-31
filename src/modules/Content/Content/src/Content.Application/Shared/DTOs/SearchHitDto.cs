using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The engagement figures behind one hit, read from the source row at search time rather
/// than projected, since a like or a share moves them without raising any event the search
/// projection listens for. A figure the hit's type does not keep is null, which is not the
/// same as a type that keeps it and stands at zero.
/// </summary>
/// <param name="LikeCount">Likes, for articles and shorts.</param>
/// <param name="CommentCount">Comments, for articles.</param>
/// <param name="ShareCount">Shares, kept by all three types.</param>
/// <param name="ViewCount">Views, for shorts.</param>
/// <param name="BookmarkCount">Bookmarks, for articles and shorts.</param>
/// <param name="RatingAverage">The mean score, for videos; meaningless without its count.</param>
/// <param name="RatingCount">Ratings cast, for videos.</param>
public sealed record SearchHitEngagementDto(
    int? LikeCount,
    int? CommentCount,
    int? ShareCount,
    int? ViewCount,
    int? BookmarkCount,
    decimal? RatingAverage,
    int? RatingCount
);

/// <summary>
/// One search result, flattened across content types. Type-specific fields are null when
/// they do not apply to the hit's type.
/// </summary>
/// <param name="Type">The content type this hit belongs to.</param>
/// <param name="Id">The entity identifier.</param>
/// <param name="Slug">The URL slug of the entity's detail page.</param>
/// <param name="Title">The entity title.</param>
/// <param name="Excerpt">A plain-text summary, truncated on a word boundary.</param>
/// <param name="ImageUrl">The resolved cover or thumbnail URL, or null when none is set.</param>
/// <param name="CategoryName">The owning category's display name, or null for a type that has none.</param>
/// <param name="PublishedAt">When the entity was published, or first created for a type that has no publication date.</param>
/// <param name="Score">The relevance score; higher ranks first.</param>
/// <param name="CategoryId">The owning category, used to resolve the display name; null for a type that has none.</param>
/// <param name="ImageFileId">The poster file, used to resolve the image URL.</param>
/// <param name="Engagement">The engagement figures the hit's type keeps.</param>
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
    Guid? CategoryId,
    Guid? ImageFileId,
    SearchHitEngagementDto Engagement
);

/// <summary>
/// Hit counts per scope for one query, resolved in the same round trip as the results so
/// the client's scope tabs need no additional request. Keyed by scope rather than one field
/// per content type, so a new searchable type adds an entry and never a member.
/// </summary>
/// <param name="ByScope">The hit count each provider reported, keyed by its scope.</param>
public sealed record SearchCountsDto(IReadOnlyDictionary<EnumSearchScope, int> ByScope)
{
    /// <summary>
    /// The total across every scope.
    /// </summary>
    public int All => ByScope.Values.Sum();

    /// <summary>
    /// The count for one scope, or the total for <see cref="EnumSearchScope.All" />.
    /// Scopes no provider answered read as zero.
    /// </summary>
    /// <param name="scope">The scope to read.</param>
    /// <returns>The hit count.</returns>
    public int For(EnumSearchScope scope) =>
        scope == EnumSearchScope.All ? All
        : ByScope.TryGetValue(scope, out int count) ? count
        : 0;
}
