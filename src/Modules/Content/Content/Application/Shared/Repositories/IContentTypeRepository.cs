using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing content-type lookup entities.
/// </summary>
public interface IContentTypeRepository
{
    /// <summary>
    /// Stages a new content type for insertion on the next commit.
    /// </summary>
    /// <param name="contentType">The content type to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(ContentTypeEntity contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tracked content type by id for mutation.
    /// </summary>
    /// <param name="id">The content type identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tracked content type.</returns>
    /// <exception cref="NotFoundException">Thrown when no content type has the supplied id.</exception>
    Task<ContentTypeEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether a content type already carries the supplied name.
    /// </summary>
    /// <param name="name">The name to probe.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the name is taken.</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads every content type, optionally narrowed by a free-text search term.
    /// </summary>
    /// <param name="search">Free-text term, or null for the full list.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The content types ordered by name.</returns>
    Task<IReadOnlyList<ContentTypeEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Reads the content types available for category assignment.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The active content types ordered by name.</returns>
    Task<IReadOnlyList<ContentTypeEntity>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages an existing content type for update on the next commit.
    /// </summary>
    /// <param name="contentType">The content type to update.</param>
    void Update(ContentTypeEntity contentType);
}
