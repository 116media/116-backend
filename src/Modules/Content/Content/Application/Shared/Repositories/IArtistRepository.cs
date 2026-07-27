using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for artist profile data access operations.
/// </summary>
public interface IArtistRepository : IRepository<ArtistEntity>
{
    /// <summary>
    /// Retrieves an artist profile by its URL-safe slug. Returns null if not found.
    /// </summary>
    /// <param name="slug">The URL-safe slug of the artist profile.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The artist entity if found, otherwise null.</returns>
    Task<ArtistEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an artist profile by its unique identifier. Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the artist profile.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The artist entity if found, otherwise null.</returns>
    Task<ArtistEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an artist profile by its unique identifier. Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the artist profile.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The artist entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the artist profile is not found.</exception>
    Task<ArtistEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the artist profile claimed by a given identity user. Returns null if the
    /// user has not claimed any profile — needed by the verified-artist fast path.
    /// </summary>
    /// <param name="userId">The identity user UUID to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The claimed artist entity if one exists, otherwise null.</returns>
    Task<ArtistEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of artist profiles with an optional search filter.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term to filter artists by name or bio.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of artist profiles and the total count.</returns>
    Task<(List<ArtistEntity> Artists, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new artist profile to the repository.
    /// </summary>
    Task AddAsync(ArtistEntity artist, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing artist profile as modified.
    /// </summary>
    void Update(ArtistEntity artist);

    /// <summary>
    /// Retrieves the public directory page: artists with surfaceable content, each carrying
    /// its total item count, ordered by folded name. The filter, the count and the folded
    /// ordering all run in a single statement — never one query per row.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="letter">Optional initial-letter bucket, <c>A</c>–<c>Z</c> or <c>#</c>.</param>
    /// <param name="search">Optional accent-insensitive name search term.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the page of directory rows and the total count.</returns>
    Task<(List<ArtistDirectoryRow> Artists, int TotalCount)> GetPublicDirectoryAsync(
        int page,
        int pageSize,
        string? letter,
        string? search,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the distinct initial letters over the same content-filtered set the
    /// directory lists, so the rail never enables a letter that leads to an empty page.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The available letters, sorted ascending.</returns>
    Task<IReadOnlyList<string>> GetAvailableLettersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an artist's per-surface item counts in a single statement, term-for-term
    /// aligned with the directory's content predicate. Feeds the profile's stat row, its
    /// tab-visibility rules, and its 404 rule.
    /// </summary>
    /// <param name="artistId">The artist profile to count for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The per-surface totals.</returns>
    Task<ArtistTotals> GetTotalsAsync(Guid artistId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an artist's social links, ordered by platform so the row is stable across
    /// requests.
    /// </summary>
    /// <param name="artistId">The artist profile to list links for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The artist's social links.</returns>
    Task<IReadOnlyList<ArtistSocialLinkEntity>> GetSocialLinksAsync(
        Guid artistId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the social link for one of an artist's platform slots. Returns null if no
    /// link exists for that platform.
    /// </summary>
    /// <param name="artistId">The artist profile the link belongs to.</param>
    /// <param name="platform">The platform slot to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The social link if found, otherwise null.</returns>
    Task<ArtistSocialLinkEntity?> GetSocialLinkAsync(
        Guid artistId,
        EnumSocialPlatform platform,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new social link to the repository.
    /// </summary>
    Task AddSocialLinkAsync(ArtistSocialLinkEntity link, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing social link as modified.
    /// </summary>
    void UpdateSocialLink(ArtistSocialLinkEntity link);

    /// <summary>
    /// Removes a social link from the repository.
    /// </summary>
    void RemoveSocialLink(ArtistSocialLinkEntity link);
}

/// <summary>
/// One row of the public artist directory: the artist plus its surfaceable item count,
/// which comes from the same projection as the directory filter and has nowhere to live on
/// the entity itself.
/// </summary>
/// <param name="Artist">The artist profile.</param>
/// <param name="ContentCount">The artist's total item count across every profile surface.</param>
public record ArtistDirectoryRow(ArtistEntity Artist, int ContentCount);

/// <summary>
/// An artist's per-surface item counts. The sum of all five is the profile's 404 predicate:
/// zero everywhere means the profile is not served.
/// </summary>
/// <param name="Songs">Published lyrics pages where this artist is the primary credit.</param>
/// <param name="Videos">Published videos where this artist is the primary credit.</param>
/// <param name="Albums">Releases typed <c>Album</c> linked to this artist.</param>
/// <param name="Mixtapes">Releases typed <c>Mixtape</c> linked to this artist.</param>
/// <param name="News">Published articles tagged to this artist.</param>
public record ArtistTotals(int Songs, int Videos, int Albums, int Mixtapes, int News);
