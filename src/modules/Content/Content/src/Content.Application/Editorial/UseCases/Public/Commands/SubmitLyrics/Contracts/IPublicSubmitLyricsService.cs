using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics.Contracts;

/// <summary>
/// Resolves the submitter's own artist profile and, when one exists, publishes the lyrics
/// directly under it instead of queueing them for moderation.
/// </summary>
public interface IPublicSubmitLyricsService
{
    /// <summary>
    /// Finds the artist profile the submitter owns, strictly by user id.
    /// </summary>
    /// <param name="userId">The submitter.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<ArtistEntity?> FindOwnedArtistAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Stages a published lyrics page under the owned artist, throwing the localized error when
    /// the slug is missing or taken or no default category is configured. The caller owns the commit.
    /// </summary>
    /// <param name="command">The submission command.</param>
    /// <param name="ownedArtist">The submitter's own artist profile.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The staged lyrics.</returns>
    Task<LyricsEntity> CreateForArtistAsync(
        PublicSubmitLyricsCommand command,
        ArtistEntity ownedArtist,
        CancellationToken cancellationToken
    );
}
