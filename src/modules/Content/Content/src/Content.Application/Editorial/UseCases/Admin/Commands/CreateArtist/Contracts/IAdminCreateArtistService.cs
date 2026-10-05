using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.Contracts;

/// <summary>
/// Resolves and applies an artist creation: validates the slug, builds the aggregate against the
/// clock and stages it.
/// </summary>
public interface IAdminCreateArtistService
{
    /// <summary>
    /// Stages the new artist, throwing the localized error when the slug is taken. The caller
    /// owns the commit.
    /// </summary>
    /// <param name="command">The creation command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The staged artist.</returns>
    Task<ArtistEntity> CreateAsync(AdminCreateArtistCommand command, CancellationToken cancellationToken);
}
