using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing tag entities and their content associations.
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Stages a new tag for insertion on the next commit.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(TagEntity tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tracked tag by id for mutation.
    /// </summary>
    /// <param name="id">The tag identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tracked tag.</returns>
    /// <exception cref="NotFoundException">Thrown when no tag has the supplied id.</exception>
    Task<TagEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a tag for deletion on the next commit.
    /// </summary>
    /// <param name="entity">The tag to remove.</param>
    void Remove(TagEntity entity);

    /// <summary>
    /// Reads a tag by its URL slug, or null when none exists.
    /// </summary>
    /// <param name="slug">The tag slug.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tag, or null.</returns>
    Task<TagEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a tag by name using a case-insensitive match, or null when none exists.
    /// </summary>
    /// <param name="name">The tag name.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tag, or null.</returns>
    Task<TagEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the tags matching the supplied names in one round trip, keyed by lower-cased name.
    /// </summary>
    /// <param name="names">The tag names to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The found tags keyed by lower-cased name; missing names are absent.</returns>
    Task<IReadOnlyDictionary<string, TagEntity>> GetByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Reads all tags, optionally filtered by search term and content-type association, and
    /// optionally limited.
    /// </summary>
    /// <param name="search">Free-text term matching name or slug, or null for no filter.</param>
    /// <param name="contentType">Restricts to tags used by the supplied content type when set.</param>
    /// <param name="limit">Maximum number of tags to return, or null for all.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <param name="publishedOnly">
    /// Counts published content only when true, every association when false.
    /// </param>
    /// <returns>The matching tags ordered by name, each with its content count.</returns>
    Task<IReadOnlyList<TagCountRow>> GetAllAsync(
        string? search = null,
        EnumCoreContentType? contentType = null,
        int? limit = null,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Reads the most-used tags, optionally scoped to a content type.
    /// </summary>
    /// <param name="limit">Maximum number of tags to return, or null for all.</param>
    /// <param name="contentType">Restricts the ranking to one content type when set.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <param name="publishedOnly">
    /// Ranks and counts published content only when true, every association when false.
    /// </param>
    /// <returns>The tags ordered by usage count descending, each with that count.</returns>
    Task<IReadOnlyList<TagCountRow>> GetPopularAsync(
        int? limit,
        EnumCoreContentType? contentType = null,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stages an existing tag for update on the next commit.
    /// </summary>
    /// <param name="tag">The tag to update.</param>
    void Update(TagEntity tag);

    /// <summary>
    /// Resolves the tags the given ids reference, in one query, keyed by id.
    /// Missing ids are simply absent from the result.
    /// </summary>
    /// <param name="ids">The identifiers to resolve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<IReadOnlyDictionary<Guid, TagEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// One tag of a listing together with how much content of each kind carries it, counted in
/// the same statement as the tag itself so a page of tags never becomes a query per tag.
/// </summary>
/// <param name="Tag">The tag.</param>
/// <param name="Articles">Articles carrying it, within the caller's scope.</param>
/// <param name="Videos">Videos carrying it, within the caller's scope.</param>
/// <param name="Lyrics">Lyrics pages carrying it, within the caller's scope.</param>
public record TagCountRow(TagEntity Tag, int Articles, int Videos, int Lyrics);
