using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.User.Services;

/// <summary>
/// Identity's avatar workflow: where avatars are stored, how they are replaced, and how they
/// are shaped for the wire. Storage itself belongs to Core, reached through its contract.
/// </summary>
public interface IAvatarService
{
    /// <summary>
    /// Resolves a user's avatar for the wire.
    /// </summary>
    /// <param name="avatarFileId">The avatar in use, or null when there is none.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The avatar DTO, or null.</returns>
    Task<FileDto?> GetAvatarAsync(Guid? avatarFileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads an avatar into the avatars folder, keyed by its owner. Performs no database
    /// work; the asset has no row until <see cref="RecordAsync" /> runs.
    /// </summary>
    /// <param name="avatarFile">The image to upload.</param>
    /// <param name="userId">The owner, used as the storage public id.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unrecorded asset.</returns>
    Task<StoredFile> UploadAsync(IFormFile avatarFile, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches an avatar from a provider URL. Returns null when the avatar in use already comes
    /// from that URL.
    /// </summary>
    /// <param name="currentAvatarFileId">The avatar in use, or null when there is none.</param>
    /// <param name="avatarUrl">The URL to fetch from.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unrecorded asset, or null when no update was needed.</returns>
    Task<StoredFile?> UploadFromUrlAsync(
        Guid? currentAvatarFileId,
        string avatarUrl,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stages the avatar's row and the replacement of the avatar it supersedes. Call inside the
    /// transaction that also writes the user, so both land together.
    /// </summary>
    /// <param name="avatar">The unrecorded asset returned by an upload.</param>
    /// <param name="supersededFileId">The avatar this one replaces, or null when none.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The reference to the recorded avatar.</returns>
    Task<FileReferenceDto> RecordAsync(
        StoredFile avatar,
        Guid? supersededFileId = null,
        CancellationToken cancellationToken = default
    );
}
