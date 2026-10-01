using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.Contracts;

/// <summary>
/// Resolves and applies a lyrics creation: validates the category, the slug and the linked video,
/// builds the aggregate and stages it.
/// </summary>
public interface IAdminCreateLyricsService
{
    /// <summary>
    /// Stages the new lyrics, throwing the localized error when the category or video is missing
    /// or the slug is taken. The caller owns the commit.
    /// </summary>
    /// <param name="command">The creation command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The staged lyrics.</returns>
    Task<LyricsEntity> CreateAsync(AdminCreateLyricsCommand command, CancellationToken cancellationToken);
}
